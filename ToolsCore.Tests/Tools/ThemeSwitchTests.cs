using System.Collections;
using System.Globalization;
using System.Reflection;
using ExControls;
using ToolsCore.Tools;
using ToolsCore.XML;

namespace ToolsCore.Tests.Tools;

/// <summary>
/// Zmena schemy za behu (tlacidlo Pouzit v nastaveniach): okno, ktore malo jednu schemu a potom dostalo druhu, vyzera rovnako
/// ako okno, ktore malo len druhu - v oboch smeroch (tmava ma vlastny vzhlad prvkov, svetla systemovy). Porovnavaju sa
/// verejne vlastnosti prvkov, poloziek ponuk, stlpcov a stylov buniek; vlastnosti, ktorymi sa lisia aj dve nove okna, sa
/// vynechaju.
/// </summary>
[STATestClass]
public class ThemeSwitchTests
{
    private static readonly Styles<Style> Schemes = Styles<Style>.CreateDefault();

    // zmena temy Windows prvku (WM_THEMECHANGED) a nove handle po zmene ramika prepocitaju rozmery a metriky - aj pri
    // tmavej teme; v skrytom okne sa navyse inak rozlozi obsah
    private static readonly HashSet<string> Recalculated = new(StringComparer.Ordinal)
    {
        "Bounds", "Bottom", "ClientRectangle", "ClientSize", "DisplayRectangle", "PreferredHeight", "ItemHeight", "VisibleCount",
        "TopNode", "LineColor", "Rtf", "Modified"
    };
    private static Style Light => Schemes[StyleNames.Light];
    private static Style Dark => Schemes[StyleNames.Dark];

    [TestMethod]
    public void TmavaPotomSvetla_RovnakoAkoLenSvetla() => AssertSwitch(Dark, Light);

    [TestMethod]
    public void SvetlaPotomTmava_RovnakoAkoLenTmava() => AssertSwitch(Light, Dark);

    private static void AssertSwitch(Style first, Style second)
    {
        var previous = GlobSettings.UsingStyle;
        using var fresh = CreateForm();
        using var other = CreateForm();
        using var switched = CreateForm();
        try
        {
            Apply(fresh, second);
            Apply(other, second);
            Apply(switched, first);
            // nastavenia pouziju schemu vo vsetkych oknach a hlavne okno ju v Applied pouzije znova
            Apply(switched, second);
            Apply(switched, second);

            var expected = Dump(fresh);
            var noise = Dump(other);
            var actual = Dump(switched);
            var differences = expected.Keys
                .Where(key => !Recalculated.Contains(key[(key.LastIndexOf('.') + 1)..]))
                .Where(key => expected[key] == noise.GetValueOrDefault(key) && expected[key] != actual.GetValueOrDefault(key))
                .Select(key => $"{key}: {expected[key]} -> {actual.GetValueOrDefault(key)}")
                .ToList();

            Assert.IsEmpty(differences, string.Join(Environment.NewLine, differences));
        }
        finally
        {
            GlobSettings.UsingStyle = previous;
        }
    }

    private static void Apply(Form form, Style style)
    {
        GlobSettings.UsingStyle = style;
        form.ApplyTheme();
    }

    /// <summary>
    /// Okno so vsetkymi typmi prvkov, ktore tema meni.
    /// </summary>
    private static Form CreateForm()
    {
        var form = new Form { ClientSize = new Size(800, 600) };

        var menu = new ContextMenuStrip();
        var parent = new ToolStripMenuItem("Rodic");
        parent.DropDownItems.Add("Dieta");
        menu.Items.AddRange([new ToolStripMenuItem("Polozka"), new ToolStripSeparator(), parent]);

        // styl buniek z navrhara a kontextova ponuka z kodu (ako tabulka vlakov GVDEditora)
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill, AutoGenerateColumns = false,
            DefaultCellStyle = new DataGridViewCellStyle { BackColor = SystemColors.Window, ForeColor = SystemColors.ControlText },
            ContextMenuStrip = new ContextMenuStrip { Items = { "Pridat", "Zobrazit" } }
        };
        grid.Columns.AddRange(new DataGridViewTextBoxColumn { Name = "Text" }, new DataGridViewButtonColumn { Name = "Tlacidlo" },
            new DataGridViewLinkColumn { Name = "Odkaz" }, new DataGridViewCheckBoxColumn { Name = "Volba" });
        grid.Rows.Add("a", "b", "c", true);

        var scroll = new Panel { AutoScroll = true, Size = new Size(100, 100) };
        scroll.Controls.Add(new Label { Text = "Dlhy obsah", Location = new Point(0, 300) });

        var split = new SplitContainer { Dock = DockStyle.Bottom, Height = 150 };
        split.Panel1.Controls.Add(new TreeView { Dock = DockStyle.Fill, ContextMenuStrip = menu, Nodes = { "Uzol" } });
        split.Panel2.Controls.Add(new ListBox { Dock = DockStyle.Fill, Items = { "Riadok" } });

        var user = new UserControl { Dock = DockStyle.Right, Width = 200 };
        user.Controls.Add(new ExPropertyGrid { Dock = DockStyle.Fill, SelectedObject = new Button() });

        var tools = new ToolStrip();
        tools.Items.AddRange([new ToolStripButton("Tlacidlo"), new ToolStripDropDownButton("Rozbalit", null, new ToolStripMenuItem("Volba"))]);
        var mainMenu = new MenuStrip();
        mainMenu.Items.Add(new ToolStripMenuItem("Subor", null, new ToolStripMenuItem("Otvorit")));
        var status = new StatusStrip();
        status.Items.Add(new ToolStripStatusLabel("Pripraveny"));

        var group = new ExGroupBox { Dock = DockStyle.Left, Width = 200 };
        group.Controls.AddRange([new ExButton { Text = "OK", Top = 20 }, new ExTextBox { Top = 50 }, new ExComboBox { Top = 80 },
            new ExCheckBox { Text = "Volba", Top = 110 }, new RichTextBox { Top = 140, Height = 40 }, new LinkLabel { Text = "Odkaz", Top = 190 },
            new PropertyGrid { Top = 220, Height = 100, SelectedObject = new Label() }]);

        form.Controls.AddRange([grid, scroll, split, user, group, tools, mainMenu, status]);
        form.MainMenuStrip = mainMenu;
        form.CreateControl();
        return form;
    }

    private static Dictionary<string, string> Dump(Form form)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        DumpControl(form, "Form", result);
        return result;
    }

    private static void DumpControl(Control control, string path, Dictionary<string, string> result)
    {
        DumpProperties(control, path, result);
        if (control.ContextMenuStrip is { } menu)
            DumpControl(menu, path + ".ContextMenuStrip", result);

        switch (control)
        {
            case ToolStrip strip:
                result[path + ".Renderer"] = strip.Renderer.GetType().Name;
                DumpItems(strip.Items, path, result);
                break;
            case DataGridView grid:
                DumpProperties(grid.DefaultCellStyle, path + ".DefaultCellStyle", result);
                DumpProperties(grid.ColumnHeadersDefaultCellStyle, path + ".ColumnHeadersDefaultCellStyle", result);
                DumpProperties(grid.RowHeadersDefaultCellStyle, path + ".RowHeadersDefaultCellStyle", result);
                foreach (DataGridViewColumn column in grid.Columns)
                {
                    DumpProperties(column, $"{path}.{column.Name}", result);
                    DumpProperties(column.DefaultCellStyle, $"{path}.{column.Name}.DefaultCellStyle", result);
                }

                break;
        }

        var index = 0;
        foreach (Control child in control.Controls)
            DumpControl(child, $"{path}/{index++}:{child.GetType().Name}", result);
    }

    private static void DumpItems(ToolStripItemCollection items, string path, Dictionary<string, string> result)
    {
        var index = 0;
        foreach (ToolStripItem item in items)
        {
            var itemPath = $"{path}.{index++}:{item.GetType().Name}";
            DumpProperties(item, itemPath, result);
            if (item is ToolStripDropDownItem { HasDropDownItems: true } dropDown)
                DumpItems(dropDown.DropDownItems, itemPath, result);
        }
    }

    private static void DumpProperties(object item, string path, Dictionary<string, string> result)
    {
        foreach (var property in item.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!property.CanRead || property.GetIndexParameters().Length > 0 || property.IsDefined(typeof(ObsoleteAttribute)))
                continue;

            object? value;
            try
            {
                value = property.GetValue(item);
            }
            catch (TargetInvocationException)
            {
                continue;
            }

            if (value is null or string or Color or Enum or Size or Point or Padding or Rectangle or bool or int or float or double)
                result[$"{path}.{property.Name}"] = Convert.ToString(value, CultureInfo.InvariantCulture) ?? "null";
            else if (value is Font font)
                result[$"{path}.{property.Name}"] = font.ToString();
            else if (value is not IEnumerable && value.GetType().Namespace == typeof(ExButton).Namespace && value is not Control)
                // casti Ex prvkov (styly pola, vzhlad tlacidla)
                DumpProperties(value, $"{path}.{property.Name}", result);
        }
    }
}
