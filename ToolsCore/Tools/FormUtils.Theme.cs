using System.Collections;
using ExControls;
using ToolsCore.XML;

namespace ToolsCore.Tools;

/// <summary>
/// Tema okien: prevod stylu programu na ExTheme a obsluhy pre standardne prvky WinForms.
/// </summary>
public static partial class FormUtils
{
    public static void ChangeColorContextMenu(Style style, ContextMenuStrip strip) => ApplyContextMenu(strip, style.ToExTheme());

    /// <summary>
    /// Tema ExControls podla stylu programu.
    /// </summary>
    public static ExTheme ToExTheme(this Style style)
    {
        var scheme = style.ControlsColorScheme;
        return new ExTheme
        {
            UseSystemStyle = style.ControlsDefaultStyle,
            DarkScrollBars = style.DarkScrollBar,
            PanelBackColor = scheme.Panel.BackColor,
            PanelForeColor = scheme.Panel.ForeColor,
            BoxBackColor = scheme.Box.BackColor,
            BoxForeColor = scheme.Box.ForeColor,
            ButtonBackColor = scheme.Button.BackColor,
            ButtonForeColor = scheme.Button.ForeColor,
            BorderColor = scheme.Border.ForeColor,
            HighlightBackColor = scheme.Highlight.BackColor,
            HighlightForeColor = scheme.Highlight.ForeColor,
            MarkColor = scheme.Mark.ForeColor,
            LabelForeColor = scheme.Label.ForeColor,
            TodayText = GlobalResources.Global_Today
        };
    }

    /// <summary>
    /// Nastavi prvkom kolekcie (a ich vnorenym prvkom) farby podla stylu. Ex* prvky sa nastyluju samy
    /// (<see cref="IThemeable" />), standardne prvky WinForms obsluhy zaregistrovane v <see cref="RegisterThemeHandlers" />.
    /// </summary>
    public static void ChangeStyleOfControls(Style style, IEnumerable collection)
    {
        RegisterThemeHandlers();
        ExThemer.Apply(collection, style.ToExTheme());
    }

    private static bool _themeHandlersRegistered;

    /// <summary>
    /// Obsluhy temy pre prvky mimo ExControls - vykreslovanie ponuk a panelov nastrojov (<see cref="MyMenuRenderer" />)
    /// a standardne prvky WinForms.
    /// </summary>
    private static void RegisterThemeHandlers()
    {
        if (_themeHandlersRegistered)
            return;
        _themeHandlersRegistered = true;

        ExThemer.Register<ContextMenuStrip>(ApplyContextMenu);
        ExThemer.Register<PropertyGrid>(ApplyPropertyGrid);
        ExThemer.Register<Panel>((panel, theme) =>
        {
            ExThemer.Apply(panel.Controls, theme);
            panel.BackColor = theme.PanelBackColor;
            panel.ForeColor = theme.PanelForeColor;
        });
        ExThemer.Register<SplitContainer>((sc, theme) =>
        {
            sc.BackColor = theme.PanelBackColor;
            sc.ForeColor = theme.PanelForeColor;
            ExThemer.Apply(sc.Panel1.Controls, theme);
            ExThemer.Apply(sc.Panel2.Controls, theme);
        });
        ExThemer.Register<RichTextBox>((tb, theme) =>
        {
            if (!theme.UseSystemStyle)
            {
                tb.BackColor = theme.BoxBackColor;
                tb.ForeColor = theme.BoxForeColor;
                tb.BorderStyle = BorderStyle.None;
            }

            if (theme.DarkScrollBars)
                tb.SetTheme(WindowsTheme.DarkExplorer);
        });
        ExThemer.Register<LinkLabel>((ll, theme) => ll.LinkColor = theme.HighlightBackColor);
        ExThemer.Register<Label>((l, theme) => l.ForeColor = theme.LabelForeColor);
        ExThemer.Register<TreeView>((tv, theme) =>
        {
            tv.BackColor = theme.BoxBackColor;
            tv.ForeColor = theme.BoxForeColor;
            if (!theme.UseSystemStyle) tv.BorderStyle = BorderStyle.None;

            if (theme.DarkScrollBars)
                tv.SetTheme(WindowsTheme.DarkExplorer);
        });
        ExThemer.Register<ListBox>((lb, theme) =>
        {
            lb.BackColor = theme.BoxBackColor;
            lb.ForeColor = theme.BoxForeColor;
            if (!theme.UseSystemStyle) lb.BorderStyle = BorderStyle.None;

            if (theme.DarkScrollBars)
                lb.SetTheme(WindowsTheme.DarkExplorer);
        });
        ExThemer.Register<MenuStrip>((ms, theme) =>
        {
            ms.BackColor = theme.PanelBackColor;
            ms.ForeColor = theme.PanelForeColor;
            ms.RenderMode = ToolStripRenderMode.Professional;
            ms.Renderer = theme.UseSystemStyle
                ? new ToolStripProfessionalRenderer(new LightColorTable())
                : new MyMenuRenderer(new MyColorTable(), false);
            ApplyToolStripItems(ms.Items, theme, false);
        });
        ExThemer.Register<ToolStrip>((ts, theme) =>
        {
            ts.BackColor = theme.PanelBackColor;
            ts.ForeColor = theme.PanelForeColor;
            ts.RenderMode = ToolStripRenderMode.Professional;
            ts.Renderer = theme.UseSystemStyle
                ? new MyMenuRenderer(new LightColorTable(), true)
                : new MyMenuRenderer(new MyColorTable(), false);
            ApplyToolStripItems(ts.Items, theme, true);
        });
        ExThemer.Register<DataGridView>(ApplyDataGridView);
        ExThemer.Register<UserControl>((uc, theme) =>
        {
            ExThemer.Apply(uc.Controls, theme);
            uc.BackColor = theme.PanelBackColor;
            uc.ForeColor = theme.PanelForeColor;
            if (theme.DarkScrollBars)
                uc.SetTheme(WindowsTheme.DarkExplorer);
        });
    }

    private static void ApplyContextMenu(ContextMenuStrip strip, ExTheme theme)
    {
        strip.RenderMode = ToolStripRenderMode.Professional;
        if (theme.UseSystemStyle)
        {
            strip.Renderer = new ToolStripProfessionalRenderer(new LightColorTable());
            return;
        }

        strip.Renderer = new MyMenuRenderer(new MyColorTable(), true);
        strip.BackColor = theme.PanelBackColor;
        strip.ForeColor = theme.PanelForeColor;
        SetColorMenuItems(strip.Items);

        void SetColorMenuItems(IEnumerable coll)
        {
            foreach (ToolStripItem toolStripItem in coll)
            {
                toolStripItem.BackColor = theme.PanelBackColor;
                toolStripItem.ForeColor = theme.PanelForeColor;
                if (toolStripItem is ToolStripMenuItem tsmi) SetColorMenuItems(tsmi.DropDown.Items);
            }
        }
    }

    /// <summary>
    /// Farby poloziek ponuky alebo panela nastrojov vratane podponuk.
    /// </summary>
    /// <param name="items">polozky</param>
    /// <param name="theme">tema</param>
    /// <param name="buttonDropDowns">ci prechadzat aj rozbalovacie tlacidla panela nastrojov</param>
    private static void ApplyToolStripItems(ToolStripItemCollection items, ExTheme theme, bool buttonDropDowns)
    {
        foreach (ToolStripItem item in items)
        {
            item.BackColor = theme.PanelBackColor;
            item.ForeColor = theme.PanelForeColor;
            switch (item)
            {
                case ToolStripMenuItem tsmi:
                    ApplyToolStripItems(tsmi.DropDown.Items, theme, buttonDropDowns);
                    break;
                case ToolStripSplitButton tssb when buttonDropDowns:
                    ApplyToolStripItems(tssb.DropDownItems, theme, buttonDropDowns);
                    break;
                case ToolStripDropDownButton tsddb when buttonDropDowns:
                    ApplyToolStripItems(tsddb.DropDownItems, theme, buttonDropDowns);
                    break;
                case IThemeable themeable:
                    themeable.ApplyTheme(theme);
                    break;
            }
        }
    }

    private static void ApplyPropertyGrid(PropertyGrid grid, ExTheme theme)
    {
        if (!theme.UseSystemStyle)
        {
            grid.BackColor = theme.PanelBackColor;
            grid.ForeColor = theme.PanelForeColor;
            grid.ViewBackColor = theme.BoxBackColor;
            grid.ViewForeColor = theme.BoxForeColor;
            grid.HelpBorderColor = theme.BorderColor;
            grid.HelpBackColor = theme.PanelBackColor;
            grid.HelpForeColor = theme.PanelForeColor;
            grid.LineColor = theme.ButtonBackColor;
            grid.CategoryForeColor = theme.BoxForeColor;
            grid.CategorySplitterColor = theme.BoxForeColor;
            grid.ViewBorderColor = theme.BorderColor;
            grid.CommandsBorderColor = theme.PanelBackColor;
            grid.CommandsForeColor = theme.PanelForeColor;
            grid.CommandsBackColor = theme.PanelBackColor;
            grid.SelectedItemWithFocusBackColor = theme.HighlightBackColor;
            grid.SelectedItemWithFocusForeColor = theme.HighlightForeColor;
        }

        if (theme.DarkScrollBars)
            grid.Controls[2].Controls[0].SetTheme(WindowsTheme.DarkExplorer);
        if (grid is ExPropertyGrid ex && !theme.UseSystemStyle)
        {
            ex.InnerToolStrip!.RenderMode = ToolStripRenderMode.Professional;
            ex.InnerToolStrip.Renderer = new MyMenuRenderer(new MyColorTable(), false);
        }
    }

    private static void ApplyDataGridView(DataGridView dgv, ExTheme theme)
    {
        // DoubleBuffered je u DataGridView protected a predvolene vypnute
        DgvDoubleBuffered?.SetValue(dgv, true);

        dgv.EnableHeadersVisualStyles = theme.UseSystemStyle;
        dgv.DefaultCellStyle.SelectionBackColor = theme.HighlightBackColor;
        dgv.DefaultCellStyle.SelectionForeColor = theme.HighlightForeColor;
        dgv.RowHeadersDefaultCellStyle.SelectionBackColor = theme.HighlightBackColor;
        dgv.RowHeadersDefaultCellStyle.SelectionForeColor = theme.HighlightForeColor;
        dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = theme.HighlightBackColor;
        dgv.ColumnHeadersDefaultCellStyle.SelectionForeColor = theme.HighlightForeColor;

        if (!theme.UseSystemStyle)
        {
            dgv.ColumnHeadersDefaultCellStyle.BackColor = theme.ButtonBackColor;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = theme.ButtonForeColor;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;

            dgv.RowHeadersDefaultCellStyle.BackColor = theme.ButtonBackColor;
            dgv.RowHeadersDefaultCellStyle.ForeColor = theme.ButtonForeColor;
            dgv.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;

            dgv.DefaultCellStyle.BackColor = theme.BoxBackColor;
            dgv.DefaultCellStyle.ForeColor = theme.BoxForeColor;

            dgv.ForeColor = theme.PanelForeColor;
            dgv.BackColor = theme.PanelBackColor;
            dgv.BackgroundColor = theme.BoxBackColor;
            dgv.GridColor = theme.BorderColor;
            dgv.BorderStyle = BorderStyle.None;
        }
        else
        {
            dgv.ForeColor = SystemColors.ControlText;
            dgv.BackColor = SystemColors.Control;
            dgv.BackgroundColor = SystemColors.AppWorkspace;
        }

        foreach (DataGridViewColumn column in dgv.Columns)
        {
            switch (column)
            {
                case IThemeable themeable:
                    themeable.ApplyTheme(theme);
                    break;
                case DataGridViewButtonColumn cbutton:
                {
                    if (!theme.UseSystemStyle)
                    {
                        cbutton.FlatStyle = FlatStyle.Flat;
                        cbutton.DefaultCellStyle.ForeColor = theme.ButtonForeColor;
                        cbutton.DefaultCellStyle.BackColor = theme.ButtonBackColor;
                        cbutton.DefaultCellStyle.SelectionBackColor = theme.HighlightBackColor;
                        cbutton.DefaultCellStyle.SelectionForeColor = theme.HighlightForeColor;
                    }

                    break;
                }
                case DataGridViewLinkColumn dcl:
                {
                    if (!theme.UseSystemStyle)
                    {
                        dcl.LinkColor = ControlPaint.LightLight(theme.HighlightBackColor);
                        dcl.ActiveLinkColor = theme.PanelForeColor;
                        dcl.TrackVisitedState = false;
                        dcl.LinkBehavior = LinkBehavior.HoverUnderline;
                    }

                    break;
                }
                case DataGridViewCheckBoxColumn ccb:
                {
                    ccb.CellTemplate = new DataGridViewExCheckBoxCell
                    {
                        DefaultStyle = theme.UseSystemStyle,
                        BorderColor = theme.BorderColor,
                        MarkColor = theme.MarkColor,
                        SquareBackColor = theme.PanelBackColor,
                        HighlightColor = theme.HighlightBackColor
                    };
                    break;
                }
            }
        }

        if (theme.DarkScrollBars)
            foreach (Control dgvc in dgv.Controls)
                if (dgvc is ScrollBar sc)
                    sc.SetTheme(WindowsTheme.DarkExplorer);
    }
}
