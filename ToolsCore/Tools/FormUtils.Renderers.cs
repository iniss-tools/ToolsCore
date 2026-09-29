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

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            if (e.Item?.Enabled == true)
                e.ArrowColor = GlobSettings.UsingStyle.ControlsColorScheme.Button.ForeColor;

            base.OnRenderArrow(e);
        }

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
