using System.Collections;
using System.Runtime.CompilerServices;
using ExControls;

namespace ToolsCore.Tools;

/// <summary>
/// Trieda obsahujúca metódy na správu formularov.
/// </summary>
public static partial class FormUtils
{
    private static readonly System.Reflection.PropertyInfo? DgvDoubleBuffered =
        typeof(DataGridView).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

    // velkost ikon panela nastrojov z navrhara pri 96 DPI - zapamata sa pred prvou zmenou podla pisma
    private static readonly ConditionalWeakTable<ToolStrip, StrongBox<int>> DesignedImageSizes = new();

    private static StrongBox<int> DesignedImageSize(ToolStrip strip)
    {
        var size = Math.Min(strip.ImageScalingSize.Width, strip.ImageScalingSize.Height);
        // predvolenu velkost 16 uz panel prepocital podla DPI
        return new StrongBox<int>(size == strip.LogicalToDeviceUnits(16) ? 16 : size);
    }

    /// <summary>
    /// Zmena farebnej schémy dialogu alebo ovladacieho prvku.
    /// </summary>
    /// <param name="form">formular</param>
    public static void ApplyThemeAndFonts(this Form form)
    {
        form.SuspendLayout();
        form.ApplyTheme();
        form.SetFormFont();
        form.ResumeLayout(true);
    }

    /// <summary>
    /// Nastaví farby a písmo podľa aktuálneho štýlu položke menu, ktorá vznikla až za behu aplikácie
    /// (napr. položka v zozname posledných projektov).
    /// </summary>
    /// <param name="item">položka menu</param>
    public static void ApplyThemeAndFont(this ToolStripItem item)
    {
        var scheme = GlobSettings.UsingStyle.ControlsColorScheme;
        item.BackColor = scheme.Panel.BackColor;
        item.ForeColor = scheme.Panel.ForeColor;
        item.Font = GlobSettings.Fonts.Menus;
    }

    public static void ApplyTheme(this Control c)
    {
        var style = GlobSettings.UsingStyle;
            
        if (c is Form f)
        {
            f.BackColor = style.ControlsColorScheme.Panel.BackColor;
            f.ForeColor = style.ControlsColorScheme.Panel.ForeColor;
            f.SetImmersiveDarkMode(style.DarkTitleBar);
            f.HandleCreated -= ReapplyTitleBar;
            f.HandleCreated += ReapplyTitleBar;
        }

        ChangeStyleOfControls(style, c.Controls);
    }

    private static void ReapplyTitleBar(object? sender, EventArgs e)
    {
        if (sender is Form f)
            f.SetImmersiveDarkMode(GlobSettings.UsingStyle.DarkTitleBar);
    }

    /// <summary>
    /// Zmení písmo Formu
    /// </summary>
    /// <param name="form">upravovaný Form</param>
    public static void SetFormFont(this Form form)
    {
        form.AutoSize = true;
        form.Font = GlobSettings.Fonts.Controls;
        ChangeControlsFont(form.Controls);
    }

    private static void ChangeControlsFont(IEnumerable collection)
    {
        foreach (Control control in collection)
        {
            switch (control)
            {
                case GroupBox or Panel or TabControl or Button:
                    control.Font = GlobSettings.Fonts.Controls;
                    break;
                case ToolStrip ts:
                    control.Font = GlobSettings.Fonts.Menus;
                    SetFontToolStripItems(ts);
                    break;
                case DataGridView dgv:
                    ApplyTableFont(dgv);
                    break;
            }

            if (control.ContextMenuStrip != null) 
                SetFontToolStripItems(control.ContextMenuStrip);

            ChangeControlsFont(control.Controls);
        }
    }

    private static void SetFontToolStripItems(ToolStrip strip)
    {
        // panel nastrojov ma ikony vo velkosti z navrhara prepocitanej podla DPI (ikony SVG su ostre len v nej),
        // ponuky a stavovy riadok podla pisma
        var size = strip is MenuStrip or StatusStrip or ToolStripDropDown
            ? GlobSettings.Fonts.Menus.Height
            : strip.LogicalToDeviceUnits(DesignedImageSizes.GetValue(strip, DesignedImageSize).Value);
        strip.ImageScalingSize = new Size(size, size);

        foreach (ToolStripItem toolStripItem in strip.Items)
        {
            toolStripItem.Font = GlobSettings.Fonts.Menus;
            switch (toolStripItem)
            {
                case ToolStripMenuItem tsmi:
                    SetFontToolStripItems(tsmi.DropDown);
                    break;
                case ToolStripSeparator sep:
                    sep.Size = new Size(6, size + 10);
                    break;
            }
        }
    }

}