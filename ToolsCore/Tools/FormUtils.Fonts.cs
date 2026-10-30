using System.Runtime.CompilerServices;

namespace ToolsCore.Tools;

public static partial class FormUtils
{
    private const float MinTableFontSize = 6;
    private const float MaxTableFontSize = 48;

    // povodna vyska riadka tabulky (z navrhara) a zvacsenie pisma Ctrl + koliesko - plati len do zatvorenia okna
    private static readonly ConditionalWeakTable<DataGridView, GridFontState> GridFonts = new();

    private sealed class GridFontState(int designedRowHeight)
    {
        public int DesignedRowHeight { get; } = designedRowHeight;
        public float Zoom { get; set; }
        public Font? ZoomedCells { get; set; }
        public Font? ZoomedHeaders { get; set; }
    }

    /// <summary>
    /// Po zmene pisiem v nastaveniach ich prevezmu tabulky vo vsetkych otvorenych oknach (editory su modalne okna
    /// a pismo si beru pri otvoreni). Pismo rozhrania meni rozlozenie okien, preto plati az po restarte.
    /// </summary>
    public static void RefreshFonts()
    {
        foreach (var form in Application.OpenForms.Cast<Form>().ToList())
            RefreshTableFonts(form.Controls);
    }

    private static void RefreshTableFonts(Control.ControlCollection controls)
    {
        foreach (Control control in controls)
        {
            if (control is DataGridView dgv)
                ApplyTableFont(dgv);
            RefreshTableFonts(control.Controls);
        }
    }

    /// <summary>
    /// Pismo buniek a hlaviciek tabulky z nastaveni (so zvacsenim Ctrl + koliesko) a vyska riadkov podla neho.
    /// </summary>
    private static void ApplyTableFont(DataGridView dgv)
    {
        if (!GridFonts.TryGetValue(dgv, out var state))
        {
            state = new GridFontState(dgv.RowTemplate.Height);
            GridFonts.Add(dgv, state);
            dgv.MouseWheel += Grid_MouseWheel;
            dgv.Disposed += (_, _) => DisposeZoomedFonts(state);
        }

        var fonts = GlobSettings.Fonts;
        Font cells = fonts.Cells, headers = fonts.Headers;
        Font? oldCells = state.ZoomedCells, oldHeaders = state.ZoomedHeaders;
        if (state.Zoom != 0)
        {
            cells = state.ZoomedCells = Zoomed(cells, state.Zoom);
            headers = state.ZoomedHeaders = Zoomed(headers, state.Zoom);
        }
        else
        {
            state.ZoomedCells = state.ZoomedHeaders = null;
        }

        dgv.ColumnHeadersDefaultCellStyle.Font = headers;
        dgv.DefaultCellStyle.Font = cells;
        oldCells?.Dispose();
        oldHeaders?.Dispose();

        // riadky s pevnou vyskou: aspon vyska z navrhu, s vacsim pismom vyssie
        var height = Math.Max(state.DesignedRowHeight, cells.Height + dgv.LogicalToDeviceUnits(8));
        if (dgv.AutoSizeRowsMode != DataGridViewAutoSizeRowsMode.None || dgv.RowTemplate.Height == height)
            return;

        dgv.RowTemplate.Height = height;
        foreach (DataGridViewRow row in dgv.Rows)
            row.Height = height;
    }

    private static Font Zoomed(Font font, float zoom) =>
        new(font.FontFamily, Math.Clamp(font.SizeInPoints + zoom, MinTableFontSize, MaxTableFontSize), font.Style, GraphicsUnit.Point);

    private static void DisposeZoomedFonts(GridFontState state)
    {
        state.ZoomedCells?.Dispose();
        state.ZoomedHeaders?.Dispose();
    }

    /// <summary>
    /// Ctrl + koliesko zvacsi alebo zmensi pismo tabulky (nastavenie sa nemeni).
    /// </summary>
    private static void Grid_MouseWheel(object? sender, MouseEventArgs e)
    {
        if (Control.ModifierKeys != Keys.Control || sender is not DataGridView dgv || !GridFonts.TryGetValue(dgv, out var state))
            return;

        // bez posunu tabulky
        if (e is HandledMouseEventArgs handled)
            handled.Handled = true;

        var size = GlobSettings.Fonts.Cells.SizeInPoints;
        var zoom = Math.Clamp(state.Zoom + Math.Sign(e.Delta), MinTableFontSize - size, MaxTableFontSize - size);
        if (zoom == state.Zoom)
            return;

        state.Zoom = zoom;
        ApplyTableFont(dgv);
    }
}
