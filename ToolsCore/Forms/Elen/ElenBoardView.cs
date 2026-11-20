using System.Drawing.Drawing2D;
using ToolsCore.Iniss.Elen;

namespace ToolsCore.Forms.Elen;

/// <summary>
/// Bodova tabula ELEN: body riadku textu (<see cref="ElenTextRaster" />) ako svietiace a zhasnute LED na tmavom
/// paneli. Velkost bodu sa riadi velkostou prvku; tabula je vzdy tmava aj vo svetlej teme ako skutocny panel.
/// Pouziva ju nahlad pisma v GVDEditore aj nahlad na tabuli v ElenFontEditore.
/// </summary>
public class ElenBoardView : Control
{
    private static readonly Color Board = Color.FromArgb(10, 11, 12);
    private static readonly Color Off = Color.FromArgb(28, 31, 34);
    private static readonly Color Missing = Color.FromArgb(255, 140, 0);

    // farby 0 bez farby (predvolena farba tabule), 1 cervena, 2 zelena, 3 zlta
    private static readonly Color[] Lit =
        [Color.FromArgb(241, 236, 226), Color.FromArgb(255, 74, 61), Color.FromArgb(63, 224, 123), Color.FromArgb(255, 210, 58)];

    private readonly System.Windows.Forms.Timer _blink = new() { Interval = 550 };
    private ElenTextRaster? _raster;
    private int _boardRows;
    private int _boardColumns;
    private int _dotColor;
    private bool _blinks;
    private bool _blinkOff;
    private int _highlightCode = -1;
    private bool _markMissing;
    private int _maxPitch;
    private int _textLeft = 1;

    /// <summary>
    /// Vytvori prazdnu tabulu.
    /// </summary>
    public ElenBoardView()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint |
                 ControlStyles.ResizeRedraw, true);
        _blink.Tick += (_, _) =>
        {
            _blinkOff = !_blinkOff;
            Invalidate();
        };
    }

    /// <summary>
    /// Klik na znak textu - poradie znaku v <see cref="ElenTextRaster.Glyphs" />.
    /// </summary>
    public event EventHandler<int>? GlyphClick;

    /// <summary>
    /// Farba svietiaceho bodu pre farbu pisma ELEN (0 bez farby, 1 cervena, 2 zelena, 3 zlta).
    /// </summary>
    public static Color LitColor(int color) => Lit[Math.Clamp(color, 0, Lit.Length - 1)];

    /// <summary>
    /// Zobrazovany riadok textu; null = prazdna tabula.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ElenTextRaster? Raster
    {
        get => _raster;
        set
        {
            _raster = value;
            Invalidate();
        }
    }

    /// <summary>
    /// Pocet riadkov bodov tabule; 0 = vyska textu.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int BoardRows
    {
        get => _boardRows;
        set => Set(ref _boardRows, Math.Max(0, value));
    }

    /// <summary>
    /// Pocet stlpcov bodov tabule; 0 = kolko sa zmesti do sirky prvku pri velkosti bodu podla vysky.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int BoardColumns
    {
        get => _boardColumns;
        set => Set(ref _boardColumns, Math.Max(0, value));
    }

    /// <summary>
    /// Farba pisma ELEN (0 bez farby, 1 cervena, 2 zelena, 3 zlta).
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int DotColor
    {
        get => _dotColor;
        set => Set(ref _dotColor, Math.Clamp(value, 0, Lit.Length - 1));
    }

    /// <summary>
    /// Text blika.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool Blinks
    {
        get => _blinks;
        set
        {
            _blinks = value;
            UpdateBlink();
            Invalidate();
        }
    }

    /// <summary>
    /// Kod znaku, ktory sa v texte oramuje vsade, kde je (napr. upravovany znak); -1 = ziadny.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int HighlightCode
    {
        get => _highlightCode;
        set => Set(ref _highlightCode, value);
    }

    /// <summary>
    /// Oznacit znaky, ktore v pisme nie su (sirka 0), zvislou ciarou na ich mieste.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool MarkMissing
    {
        get => _markMissing;
        set
        {
            _markMissing = value;
            Invalidate();
        }
    }

    /// <summary>
    /// Najvacsia vzdialenost bodov v logickych bodoch (96 DPI); 0 = bez obmedzenia.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int MaxPitch
    {
        get => _maxPitch;
        set => Set(ref _maxPitch, Math.Max(0, value));
    }

    /// <summary>
    /// Stlpec tabule, od ktoreho sa kresli text.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int TextLeft
    {
        get => _textLeft;
        set => Set(ref _textLeft, Math.Max(0, value));
    }

    /// <summary>Farba oramovania znakov <see cref="HighlightCode" />.</summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color HighlightColor { get; set; } = Color.FromArgb(0, 120, 215);

    private void Set(ref int field, int value)
    {
        if (field == value)
            return;
        field = value;
        Invalidate();
    }

    /// <inheritdoc />
    protected override void OnVisibleChanged(EventArgs e)
    {
        base.OnVisibleChanged(e);
        UpdateBlink();
    }

    private void UpdateBlink()
    {
        var blink = _blinks && Visible && !DesignMode;
        _blink.Enabled = blink;
        if (!blink)
            _blinkOff = false;
    }

    /// <summary>
    /// Rozlozenie tabule v prvku: vzdialenost bodov, pocet riadkov a stlpcov a lavy horny roh.
    /// </summary>
    private (int Pitch, int Rows, int Columns, int Left, int Top) BoardLayout()
    {
        var margin = LogicalToDeviceUnits(2);
        var width = Math.Max(1, ClientSize.Width - 2 * margin);
        var height = Math.Max(1, ClientSize.Height - 2 * margin);
        var rows = _boardRows > 0 ? _boardRows : Math.Max(1, _raster?.Height ?? 1);

        var pitch = height / rows;
        if (_boardColumns > 0)
            pitch = Math.Min(pitch, width / _boardColumns);
        if (_maxPitch > 0)
            pitch = Math.Min(pitch, LogicalToDeviceUnits(_maxPitch));
        pitch = Math.Max(2, pitch);

        var columns = _boardColumns > 0 ? _boardColumns : Math.Max(1, width / pitch);
        var left = (ClientSize.Width - columns * pitch) / 2;
        var top = (ClientSize.Height - rows * pitch) / 2;
        return (pitch, rows, columns, left, top);
    }

    /// <inheritdoc />
    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Board);
        var (pitch, rows, columns, left, top) = BoardLayout();

        using var on = new SolidBrush(_blinkOff ? Off : Lit[_dotColor]);
        using var off = new SolidBrush(Off);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var size = pitch * 0.74f;
        var inset = (pitch - size) / 2;
        for (var y = 0; y < rows; y++)
        {
            for (var x = 0; x < columns; x++)
            {
                var lit = _raster?.IsSet(x - _textLeft, y) == true;
                g.FillEllipse(lit ? on : off, left + x * pitch + inset, top + y * pitch + inset, size, size);
            }
        }

        g.SmoothingMode = SmoothingMode.None;
        if (_raster is null)
            return;

        var bottom = top + rows * pitch;
        if (_markMissing)
        {
            using var missing = new Pen(Missing, Math.Max(1, pitch / 4));
            foreach (var glyph in _raster.Glyphs.Where(glyph => glyph.IsMissing))
            {
                var x = left + (_textLeft + glyph.X) * pitch;
                if (x <= left + columns * pitch)
                    g.DrawLine(missing, x, top, x, bottom - 1);
            }
        }

        if (_highlightCode < 0)
            return;

        using var pen = new Pen(HighlightColor, Math.Max(1, LogicalToDeviceUnits(2)));
        foreach (var glyph in _raster.Glyphs.Where(glyph => glyph.Code == _highlightCode && !glyph.IsMissing))
        {
            var x = left + (_textLeft + glyph.X) * pitch;
            g.DrawRectangle(pen, x, top, glyph.Width * pitch - 1, rows * pitch - 1);
        }
    }

    /// <inheritdoc />
    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        if (_raster is null || e.Button != MouseButtons.Left)
            return;

        var (pitch, _, _, left, _) = BoardLayout();
        var column = (e.X - left) / pitch - _textLeft;
        if (e.X >= left && _raster.GlyphAt(column) is var index and >= 0)
            GlyphClick?.Invoke(this, index);
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _blink.Dispose();
        base.Dispose(disposing);
    }
}
