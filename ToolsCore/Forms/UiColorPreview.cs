using ToolsCore.XML;

namespace ToolsCore.Forms;

/// <summary>
/// Nahlad farieb ovladacich prvkov na stranke farebnej schemy: panel, stitok, tlacidlo, zaskrtavacie pole, zoznam
/// s vybranou polozkou a stavovy riadok. Klik vyberie polozku schemy.
/// </summary>
internal sealed class UiColorPreview : Control, IColorPreview
{
    private readonly List<(Rectangle Bounds, ColorKey Key)> _areas = [];
    private Style? _style;

    public UiColorPreview()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
    }

    /// <inheritdoc />
    public Control Control => this;

    /// <inheritdoc />
    public event EventHandler<string>? KeyClicked;

    /// <inheritdoc />
    public void ShowScheme(Style style)
    {
        _style = style;
        Invalidate();
    }

    /// <inheritdoc />
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        _areas.Clear();
        if (_style is null)
            return;

        var c = _style.ControlsColorScheme;
        var g = e.Graphics;
        int pad = LogicalToDeviceUnits(8), row = Font.Height + LogicalToDeviceUnits(6);

        // panel cez cely nahlad, stavovy riadok dole
        Fill(g, ClientRectangle, c.Panel.BackColor);
        Area(ClientRectangle, UiColors.Panel);
        var status = new Rectangle(0, Height - row, Width, row);
        Fill(g, status, c.StatusBar.BackColor);
        DrawLabel(g, UiColors.StatusBar.Name, status, c.StatusBar.ForeColor, TextFormatFlags.Left);
        Area(status, UiColors.StatusBar);

        var x = pad;
        var list = new Rectangle(Width / 2, pad, Width / 2 - pad, row * 3 + 2);
        DrawLabel(g, UiColors.Panel.Name, new Rectangle(x, pad, Width / 3, row), c.Panel.ForeColor, TextFormatFlags.Left);

        var label = new Rectangle(x, pad + row + pad / 2, list.Left - pad * 2, row);
        Fill(g, label, c.Label.BackColor);
        DrawLabel(g, UiColors.Label.Name, label, c.Label.ForeColor, TextFormatFlags.Left);
        Area(label, UiColors.Label);

        var button = new Rectangle(x, label.Bottom + pad, Width / 5, row + LogicalToDeviceUnits(4));
        Fill(g, button, c.Button.BackColor);
        Outline(g, button, c.Border.ForeColor);
        DrawLabel(g, UiColors.Button.Name, button, c.Button.ForeColor, TextFormatFlags.HorizontalCenter);
        Area(Rectangle.Inflate(button, 2, 2), UiColors.Border);
        Area(Rectangle.Inflate(button, -2, -2), UiColors.Button);

        // zaskrtavacie pole so znackou - velkost a tvar ako systemove pole (13 bodov, tenka vyhladena fajka)
        var size = LogicalToDeviceUnits(13);
        var box = new Rectangle(button.Right + pad * 2, button.Top + (button.Height - size) / 2, size, size);
        Fill(g, box, c.Box.BackColor);
        Outline(g, box, c.Border.ForeColor);
        var smoothing = g.SmoothingMode;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        using (var pen = new Pen(c.Mark.ForeColor, LogicalToDeviceUnits(2) * 0.8f) { LineJoin = System.Drawing.Drawing2D.LineJoin.Round })
            g.DrawLines(pen, [Mark(box, 0.24f, 0.52f), Mark(box, 0.42f, 0.70f), Mark(box, 0.76f, 0.30f)]);
        g.SmoothingMode = smoothing;
        var markText = new Rectangle(box.Right + pad / 2, button.Top, list.Left - pad - box.Right, button.Height);
        DrawLabel(g, UiColors.Mark.Name, markText, c.Panel.ForeColor, TextFormatFlags.Left);
        Area(Rectangle.Union(box, markText), UiColors.Mark);

        // zoznam s vybranou polozkou
        Fill(g, list, c.Box.BackColor);
        Outline(g, list, c.Border.ForeColor);
        Area(Rectangle.Inflate(list, 2, 2), UiColors.Border);
        Area(Rectangle.Inflate(list, -2, -2), UiColors.Box);
        var item = new Rectangle(list.Left + 1, list.Top + 1, list.Width - 2, row);
        DrawLabel(g, UiColors.Box.Name, item, c.Box.ForeColor, TextFormatFlags.Left);
        item.Offset(0, row);
        Fill(g, item, c.Highlight.BackColor);
        DrawLabel(g, UiColors.Highlight.Name, item, c.Highlight.ForeColor, TextFormatFlags.Left);
        Area(item, UiColors.Highlight);
        item.Offset(0, row);
        DrawLabel(g, UiColors.Box.Name, item, c.Box.ForeColor, TextFormatFlags.Left);
    }

    /// <inheritdoc />
    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        // posledna pridana oblast je navrchu
        for (var i = _areas.Count - 1; i >= 0; i--)
        {
            if (!_areas[i].Bounds.Contains(e.Location))
                continue;

            KeyClicked?.Invoke(this, _areas[i].Key.Id);
            return;
        }
    }

    private void Area(Rectangle bounds, ColorKey key) => _areas.Add((bounds, key));

    private static PointF Mark(Rectangle box, float x, float y) => new(box.Left + box.Width * x, box.Top + box.Height * y);

    private static void Fill(Graphics g, Rectangle bounds, Color color)
    {
        using var brush = new SolidBrush(color);
        g.FillRectangle(brush, bounds);
    }

    private static void Outline(Graphics g, Rectangle bounds, Color color)
    {
        using var pen = new Pen(color);
        g.DrawRectangle(pen, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
    }

    private void DrawLabel(Graphics g, string text, Rectangle bounds, Color color, TextFormatFlags align)
    {
        var inner = Rectangle.Inflate(bounds, -LogicalToDeviceUnits(4), 0);
        TextRenderer.DrawText(g, text, Font, inner, color, align | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }
}
