using System.Collections;
using ExControls;
using ToolsCore.Icons;
using ToolsCore.XML;

namespace ToolsCore.Tools;

/// <summary>
/// Tema okien: prevod stylu programu na ExTheme a obsluhy pre standardne prvky WinForms.
/// </summary>
public static partial class FormUtils
{
    public static void ChangeColorContextMenu(Style style, ContextMenuStrip strip) => ApplyContextMenu(strip, style.ToExTheme());

    /// <summary>
    /// Po zmene schemy v nastaveniach ju prevezmu otvorene okna (okrem <paramref name="except" />).
    /// </summary>
    public static void RefreshTheme(Form? except = null)
    {
        foreach (var form in Application.OpenForms.Cast<Form>().Where(f => f != except).ToList())
            form.ApplyTheme();
    }

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
            if (panel.AutoScroll)
                panel.SetDarkScrollBars(theme.DarkScrollBars);
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
            ExThemer.KeepDefaults(tb, theme, nameof(tb.BackColor), nameof(tb.ForeColor), nameof(tb.BorderStyle));
            if (!theme.UseSystemStyle)
            {
                tb.BackColor = theme.BoxBackColor;
                tb.ForeColor = theme.BoxForeColor;
                tb.BorderStyle = BorderStyle.None;
            }

            tb.SetDarkScrollBars(theme.DarkScrollBars);
        });
        ExThemer.Register<LinkLabel>((ll, theme) => ll.LinkColor = theme.HighlightBackColor);
        ExThemer.Register<Label>((l, theme) => l.ForeColor = theme.LabelForeColor);
        ExThemer.Register<TreeView>((tv, theme) =>
        {
            ExThemer.KeepDefaults(tv, theme, nameof(tv.BorderStyle));
            tv.BackColor = theme.BoxBackColor;
            tv.ForeColor = theme.BoxForeColor;
            if (!theme.UseSystemStyle) tv.BorderStyle = BorderStyle.None;

            tv.SetDarkScrollBars(theme.DarkScrollBars);
        });
        ExThemer.Register<ListBox>((lb, theme) =>
        {
            ExThemer.KeepDefaults(lb, theme, nameof(lb.BorderStyle));
            lb.BackColor = theme.BoxBackColor;
            lb.ForeColor = theme.BoxForeColor;
            if (!theme.UseSystemStyle) lb.BorderStyle = BorderStyle.None;

            lb.SetDarkScrollBars(theme.DarkScrollBars);
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
        ExThemer.Register<ToolStrip>(ApplyToolStrip);
        ExThemer.Register<StatusStrip>((ss, theme) =>
        {
            ApplyToolStrip(ss, theme);

            // stavovy riadok ma vlastnu farbu stylu (predvolene ako panel); polozky ju dostanu tiez, inak by mali
            // pozadie panela - rozbalovacie ponuky tlacidiel ostavaju vo farbach panela
            var bar = GlobSettings.UsingStyle.ControlsColorScheme.StatusBar;
            ss.BackColor = bar.BackColor;
            ss.ForeColor = bar.ForeColor;
            foreach (ToolStripItem item in ss.Items)
            {
                item.BackColor = bar.BackColor;
                item.ForeColor = bar.ForeColor;
            }
        });
        ExThemer.Register<DataGridView>(ApplyDataGridView);
        ExThemer.Register<UserControl>((uc, theme) =>
        {
            ExThemer.Apply(uc.Controls, theme);
            uc.BackColor = theme.PanelBackColor;
            uc.ForeColor = theme.PanelForeColor;
            uc.SetDarkScrollBars(theme.DarkScrollBars);
        });
    }

    private static void ApplyToolStrip(ToolStrip ts, ExTheme theme)
    {
        ts.BackColor = theme.PanelBackColor;
        ts.ForeColor = theme.PanelForeColor;
        ts.RenderMode = ToolStripRenderMode.Professional;
        ts.Renderer = theme.UseSystemStyle
            ? new MyMenuRenderer(new LightColorTable(), true)
            : new MyMenuRenderer(new MyColorTable(), false);
        ApplyToolStripItems(ts.Items, theme, true);
    }

    private static void ApplyContextMenu(ContextMenuStrip strip, ExTheme theme)
    {
        strip.RenderMode = ToolStripRenderMode.Professional;
        strip.Renderer = theme.UseSystemStyle
            ? new ToolStripProfessionalRenderer(new LightColorTable())
            : new MyMenuRenderer(new MyColorTable(), true);
        SetColors(strip);
        SetColorMenuItems(strip.Items);

        void SetColorMenuItems(IEnumerable coll)
        {
            foreach (ToolStripItem toolStripItem in coll)
            {
                // oddelovac kresli renderer farbami ColorTable
                if (toolStripItem is ToolStripSeparator)
                    continue;

                SetColors(toolStripItem);
                if (toolStripItem is ToolStripMenuItem tsmi) SetColorMenuItems(tsmi.DropDown.Items);
            }
        }

        // systemovy vzhlad ma farby z navrhu
        void SetColors(Component item)
        {
            ExThemer.KeepDefaults(item, theme, nameof(Control.BackColor), nameof(Control.ForeColor));
            if (theme.UseSystemStyle)
                return;

            switch (item)
            {
                case ToolStrip ts:
                    ts.BackColor = theme.PanelBackColor;
                    ts.ForeColor = theme.PanelForeColor;
                    break;
                case ToolStripItem tsi:
                    tsi.BackColor = theme.PanelBackColor;
                    tsi.ForeColor = theme.PanelForeColor;
                    break;
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
            // polozka s vlastnou temou si najprv zapamata hodnoty z navrhu a nastavi sa sama, farby panela dostane
            // len pri systemovom vzhlade
            var themed = item is IThemeable or ToolStripControlHost { Control: IThemeable };
            if (themed)
                ApplyItemTheme(item, theme);
            if (!themed || theme.UseSystemStyle)
            {
                item.BackColor = theme.PanelBackColor;
                item.ForeColor = theme.PanelForeColor;
            }

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
                // prvok vlozeny do panela cez hostitela (napr. obycajne pole) - inak by ostal vo farbach systemu
                case ToolStripControlHost host when !themed:
                    ExThemer.Apply(host.Control, theme);
                    break;
            }
        }
    }

    private static void ApplyItemTheme(ToolStripItem item, ExTheme theme)
    {
        if (item is IThemeable themeable)
            themeable.ApplyTheme(theme);
        else if (item is ToolStripControlHost host)
            ExThemer.Apply(host.Control, theme);
    }

    private static void ApplyPropertyGrid(PropertyGrid grid, ExTheme theme)
    {
        ExThemer.KeepDefaults(grid, theme, nameof(grid.BackColor), nameof(grid.ForeColor), nameof(grid.ViewBackColor),
            nameof(grid.ViewForeColor), nameof(grid.HelpBorderColor), nameof(grid.HelpBackColor), nameof(grid.HelpForeColor),
            nameof(grid.LineColor), nameof(grid.CategoryForeColor), nameof(grid.CategorySplitterColor), nameof(grid.ViewBorderColor),
            nameof(grid.CommandsBorderColor), nameof(grid.CommandsForeColor), nameof(grid.CommandsBackColor),
            nameof(grid.SelectedItemWithFocusBackColor), nameof(grid.SelectedItemWithFocusForeColor));
        // farby prikazov dostane aj ich popis, ale obnova farieb mriezky ho vynecha
        foreach (Control command in grid.Controls[1].Controls)
            ExThemer.KeepDefaults(command, theme, nameof(command.BackColor), nameof(command.ForeColor));
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

        grid.Controls[2].Controls[0].SetDarkScrollBars(theme.DarkScrollBars);
        if (grid is not ExPropertyGrid ex)
            return;

        ExThemer.KeepDefaults(ex.InnerToolStrip!, theme, nameof(ToolStrip.Renderer));
        if (!theme.UseSystemStyle)
        {
            ex.InnerToolStrip!.RenderMode = ToolStripRenderMode.Professional;
            ex.InnerToolStrip.Renderer = new MyMenuRenderer(new MyColorTable(), false);
        }
    }

    private static void ApplyDataGridView(DataGridView dgv, ExTheme theme)
    {
        // ikona chyby bunky a riadka z banky namiesto systemovej
        GridErrorIcons.Attach(dgv);

        // DoubleBuffered je u DataGridView protected a predvolene vypnute
        DgvDoubleBuffered?.SetValue(dgv, true);

        ExThemer.KeepDefaults(dgv, theme, nameof(dgv.ColumnHeadersBorderStyle), nameof(dgv.RowHeadersBorderStyle),
            nameof(dgv.GridColor), nameof(dgv.BorderStyle));
        foreach (var style in new[] { dgv.ColumnHeadersDefaultCellStyle, dgv.RowHeadersDefaultCellStyle, dgv.DefaultCellStyle })
            ExThemer.KeepDefaults(style, theme, nameof(style.BackColor), nameof(style.ForeColor));

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
                    ExThemer.KeepDefaults(cbutton, theme, nameof(cbutton.FlatStyle));
                    ExThemer.KeepDefaults(cbutton.DefaultCellStyle, theme, nameof(DataGridViewCellStyle.ForeColor), nameof(DataGridViewCellStyle.BackColor),
                        nameof(DataGridViewCellStyle.SelectionBackColor), nameof(DataGridViewCellStyle.SelectionForeColor));
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
                    ExThemer.KeepDefaults(dcl, theme, nameof(dcl.LinkColor), nameof(dcl.ActiveLinkColor), nameof(dcl.TrackVisitedState),
                        nameof(dcl.LinkBehavior));
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

        foreach (Control dgvc in dgv.Controls)
            if (dgvc is ScrollBar sc)
                sc.SetDarkScrollBars(theme.DarkScrollBars);
        GridScrollCorner.Apply(dgv, theme.DarkScrollBars);
    }
}
