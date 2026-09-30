using System.Collections;
using ExControls;

namespace ToolsCore.Tools;

/// <summary>
/// Trieda obsahujúca metódy na správu formularov.
/// </summary>
public static partial class FormUtils
{
    private static readonly System.Reflection.PropertyInfo? DgvDoubleBuffered =
        typeof(DataGridView).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

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
        item.Font = GlobSettings.Fonts.Menu.Font;
    }

    public static void ApplyTheme(this Control c)
    {
        var style = GlobSettings.UsingStyle;
            
        if (c is Form f)
        {
            f.BackColor = style.ControlsColorScheme.Panel.BackColor;
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
        form.Font = GlobSettings.Fonts.Labels.Font;
        ChangeControlsFont(form.Controls);
    }

    private static void ChangeControlsFont(IEnumerable collection)
    {
        foreach (Control control in collection)
        {
            switch (control)
            {
                case GroupBox:
                    control.Font = GlobSettings.Fonts.Labels.Font;
                    break;
                case Panel:
                    control.Font = GlobSettings.Fonts.Labels.Font;
                    break;
                case TabControl:
                    control.Font = GlobSettings.Fonts.Labels.Font;
                    break;
                case Button:
                    control.Font = GlobSettings.Fonts.Buttons.Font;
                    break;
                case MenuStrip ms:
                    control.Font = GlobSettings.Fonts.Menu.Font;
                    SetFontToolStripItems(ms);
                    break;
                case ToolStrip ts:
                    control.Font = GlobSettings.Fonts.Menu.Font;
                    SetFontToolStripItems(ts);
                    break;
                case DataGridView dgv:
                    dgv.ColumnHeadersDefaultCellStyle.Font = GlobSettings.Fonts.ColsHeader.Font;
                    dgv.DefaultCellStyle.Font = GlobSettings.Fonts.TableCells.Font;
                    break;
            }

            if (control.ContextMenuStrip != null) 
                SetFontToolStripItems(control.ContextMenuStrip);

            ChangeControlsFont(control.Controls);
        }
    }

    private static void SetFontToolStripItems(ToolStrip strip)
    {
        var size = GlobSettings.Fonts.Menu.Font.Height;
        strip.ImageScalingSize = new Size(size, size);

        foreach (ToolStripItem toolStripItem in strip.Items)
        {
            toolStripItem.Font = GlobSettings.Fonts.Menu.Font;
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