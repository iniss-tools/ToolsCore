using System.Collections;
using System.Reflection;
using ExControls;
using ToolsCore.Commands;
using ToolsCore.Properties;
using ToolsCore.Tools;
using ToolsCore.XML;
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable UnusedParameter.Global
// ReSharper disable VirtualMemberNeverOverridden.Global

namespace ToolsCore.Forms;

public partial class FAppSettingsBase : Form
{
    private const string STYLES_EXAMPLE_TEXT = "ij = I.\":/{018Abc()Os";

    private string? _preselectMenuItem;
    // vlastnosti nizsie nastavuje len "skutocny" konstruktor - bezparametrovy je len pre navrhara
    private bool _changingStyleSelection;

    // konfiguracia pri otvoreni okna - na zistenie zmien, ktore sa prejavia az po restarte
    private readonly ConfigBase _original = null!;

    public ConfigBase Config { get; } = null!;
    public IList Styles { get; } = null!;
    public Type StyleType { get; } = null!;
    protected ExBindingList<DesktopColumn> Columns { get; set; } = null!;
    protected virtual IList<DesktopColumn> DefaultColumns => [];

    private IReadOnlyList<CommandInfo> _commandCatalog = [];
    private IEnumerable<AppCommand>? _boundCommands;

    /// <summary>
    /// Prikazy pre stranku klavesovych skratiek: vsetky prikazy programu a prikazy hlavneho okna naviazane na ponuku
    /// (strom potom kopiruje jej poradie); bez okna (<see langword="null" />) sa zoradia podla katalogu.
    /// </summary>
    protected void SetCommands(IReadOnlyList<CommandInfo> catalog, IEnumerable<AppCommand>? bound)
    {
        _commandCatalog = catalog;
        _boundCommands = bound;
    }

    /// <summary>
    /// Text volby, ktora pri spusteni otvori naposledy otvoreny projekt programu (instalaciu, banku, subor...);
    /// <see langword="null" /> = vseobecny text.
    /// </summary>
    protected virtual string? ReopenLastText => null;

    /// <summary>
    /// Nazov stranky so stlpcami tabulky hlavneho okna; <see langword="null" /> = vseobecny nazov.
    /// </summary>
    protected virtual string? ColumnsPageText => null;

    /// <summary>
    /// Popis pouzitia pisma editora na stranke Pisma; <see langword="null" /> = program editor nema.
    /// </summary>
    protected virtual string? EditorFontText => null;

    protected bool ShouldRestart { get; set; }
    public Style UsingStyle { get; private set; } = null!;

    /// <summary>
    /// Not use - only for designer
    /// </summary>
    public FAppSettingsBase()
    {
        InitializeComponent();
    }

    public FAppSettingsBase(ConfigBase config, IList copiedStyles, Style usingStyle, Type styleType)
    {
        InitializeComponent();

        _original = config;
        Config = config with {};
        Styles = copiedStyles;
        StyleType = styleType;

        optionsView.HeaderNodeNameFont = new Font(optionsView.HeaderNodeNameFont, FontStyle.Bold);
        optionsView.SearchBox.HintText = Resources.Settings_SearchHint;

        UsingStyle = usingStyle;
    }

    private void LoadComponents()
    {
        cbAppLanguage.BindEnum(new Dictionary<AppLanguage, string>
        {
            [AppLanguage.Slovak] = "Slovenčina",
            [AppLanguage.Czech] = "Čeština" 
        });

        if (ReopenLastText is { } reopenText)
            cboxReopenLast.Text = reopenText;
        cboxReopenLast.Checked = Config.Startup == StartupType.LastProject;
        if (ColumnsPageText is { } columnsText)
            pDesktopColumns.NodeText = columnsText;

        keymapPage.LoadCommands(KeymapTree.Build(_commandCatalog, _boundCommands, Resources.Keymap_MainWindow), Config.Shortcuts);
        configBindingSource.DataSource = Config;
        fontsPage.LoadFonts(Config.Fonts, EditorFontText);
        tvStyles.SelectedNode = tvStyles.Nodes.Count == 0 ? null : tvStyles.Nodes[0];
    }

    private void FAppSettingsBase_Load(object sender, EventArgs e)
    {
        if (DesignMode) 
            return;

        this.ApplyThemeAndFonts();

        tscbStyles.ComboBox.DataSource = Styles;
        tscbStyles.SelectedItem = Styles.Cast<Style>().FirstOrDefault(s => s.Name == UsingStyle.Name);
        RefreshStyleChoice();
        tsbApplyStyle.Enabled = false;
        optionsView.TreeView.ExpandAll();
        OnLoad();
    }

    public void PreselectMenuItem(string name) => _preselectMenuItem = name;

    protected virtual void OnLoad()
    {
        LoadComponents();

        if (!string.IsNullOrWhiteSpace(_preselectMenuItem)) 
            optionsView.ShowPanel(_preselectMenuItem);
    }

    private void FAppSettingsBase_HelpButtonClicked(object sender, CancelEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(Config.LinkAppSettingsGuide))
            return;

        Utils.OpenShell(Config.LinkAppSettingsGuide);
    }

    private void BSave_Click(object sender, EventArgs e)
    {
        configBindingSource.EndEdit();
        Config.Startup = cboxReopenLast.Checked ? StartupType.LastProject : StartupType.EmptyWindow;
        fontsPage.SaveTo(Config.Fonts);
        if (Config.Language != _original.Language || !Equals(Config.Fonts.Interface, _original.Fonts.Interface))
            ShouldRestart = true;
        var result = OnSaving();
        DialogResult = result ? DialogResult.OK : DialogResult.None;
        if (!result)
            return;

        SaveData();
        // tabulky a editory prevezmu pisma hned; pismo rozhrania az po restarte (meni rozlozenie okien)
        GlobSettings.Fonts = Config.Fonts with { Interface = GlobSettings.Fonts.Interface };
        FormUtils.RefreshFonts();
    }

    private void BStorno_Click(object sender, EventArgs e)
    {
        configBindingSource.CancelEdit();
        DialogResult = DialogResult.Cancel;
    }

    protected virtual bool OnSaving()
    {
        if (ShouldRestart)
        {
            var result = Utils.ShowQuestion(Resources.Settings_RestartNow, 
                MessageBoxButtons.YesNoCancel);
            switch (result)
            {
                case DialogResult.Yes:
                    SaveData();
                    Utils.RestartApp();
                    break;
                case DialogResult.Cancel:
                    return false;
            }
        }

        return true;
    }

    protected virtual void SaveData()
    {
    }

    private void BColUp_Click(object sender, EventArgs e)
    {
        if (dgvColumns.CurrentRow == null) 
            return;
        
        var sel = dgvColumns.SelectedRows[0].Index;
        var col = Columns[sel];

        if (sel - 1 >= 0)
        {
            Columns.RemoveAt(sel);
            Columns.Insert(sel - 1, col);
            dgvColumns.ClearSelection();
            dgvColumns.Rows[sel - 1].Selected = true;
        }
    }

    private void BColDown_Click(object sender, EventArgs e)
    {
        if (dgvColumns.CurrentRow == null) 
            return;
        
        var sel = dgvColumns.SelectedRows[0].Index;
        var col = Columns[sel];

        if (sel + 1 < Columns.Count)
        {
            Columns.RemoveAt(sel);
            Columns.Insert(sel + 1, col);
            dgvColumns.ClearSelection();
            dgvColumns.Rows[sel + 1].Selected = true;
        }
    }

    private void BResetColumns_Click(object sender, EventArgs e)
    {
        Columns.Clear();
        foreach (var column in DefaultColumns) Columns.Add(column);
        Columns.ResetBindings();
    }

    private void TscbStyles_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (tscbStyles.SelectedIndex == -1)
            return;

        var style = (Style)Styles[tscbStyles.SelectedIndex]!;
        FillTreeViewStyles(style, tvStyles);
        tvStyles.ExpandAll();
        cboxDefaultVisual.Checked = style.ControlsDefaultStyle;
        cboxDarkTitlebar.Checked = style.DarkTitleBar;
        cboxDarkScrollbars.Checked = style.DarkScrollBar;

        if (style.Name is StyleNames.Light or StyleNames.Dark)
        {
            tsbRenameStyle.Enabled = false;
            tsbDeleteStyle.Enabled = false;
        }
        else
        {
            tsbRenameStyle.Enabled = true;
            tsbDeleteStyle.Enabled = true;
        }

        tsbApplyStyle.Enabled = style != UsingStyle;
        tvStyles.SelectedNode = tvStyles.Nodes.Count != 0 ? tvStyles.Nodes[0] : null;
    }

    private void CboxDefaultVisual_CheckedChanged(object sender, EventArgs e)
    {
        var style = (Style?)tscbStyles.SelectedItem;
        if (style is not null)
            style.ControlsDefaultStyle = cboxDefaultVisual.Checked;
    }

    private void CboxDarkTitlebar_CheckedChanged(object sender, EventArgs e)
    {
        var style = (Style?)tscbStyles.SelectedItem;
        if (style is not null)
            style.DarkTitleBar = cboxDarkTitlebar.Checked;
    }

    private void CboxDarkScrollbars_CheckedChanged(object sender, EventArgs e)
    {
        var style = (Style?)tscbStyles.SelectedItem;
        if (style is not null)
            style.DarkScrollBar = cboxDarkScrollbars.Checked;
    }

    protected virtual void FillTreeViewStyles(object selectedStyle, ExTreeView tv)
    {
        if (selectedStyle is not Style style)
            return;

        tv.Nodes.Clear();
        var controls = CreateParentNode(style.ControlsColorScheme, nameof(style.ControlsColorScheme));
        CreateNode(style.ControlsColorScheme.Label, nameof(style.ControlsColorScheme.Label), controls);
        CreateNode(style.ControlsColorScheme.Panel, nameof(style.ControlsColorScheme.Panel), controls);
        CreateNode(style.ControlsColorScheme.StatusBar, nameof(style.ControlsColorScheme.StatusBar), controls);
        CreateNode(style.ControlsColorScheme.Border, nameof(style.ControlsColorScheme.Border), controls);
        CreateNode(style.ControlsColorScheme.Box, nameof(style.ControlsColorScheme.Box), controls);
        CreateNode(style.ControlsColorScheme.Button, nameof(style.ControlsColorScheme.Button), controls);
        CreateNode(style.ControlsColorScheme.Mark, nameof(style.ControlsColorScheme.Mark), controls);
        CreateNode(style.ControlsColorScheme.Highlight, nameof(style.ControlsColorScheme.Highlight), controls);
        tv.Nodes.Add(controls);
    }

    public static TreeNode CreateParentNode(IColorScheme scheme, string name)
    {
        var node = new TreeNode();
        node.Text = scheme.Name;
        node.Name = name;
        node.Tag = scheme;
        return node;
    }

    public static void CreateNode(ColorSetting setting, string name, TreeNode parent)
    {
        var node = new TreeNode();
        node.Text = setting.Name;
        node.Name = name;
        node.Tag = setting;
        parent.Nodes.Add(node);
    }

    private void TvStyles_AfterSelect(object sender, TreeViewEventArgs e)
    {
        _changingStyleSelection = true;
        var node = e.Node!;

        if (node.Tag is IColorScheme)
        {
            //tag je katagoria poloziek
            gbColorItemSettings.Enabled = false;
            csForeColor.SelectedColor = Color.Transparent;
            csForeColor.BorderColor = SystemColors.ButtonShadow;
            csBackColor.SelectedColor = Color.Transparent;
            csBackColor.BorderColor = SystemColors.ButtonShadow;

            lFontStyleExample.Text = "";
            lFontStyleExample.BackColor = Color.Transparent;
            _changingStyleSelection = false;
            return;
        }

        if (node.Tag is not ColorSetting setting)
        {
            _changingStyleSelection = false;
            return;
        }
        
        //tag je nastavenie polozky
        gbColorItemSettings.Enabled = true;
        cboxBold.Enabled = !setting.DisableFontBoldEdit;
        csForeColor.Enabled = true;
        csBackColor.Enabled = !setting.DisableBackColorEdit;
        csForeColor.SelectedColor = setting.ForeColor;
        csBackColor.SelectedColor = setting.BackColor;
        csForeColor.BorderColor = Color.Black;
        csBackColor.BorderColor = setting.DisableBackColorEdit ? SystemColors.ButtonShadow : Color.Black;
        lStyleBackColor.Enabled = !setting.DisableBackColorEdit;
        lStyleForeColor.Enabled = true;
        bResetColorSetting.Enabled = true;

        lFontStyleExample.Text = STYLES_EXAMPLE_TEXT;

        _changingStyleSelection = false;
    }

    private void CsForeColor_SelectedColorChanged(object sender, EventArgs e)
    {
        RefreshColorSetting();
    }

    private void CsBackColor_SelectedColorChanged(object sender, EventArgs e)
    {
        RefreshColorSetting();
    }

    private void CboxBold_CheckedChanged(object sender, EventArgs e)
    {
        RefreshColorSetting();
    }

    private void RefreshColorSetting()
    {
        var selectedNode = tvStyles.SelectedNode;

        //pri inicializacii formulara
        if (selectedNode is null)
            return;

        //je vybrata kategoria
        if (selectedNode.Tag is IColorScheme)
            return;

        //je vybrata polozka kategorie
        lFontStyleExample.ForeColor = csForeColor.SelectedColor;
        lFontStyleExample.BackColor = csBackColor.SelectedColor;
        var oldFont = lFontStyleExample.Font;
        lFontStyleExample.Font = new Font(Font, cboxBold.Checked ? FontStyle.Bold : FontStyle.Regular);
        if (oldFont != Font)
            oldFont.Dispose();

        if (_changingStyleSelection)
            return;

        var selectedStyleConfig = (ColorSetting) selectedNode.Tag!;

        selectedStyleConfig.ForeColor = lFontStyleExample.ForeColor = csForeColor.SelectedColor;
        if (!selectedStyleConfig.DisableBackColorEdit) 
            selectedStyleConfig.BackColor = csBackColor.SelectedColor;
        
        if (!selectedStyleConfig.DisableFontBoldEdit) 
            selectedStyleConfig.Bold = cboxBold.Checked;
    }

    private void TsbApplyStyle_Click(object sender, EventArgs e)
    {
        if (tscbStyles.SelectedItem is Style style)
            ApplyStyle(style);
    }

    private void CbStyle_SelectionChangeCommitted(object? sender, EventArgs e)
    {
        if (cbStyle.SelectedItem is Style style)
            ApplyStyle(style);
    }

    /// <summary>
    /// Nastavi pouzivany styl (prejavi sa po ulozeni nastaveni).
    /// </summary>
    private void ApplyStyle(Style style)
    {
        var previousStyle = UsingStyle;
        UsingStyle = style;

        foreach (Style s in Styles) 
            s.Used = false;

        UsingStyle.Used = true;
        if (!previousStyle.ControlsDefaultStyle && UsingStyle.ControlsDefaultStyle)
        {
            Utils.ShowInfo(Resources.Settings_RestartLater);
            ShouldRestart = true;
        }
        tsbApplyStyle.Enabled = !ReferenceEquals(tscbStyles.SelectedItem, UsingStyle);
        RefreshStyleChoice();
    }

    /// <summary>
    /// Zoznam stylov na stranke Vzhlad s vybranym pouzivanym stylom.
    /// </summary>
    private void RefreshStyleChoice()
    {
        cbStyle.DataSource = null;
        cbStyle.DataSource = Styles.Cast<Style>().ToList();
        cbStyle.SelectedItem = UsingStyle;
    }

    private void TsbAddStyle_Click(object sender, EventArgs e)
    {
        var form = new FInputBox(Styles, (o, v) => v == ((Style) o).Name);
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            var newStyle = CreateStyleInstance(form.NewValue);
            Styles.Add(newStyle);
            tscbStyles.ComboBox.DataSource = null;
            tscbStyles.ComboBox.DataSource = Styles;
            tscbStyles.SelectedItem = newStyle;
            RefreshStyleChoice();
        }
    }

    private void TsbResetStyle_Click(object sender, EventArgs e)
    {
        if (tscbStyles.SelectedIndex == -1)
            return;

        var style = (Style)tscbStyles.SelectedItem!;
        var newStyle = OnResetStyle(style.Name == StyleNames.Dark);
        newStyle.Name = style.Name;
        newStyle.Used = style.Used;
        var actualIndex = tscbStyles.SelectedIndex;
        Styles[actualIndex] = newStyle;
        tscbStyles.ComboBox.DataSource = Styles;
        tscbStyles.SelectedIndex = -1;
        tscbStyles.SelectedIndex = actualIndex;
        FillTreeViewStyles(newStyle, tvStyles);
        tvStyles.ExpandAll();
        tvStyles.SelectedNode = tvStyles.Nodes.Count != 0 ? tvStyles.Nodes[0] : null;
    }

    protected virtual Style OnResetStyle(bool darkMode)
    {
        throw new InvalidOperationException();
    }

    private void BResetColorSetting_Click(object sender, EventArgs e)
    {
        if (tscbStyles.SelectedIndex == -1)
            return;

        var selectedNode = tvStyles.SelectedNode;
        if (selectedNode?.Tag is not ColorSetting cs)
            return;

        var parent = selectedNode.Parent;
        var style = (Style)tscbStyles.SelectedItem!;
        var newStyle = OnResetStyle(style.Name == StyleNames.Dark);
        var categoryFromDef = StyleType.GetProperty(parent!.Name, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance)?.GetValue(newStyle);

        if (categoryFromDef?.GetType().GetProperty(selectedNode.Name)?.GetValue(categoryFromDef) is not ColorSetting settingDef)
            return;

        cs.BackColor = settingDef.BackColor;
        cs.ForeColor = settingDef.ForeColor;
        cs.Bold = settingDef.Bold;

        //reset controls
        tvStyles.SelectedNode = null;
        tvStyles.SelectedNode = selectedNode;
    }

    protected virtual Style? CreateStyleInstance(string name) => null;

    private void TsbRenameStyle_Click(object sender, EventArgs e)
    {
        var form = new FInputBox(Styles, (o, v) => v == ((Style) o).Name, tscbStyles.SelectedItem?.ToString() ?? "");
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            ((Style)tscbStyles.SelectedItem!).Name = form.NewValue;
            tscbStyles.ComboBox.ResetBindings();
            RefreshStyleChoice();
        }
    }

    private void TsbDeleteStyle_Click(object sender, EventArgs e)
    {
        if (UsingStyle == (Style) tscbStyles.SelectedItem!)
            UsingStyle = (Styles[0] as Style)!;

        Styles.Remove(tscbStyles.SelectedItem);
        tscbStyles.ComboBox.ResetBindings();
        RefreshStyleChoice();
    }
}