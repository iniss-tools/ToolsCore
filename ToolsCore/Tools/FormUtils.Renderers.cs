using System.Drawing.Imaging;

namespace ToolsCore.Tools;

/// <summary>
/// Vykreslovanie ponuk a panelov nastrojov podla stylu programu.
/// </summary>
public static partial class FormUtils
{
    private sealed class MyColorTable : ProfessionalColorTable
    {
        public override Color MenuItemSelected => GlobSettings.UsingStyle.ControlsColorScheme.Highlight.BackColor;
        public override Color MenuBorder => GlobSettings.UsingStyle.ControlsColorScheme.Border.ForeColor;
        public override Color ToolStripBorder => GlobSettings.UsingStyle.ControlsColorScheme.Border.ForeColor;
        public override Color SeparatorDark => GlobSettings.UsingStyle.ControlsColorScheme.Border.ForeColor;
        public override Color SeparatorLight => Color.Transparent;
        public override Color MenuItemBorder => GlobSettings.UsingStyle.ControlsColorScheme.Border.ForeColor;
        public override Color GripDark => GlobSettings.UsingStyle.ControlsColorScheme.Border.ForeColor;
        public override Color GripLight => Color.Transparent;
        public override Color MenuStripGradientBegin => GlobSettings.UsingStyle.ControlsColorScheme.Border.ForeColor;
        public override Color MenuStripGradientEnd => GlobSettings.UsingStyle.ControlsColorScheme.Border.ForeColor;

        public override Color ToolStripDropDownBackground => GlobSettings.UsingStyle.ControlsColorScheme.Panel.BackColor;
        public override Color ToolStripGradientBegin => GlobSettings.UsingStyle.ControlsColorScheme.Panel.BackColor;
        public override Color ToolStripGradientEnd => GlobSettings.UsingStyle.ControlsColorScheme.Panel.BackColor;
        public override Color ToolStripGradientMiddle => GlobSettings.UsingStyle.ControlsColorScheme.Panel.BackColor;

        public override Color MenuItemSelectedGradientBegin => GlobSettings.UsingStyle.ControlsColorScheme.Highlight.BackColor;
        public override Color MenuItemSelectedGradientEnd => GlobSettings.UsingStyle.ControlsColorScheme.Highlight.BackColor;

        public override Color MenuItemPressedGradientBegin => GlobSettings.UsingStyle.ControlsColorScheme.Highlight.BackColor;
        public override Color MenuItemPressedGradientEnd => GlobSettings.UsingStyle.ControlsColorScheme.Highlight.BackColor;

        public override Color ImageMarginGradientBegin => GlobSettings.UsingStyle.ControlsColorScheme.Panel.BackColor;
        public override Color ImageMarginGradientEnd => GlobSettings.UsingStyle.ControlsColorScheme.Panel.BackColor;
        public override Color ImageMarginGradientMiddle => GlobSettings.UsingStyle.ControlsColorScheme.Panel.BackColor;

        public override Color ButtonSelectedHighlight => GlobSettings.UsingStyle.ControlsColorScheme.Highlight.BackColor;
        public override Color ButtonSelectedHighlightBorder => GlobSettings.UsingStyle.ControlsColorScheme.Highlight.BackColor;

        public override Color ButtonSelectedGradientBegin => ControlPaint.Light(GlobSettings.UsingStyle.ControlsColorScheme.Highlight.BackColor);
        public override Color ButtonSelectedGradientEnd => ControlPaint.Light(GlobSettings.UsingStyle.ControlsColorScheme.Highlight.BackColor);
        public override Color ButtonSelectedGradientMiddle => ControlPaint.Light(GlobSettings.UsingStyle.ControlsColorScheme.Highlight.BackColor);
        public override Color ButtonSelectedBorder => GlobSettings.UsingStyle.ControlsColorScheme.Highlight.BackColor;

        public override Color ButtonPressedGradientBegin => GlobSettings.UsingStyle.ControlsColorScheme.Highlight.BackColor;
        public override Color ButtonPressedGradientEnd => GlobSettings.UsingStyle.ControlsColorScheme.Highlight.BackColor;
        public override Color ButtonPressedGradientMiddle => GlobSettings.UsingStyle.ControlsColorScheme.Highlight.BackColor;
        public override Color ButtonPressedBorder => Color.Transparent;

        public override Color CheckBackground => GlobSettings.UsingStyle.ControlsColorScheme.Panel.BackColor;
        public override Color CheckSelectedBackground => GlobSettings.UsingStyle.ControlsColorScheme.Highlight.BackColor;
        public override Color CheckPressedBackground => GlobSettings.UsingStyle.ControlsColorScheme.Highlight.BackColor;
    }

    public class LightColorTable : ProfessionalColorTable
    {
        public override Color ImageMarginGradientBegin => Color.White;
        public override Color ImageMarginGradientEnd => Color.White;
        public override Color ImageMarginGradientMiddle => Color.White;
    }

    public class MyMenuRenderer : ToolStripProfessionalRenderer
    {
        private bool WithBorder { get; }
        public MyMenuRenderer(ProfessionalColorTable table, bool withBorder) : base(table)
        {
            WithBorder = withBorder;
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            if (WithBorder)
            {
                base.OnRenderToolStripBorder(e);
            }
        }

        /// <summary>
        /// Okraj popisu stavoveho riadka (<see cref="ToolStripStatusLabel.BorderSides" />) ako oddelovac panela nastrojov.
        /// WinForms ho kresli ako 3D okraj systemovymi farbami - v tmavom style by bol biely a dvojity.
        /// </summary>
        protected override void OnRenderLabelBackground(ToolStripItemRenderEventArgs e)
        {
            if (e.Item is not ToolStripStatusLabel { BorderSides: not ToolStripStatusLabelBorderSides.None } label)
            {
                base.OnRenderLabelBackground(e);
                return;
            }

            var bounds = new Rectangle(Point.Empty, label.Size);
            if (label.BackColor != label.Owner?.BackColor)
            {
                using var brush = new SolidBrush(label.BackColor);
                e.Graphics.FillRectangle(brush, bounds);
            }

            // rovnake odsadenie ako zvisly oddelovac panela - ciara nesiaha az po okraj riadka
            var inset = label.Owner?.LogicalToDeviceUnits(3) ?? 3;
            var sides = label.BorderSides;
            DrawSeparatorLine(e.Graphics, sides.HasFlag(ToolStripStatusLabelBorderSides.Left), bounds.Left, bounds.Top + inset, bounds.Left, bounds.Bottom - inset - 1);
            DrawSeparatorLine(e.Graphics, sides.HasFlag(ToolStripStatusLabelBorderSides.Right), bounds.Right - 1, bounds.Top + inset, bounds.Right - 1, bounds.Bottom - inset - 1);
            DrawSeparatorLine(e.Graphics, sides.HasFlag(ToolStripStatusLabelBorderSides.Top), bounds.Left, bounds.Top, bounds.Right - 1, bounds.Top);
            DrawSeparatorLine(e.Graphics, sides.HasFlag(ToolStripStatusLabelBorderSides.Bottom), bounds.Left, bounds.Bottom - 1, bounds.Right - 1, bounds.Bottom - 1);
        }

        private void DrawSeparatorLine(Graphics graphics, bool draw, int x1, int y1, int x2, int y2)
        {
            if (!draw)
                return;

            using var pen = new Pen(ColorTable.SeparatorDark);
            graphics.DrawLine(pen, x1, y1, x2, y2);
        }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            if (e.Item?.Enabled == true)
                e.ArrowColor = GlobSettings.UsingStyle.ControlsColorScheme.Button.ForeColor;

            base.OnRenderArrow(e);
        }

        protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e)
        {
            if (e.Item is not { Enabled: false } item || e.Image is not { } image || e.ImageRectangle.IsEmpty)
            {
                base.OnRenderItemImage(e);
                return;
            }

            // neaktivna ikona siva a polopriehladna ako v IntelliJ - CreateDisabledImage ju zosvetli, na tmavom pozadi svieti
            using var attributes = new ImageAttributes();
            attributes.SetColorMatrix(DisabledImageMatrix);
            var source = item.ImageScaling == ToolStripItemImageScaling.None ? e.ImageRectangle.Size : image.Size;
            e.Graphics.DrawImage(image, e.ImageRectangle, 0, 0, source.Width, source.Height, GraphicsUnit.Pixel, attributes);
        }

        private static readonly ColorMatrix DisabledImageMatrix = new(
        [
            [0.30f, 0.30f, 0.30f, 0, 0],
            [0.59f, 0.59f, 0.59f, 0, 0],
            [0.11f, 0.11f, 0.11f, 0, 0],
            [0, 0, 0, 0.35f, 0],
            [0, 0, 0, 0, 1]
        ]);

        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
        {
            var bitmap = new Bitmap(e.Image!);

            // Set the image attribute's color mappings
            var colorMap = new ColorMap[1];
            colorMap[0] = new ColorMap
            {
                OldColor = Color.FromArgb(4, 2, 4),
                NewColor = GlobSettings.UsingStyle.ControlsColorScheme.Mark.ForeColor
            };
            var attr = new ImageAttributes();
            attr.SetRemapTable(colorMap);

            e.Graphics.DrawImage(bitmap, e.ImageRectangle, 0, 0, e.ImageRectangle.Width, e.ImageRectangle.Height, GraphicsUnit.Pixel, attr);
        }
    }
}
