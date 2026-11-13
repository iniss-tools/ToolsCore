using System.Runtime.CompilerServices;

namespace ToolsCore.Tools;

/// <summary>
/// Roh medzi posuvnikmi <see cref="DataGridView" />: posuvniky su samostatne okna s temou Windows, roh vsak kresli tabulka
/// sama systemovou farbou ovladacich prvkov este pred udalostou <see cref="Control.Paint" />. Pri tmavych posuvnikoch by
/// svietil ako svetly stvorcek, preto ho obsluha Paint prekresli farbou drahy tmaveho posuvnika.
/// </summary>
internal static class GridScrollCorner
{
    /// <summary>
    /// Draha posuvnika temy Windows DarkMode_Explorer.
    /// </summary>
    internal static readonly Color DarkTrack = Color.FromArgb(23, 23, 23);

    // farba rohu tabulky; prazdna = systemova (tabulka bez tmavych posuvnikov)
    private static readonly ConditionalWeakTable<DataGridView, StrongBox<Color>> Colors = new();

    /// <summary>
    /// Nastavi farbu rohu podla posuvnikov tabulky (pri kazdej zmene temy).
    /// </summary>
    /// <param name="grid">tabulka</param>
    /// <param name="darkScrollBars">tabulka ma tmave posuvniky</param>
    public static void Apply(DataGridView grid, bool darkScrollBars)
    {
        var color = darkScrollBars ? DarkTrack : Color.Empty;
        if (Colors.TryGetValue(grid, out var box))
        {
            if (box.Value == color)
                return;
            box.Value = color;
        }
        else
        {
            if (!darkScrollBars)
                return;
            Colors.Add(grid, new StrongBox<Color>(color));
            grid.Paint += Grid_Paint;
        }

        if (Corner(grid) is { } corner)
            grid.Invalidate(corner);
    }

    /// <summary>
    /// Roh medzi vodorovnym a zvislym posuvnikom; <see langword="null" />, ked nie su viditelne oba.
    /// </summary>
    internal static Rectangle? Corner(DataGridView grid)
    {
        Control? horizontal = null, vertical = null;
        foreach (Control control in grid.Controls)
        {
            if (control is HScrollBar { Visible: true })
                horizontal = control;
            else if (control is VScrollBar { Visible: true })
                vertical = control;
        }

        return horizontal is null || vertical is null ? null : new Rectangle(vertical.Left, horizontal.Top, vertical.Width, horizontal.Height);
    }

    private static void Grid_Paint(object? sender, PaintEventArgs e)
    {
        if (sender is not DataGridView grid || !Colors.TryGetValue(grid, out var box) || box.Value.IsEmpty)
            return;

        if (Corner(grid) is { } corner && e.ClipRectangle.IntersectsWith(corner))
        {
            using var brush = new SolidBrush(box.Value);
            e.Graphics.FillRectangle(brush, corner);
        }
    }
}
