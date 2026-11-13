using System.Globalization;

namespace ToolsCore.Forms.Calendar;

/// <summary>
/// Pruh kalendara variant vlaku: pre kazdu variantu riadok dni obdobia, dni jazdy farebne a dni, v ktore ide
/// viac variant naraz, cervene. Nad riadkami su mesiace. Bublina ukaze datum a varianty, ktore v den idu. Riadok
/// <see cref="CalendarStripRow.IsOwn" /> (upravovana varianta) ma tucny popis a vlastnu farbu.
/// </summary>
public sealed class CalendarStrip : Control
{
    private const int HeaderHeight = 18;
    private const int RowHeight = 16;
    private const int LabelWidth = 44;

    private readonly ToolTip _toolTip = new();
    private CalendarStripData? _calendar;
    private int _hoverDay = -1;

    /// <summary>
    /// Vytvori prazdny pruh.
    /// </summary>
    public CalendarStrip()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                 ControlStyles.UserPaint, true);
        Height = HeaderHeight + RowHeight + 2;
    }

    /// <summary>
    /// Text popisu riadku (napr. „1/3“) podla riadku kalendara.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Func<CalendarStripRow, string> RowLabel { get; set; } = row => $"{row.Position}";

    /// <summary>
    /// Text bubliny pre den: datum a popisy variant, ktore v nom idu.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Func<DateTime, IReadOnlyList<CalendarStripRow>, string> DayToolTip { get; set; } =
        (date, _) => date.ToString("d", CultureInfo.CurrentCulture);

    /// <summary>
    /// Poradie varianty vybranej v tabulke (jej riadok ma ramik); 0 = ziadna.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int SelectedPosition
    {
        get;
        set
        {
            if (field == value)
                return;
            field = value;
            Invalidate();
        }
    }

    /// <summary>
    /// Zobrazi kalendar a prisposobi vysku poctu variant.
    /// </summary>
    public void SetCalendar(CalendarStripData? calendar)
    {
        _calendar = calendar;
        Height = HeaderHeight + RowHeight * Math.Max(1, calendar?.Rows.Count ?? 1) + 2;
        Invalidate();
    }

    private bool Dark => BackColor.GetBrightness() < 0.5f;

    private RectangleF DaysArea => new(LabelWidth, HeaderHeight, Math.Max(1, Width - LabelWidth - 2), Height - HeaderHeight - 2);

    /// <inheritdoc />
    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(BackColor);
        if (_calendar is not { Days: > 0 } calendar)
            return;

        var area = DaysArea;
        var dayWidth = area.Width / calendar.Days;
        var dark = Dark;
        var grid = dark ? Color.FromArgb(70, 70, 70) : Color.FromArgb(215, 215, 215);
        // draha riadku musi byt viditelna aj pri vlaku, ktory ide len par dni
        var track = dark ? Color.FromArgb(58, 58, 58) : Color.FromArgb(226, 226, 226);
        var trackBorder = dark ? Color.FromArgb(85, 85, 85) : Color.FromArgb(200, 200, 200);
        var own = dark ? Color.FromArgb(80, 160, 240) : Color.FromArgb(0, 110, 200);
        var other = dark ? Color.FromArgb(115, 130, 150) : Color.FromArgb(140, 158, 182);
        var overlap = dark ? Color.FromArgb(245, 95, 95) : Color.FromArgb(210, 30, 45);
        var selection = dark ? Color.FromArgb(255, 200, 80) : Color.FromArgb(230, 140, 0);

        using var gridPen = new Pen(grid);
        using var trackPen = new Pen(trackBorder);
        using var selectionPen = new Pen(selection, 2);
        using var textBrush = new SolidBrush(ForeColor);
        using var trackBrush = new SolidBrush(track);
        using var ownBrush = new SolidBrush(own);
        using var otherBrush = new SolidBrush(other);
        using var overlapBrush = new SolidBrush(overlap);
        var format = new StringFormat { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.None };

        // mesiace
        for (var day = 0; day < calendar.Days; day++)
        {
            var date = calendar.From.AddDays(day);
            if (day != 0 && date.Day != 1)
                continue;

            var x = area.Left + day * dayWidth;
            g.DrawLine(gridPen, x, 0, x, Height);
            var monthName = date.ToString(dayWidth * 31 >= 40 ? "MMM" : "%M", CultureInfo.CurrentCulture);
            g.DrawString(monthName, Font, textBrush, new RectangleF(x + 2, 0, dayWidth * 31, HeaderHeight), format);
        }

        const int barTop = 2, barHeight = RowHeight - 4;
        for (var r = 0; r < calendar.Rows.Count; r++)
        {
            var row = calendar.Rows[r];
            var top = area.Top + r * RowHeight;
            using var labelFont = row.IsOwn ? new Font(Font, FontStyle.Bold) : null;
            g.DrawString(RowLabel(row), labelFont ?? Font, textBrush, new RectangleF(2, top, LabelWidth - 4, RowHeight), format);
            g.FillRectangle(trackBrush, area.Left, top + barTop, area.Width, barHeight);
            g.DrawRectangle(trackPen, area.Left, top + barTop, area.Width - 1, barHeight - 1);

            // dni jazdy vo farbe riadku, prekrytie cerveno v strede - vidno oboje; suvisle useky maju aspon
            // MinSegment bodov, aby bol vidiet aj jediny den celorocneho obdobia
            var runBrush = row.IsOwn ? ownBrush : otherBrush;
            foreach (var (start, length) in Segments(day => row.Runs[day], calendar.Days))
                FillSegment(g, runBrush, area.Left, dayWidth, start, length, top + barTop, barHeight);

            foreach (var (start, length) in Segments(day => row.Runs[day] && calendar.IsOverlap(day), calendar.Days))
                FillSegment(g, overlapBrush, area.Left, dayWidth, start, length, top + barTop + barHeight / 4f, barHeight / 2f);

            if (row.Position == SelectedPosition)
                g.DrawRectangle(selectionPen, 1, top + 1, Width - 3, RowHeight - 2);
        }

        if (_hoverDay >= 0 && _hoverDay < calendar.Days)
        {
            var x = area.Left + _hoverDay * dayWidth;
            g.DrawRectangle(Pens.Gray, x, area.Top, Math.Max(1f, dayWidth), area.Height - 1);
        }
    }

    private const float MinSegment = 4f;

    /// <summary>
    /// Suvisle useky dni, pre ktore plati <paramref name="test" />: (prvy den, pocet dni).
    /// </summary>
    private static IEnumerable<(int Start, int Length)> Segments(Func<int, bool> test, int days)
    {
        var start = -1;
        for (var day = 0; day <= days; day++)
        {
            var on = day < days && test(day);
            if (on && start < 0)
                start = day;
            else if (!on && start >= 0)
            {
                yield return (start, day - start);
                start = -1;
            }
        }
    }

    // usek dni, kratky usek sa rozsiri na MinSegment bodov okolo svojho stredu
    private static void FillSegment(Graphics g, Brush brush, float left, float dayWidth, int start, int length, float top, float height)
    {
        var x = left + start * dayWidth;
        var width = length * dayWidth;
        if (width < MinSegment)
        {
            x -= (MinSegment - width) / 2;
            width = MinSegment;
        }

        g.FillRectangle(brush, x, top, width, height);
    }

    /// <inheritdoc />
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_calendar is not { Days: > 0 } calendar)
            return;

        var area = DaysArea;
        var day = e.X < area.Left ? -1 : (int)((e.X - area.Left) / (area.Width / calendar.Days));
        if (day >= calendar.Days)
            day = -1;
        if (day == _hoverDay)
            return;

        _hoverDay = day;
        Invalidate();
        if (day < 0)
        {
            _toolTip.SetToolTip(this, null);
            return;
        }

        var running = calendar.Rows.Where(row => row.Runs[day]).ToList();
        _toolTip.SetToolTip(this, DayToolTip(calendar.From.AddDays(day), running));
    }

    /// <inheritdoc />
    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _hoverDay = -1;
        _toolTip.SetToolTip(this, null);
        Invalidate();
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _toolTip.Dispose();
        base.Dispose(disposing);
    }
}
