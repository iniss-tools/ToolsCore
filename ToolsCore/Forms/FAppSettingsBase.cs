using System.Collections;
using System.Reflection;
using ExControls;
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

    [Localizable(true)] 
    private readonly string _lShortcutHelp1 = Resources.FAppSettings_lShortcutHelp1;

    [Localizable(true)] 
    private readonly string _lShortcutHelp2 = Resources.FAppSettings_lShortcutHelp2;

    private string? _preselectMenuItem;
    private int _shortcutSetIndex = -1;
    // _allSystemFonts/_monospacedFonts, and the properties below, are only ever populated by the
    // "real" constructor - the parameterless one exists purely for the WinForms visual designer and
    // is documented as such ("Not use - only for designer"), so they're guaranteed non-null in real use.
    private readonly List<string> _allSystemFonts = null!;
    private List<string> _monospacedFonts = null!;
    private bool _changingStyleSelection;

    public ConfigBase Config { get; } = null!;
    public IList Styles { get; } = null!;
    public Type StyleType { get; } = null!;
    protected ExBindingList<CmdShortcut> Shortcuts { get; set; } = null!;
    protected ExBindingList<DesktopColumn> Columns { get; set; } = null!;
    protected virtual IList<CmdShortcut> DefaultShortcuts => new List<CmdShortcut>();
    protected virtual IList<DesktopColumn> DefaultColumns => new List<DesktopColumn>();
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

        Config = config with {};
        Styles = copiedStyles;
        StyleType = styleType;

        optionsView.HeaderNodeNameFont = new Font(optionsView.HeaderNodeNameFont, FontStyle.Bold);

        _allSystemFonts = Utils.GetSystemFontNames();
        InitMonospacedFonts();

        UsingStyle = usingStyle;
    }

    protected override CreateParams CreateParams
    {
        get
        {
            var handleParam = base.CreateParams;
            if (!DesignMode) 
                handleParam.ExStyle |= 0x02000000; // WS_EX_COMPOSITED 
            return handleParam;
        }
    }

    private void LoadComponents()
    {
        switch (Config.DesktopMenuMode)
        {
            case DesktopMenu.MsTs:
                rbMenuToolStrip.Checked = true;
                break;
            case DesktopMenu.TsOnly:
                rbToolOnly.Checked = true;
                break;
            case DesktopMenu.MsOnly:
                rbMenuOnly.Checked = true;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        cbAppLanguage.BindEnum(new Dictionary<AppLanguage, string>
        {
            [AppLanguage.Slovak] = "Slovenčina",
            [AppLanguage.Czech] = "Čeština" 
        });

        cbShowErrors.BindEnum(new Dictionary<DebugMode, string>
        {
            [DebugMode.OnlyMessage] = "Iba krátka správa",
            [DebugMode.DetailInfo] = "Detailné informácie",
            [DebugMode.AppCrash] = "Spadnutie programu (systémový dialóg)"
        });

        cbStartup.BindEnum(new Dictionary<StartupType, string>
        {
            [StartupType.EmptyWindow] = "Prázdne okno",
            [StartupType.LastProject] = "Posledný projekt"
        });

        configBindingSource.DataSource = Config;
        pgFonts.SelectedObject = Config.Fonts;
        tvStyles.SelectedNode = tvStyles.Nodes.Count == 0 ? null : tvStyles.Nodes[0];
    }

    private void FAppSettingsBase_Load(object sender, EventArgs e)
    {
        if (DesignMode) 
            return;

        this.ApplyThemeAndFonts();

        lShortcutMsg.Text = "Vyberte skratku zo zoznamu";
        if (Shortcuts is not null) 
            FindAndCheckDuplicateShortcuts();

        cbFont.DataSource = _allSystemFonts;
        tscbStyles.ComboBox.DataSource = Styles;
        tscbStyles.SelectedItem = Styles.Cast<Style>().FirstOrDefault(s => s.Name == UsingStyle.Name);
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
        var result = OnSaving();
        DialogResult = result ? DialogResult.OK : DialogResult.None;
        if (result) SaveData();
    }

    private void BStorno_Click(object sender, EventArgs e)
    {
        configBindingSource.CancelEdit();
        DialogResult = DialogResult.Cancel;
    }

    private void PgFonts_SelectedGridItemChanged(object sender, SelectedGridItemChangedEventArgs e)
    {
        if (e.NewSelection?.Value is not AppFont fnt)
            return;

        lFontExample.Font = fnt.Font;
    }

    private void DgvShortcuts_CellContentClick(object sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0)
            return;
        var shortcut = (CmdShortcut) dgvShortcuts.Rows[e.RowIndex].DataBoundItem!;
        switch (dgvShortcuts.Columns[e.ColumnIndex].Name)
        {
            case nameof(cShortcutAttach):
                OnShortcutAttach(e.RowIndex, shortcut);
                break;
            case nameof(cShortcutDeattach):
                OnShortcutDeattach(e.RowIndex, shortcut);
                break;
            case nameof(cShortcutReset):
                OnShortcutReset(e.RowIndex, shortcut);
                break;
        }
    }

    protected virtual void OnShortcutAttach(int index, CmdShortcut shortcut)
    {
        _shortcutSetIndex = index;
        lShortcutMsg.Text = _lShortcutHelp1;
        KeyPreview = true;
    }

    protected virtual void OnShortcutDeattach(int index, CmdShortcut shortcut)
    {
        shortcut.Shortcut = Shortcut.None;
        Shortcuts.ResetBindings();
    }

    protected virtual void OnShortcutReset(int index, CmdShortcut shortcut)
    {
        shortcut.Shortcut = DefaultShortcuts.First(s => s.PropertyName == shortcut.PropertyName).Shortcut;
        Shortcuts.ResetBindings();
    }

    protected virtual bool OnSaving()
    {
        if (ShouldRestart)
        {
            var result = Utils.ShowQuestion("Vykonali ste zmeny vyžadujúce reštartovanie programu.\n\nReštartovať program teraz?", 
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

    private void FAppSettingsBase_KeyDown(object sender, KeyEventArgs e)
    {
        if (_shortcutSetIndex == -1)
            return;

        if (Utils.ValidateShortcut(e.KeyData))
        {
            Shortcuts[_shortcutSetIndex].Shortcut = (Shortcut) e.KeyData;
            Shortcuts.ResetBindings();

            FindAndCheckDuplicateShortcuts();

            KeyPreview = false;
            _shortcutSetIndex = -1;
            lShortcutMsg.Text = "";
        }
        else
        {
            lShortcutMsg.Text = _lShortcutHelp2;
        }

        e.Handled = true;
    }

    private void FindAndCheckDuplicateShortcuts()
    {
        ResetColorShortcutDuplicates();

        for (var i = 0; i < Shortcuts.Count; i++)
        {
            var s1 = Shortcuts[i].Shortcut.Value;
            for (var j = 0; j < Shortcuts.Count; j++)
            {
                var s2 = Shortcuts[j].Shortcut.Value;
                if (i != j && s1 == s2 && s1 != Shortcut.None)
                {
                    dgvShortcuts.Rows[i].Cells["cShortcut"].Style.ForeColor = Color.Red;
                    dgvShortcuts.Rows[j].Cells["cShortcut"].Style.ForeColor = Color.Red;
                }
            }
        }
    }

    private void ResetColorShortcutDuplicates()
    {
        for (var i = 0; i < Shortcuts.Count; i++) 
            dgvShortcuts.Rows[i].Cells["cShortcut"].Style.ForeColor = dgvShortcuts.ForeColor;
    }

    private void BShortcutsReset_Click(object sender, EventArgs e)
    {
        foreach (var shortcut in Shortcuts)
            shortcut.Shortcut = DefaultShortcuts.First(s => s.PropertyName == shortcut.PropertyName).Shortcut;
        Shortcuts.ResetBindings();
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
        cboxHighlightStatusBar.Checked = style.HighlightStatusBar;

        if (style.Name is StyleNames.LIGHT or StyleNames.DARK)
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

    private void CboxHighlightStatusBar_CheckedChanged(object sender, EventArgs e)
    {
        var style = (Style?)tscbStyles.SelectedItem;
        if (style is not null)
            style.HighlightStatusBar = cboxHighlightStatusBar.Checked;
    }

    protected virtual void FillTreeViewStyles(object selectedStyle, ExTreeView tv)
    {
        if (selectedStyle is not Style style)
            return;

        tv.Nodes.Clear();
        var controls = CreateParentNode(style.ControlsColorScheme, nameof(style.ControlsColorScheme));
        CreateNode(style.ControlsColorScheme.Label, nameof(style.ControlsColorScheme.Label), controls);
        CreateNode(style.ControlsColorScheme.Panel, nameof(style.ControlsColorScheme.Panel), controls);
        CreateNode(style.ControlsColorScheme.Border, nameof(style.ControlsColorScheme.Border), controls);
        CreateNode(style.ControlsColorScheme.Box, nameof(style.ControlsColorScheme.Box), controls);
        CreateNode(style.ControlsColorScheme.Button, nameof(style.ControlsColorScheme.Button), controls);
        CreateNode(style.ControlsColorScheme.Mark, nameof(style.ControlsColorScheme.Mark), controls);
        CreateNode(style.ControlsColorScheme.Highlight, nameof(style.ControlsColorScheme.Highlight), controls);
        tv.Nodes.Add(controls);
    }

    public TreeNode CreateParentNode(IColorScheme scheme, string name)
    {
        var node = new TreeNode();
        node.Text = scheme.Name;
        node.Name = name;
        node.Tag = scheme;
        return node;
    }

    public void CreateNode(ColorSetting setting, string name, TreeNode parent)
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

        if (node.Tag is IColorScheme sc)
        {
            //tag je katagoria poloziek
            gbCategorySettings.Enabled = !sc.DisableFontEdit;
            gbColorItemSettings.Enabled = false;
            cbFont.SelectedItem = sc.DisableFontEdit ? null : sc.Font.FontFamily.Name;
            nudFontSize.Value = sc.DisableFontEdit ? 1 : (decimal) sc.Font.Size;
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
        gbCategorySettings.Enabled = false;
        gbColorItemSettings.Enabled = true;
        cbFont.SelectedItem = (node.Parent?.Tag as IColorScheme)?.Font?.FontFamily.Name;
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

    private void CboxOnlyNonPropFont_CheckedChanged(object sender, EventArgs e)
    {
        var selected = cbFont.SelectedItem;
        var fonts = cboxOnlyNonPropFont.Checked ? _monospacedFonts : _allSystemFonts;
        cbFont.DataSource = fonts;
        cbFont.SelectedIndex = fonts.IndexOf(selected);
    }

    private void InitMonospacedFonts()
    {
        var graphics = CreateGraphics();
        _monospacedFonts = new List<string>(_allSystemFonts);
        foreach (var fontStr in _allSystemFonts)
        {
            var font = new Font(fontStr, 12);
            if (!Utils.IsFontMonospaced(graphics, font)) 
                _monospacedFonts.Remove(fontStr);
        }
    }

    private void CsForeColor_SelectedColorChanged(object sender, EventArgs e)
    {
        RefreshColorSetting();
    }

    private void CsBackColor_SelectedColorChanged(object sender, EventArgs e)
    {
        RefreshColorSetting();
    }

    private void CbFont_SelectedIndexChanged(object sender, EventArgs e)
    {
        RefreshColorSetting();
    }

    private void CboxBold_CheckedChanged(object sender, EventArgs e)
    {
        RefreshColorSetting();
    }

    private void NudFontSize_ValueChanged(object sender, EventArgs e)
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
        if (selectedNode.Tag is IColorScheme scheme)
        {
            if (!scheme.DisableFontEdit && !_changingStyleSelection)
            {
                var fam = new FontFamily(cbFont.SelectedItem?.ToString() ?? string.Empty);
                scheme.Font = new Font(fam, decimal.ToInt32(nudFontSize.Value), FontStyle.Regular);
            }
            return;
        }

        //je vybrata polozka kategorie
        var fnt = (selectedNode.Parent?.Tag as IColorScheme)?.Font ?? SystemFonts.DefaultFont;
        lFontStyleExample.ForeColor = csForeColor.SelectedColor;
        lFontStyleExample.BackColor = csBackColor.SelectedColor;
        lFontStyleExample.Font = new Font(fnt, cboxBold.Checked ? FontStyle.Bold : FontStyle.Regular);

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
        var previousStyle = UsingStyle;
        UsingStyle = (tscbStyles.SelectedItem as Style)!;
        if (UsingStyle is null)
            return;

        foreach (Style style in Styles) 
            style.Used = false;

        UsingStyle.Used = true;
        if (!previousStyle.ControlsDefaultStyle && UsingStyle.ControlsDefaultStyle)
        {
            Utils.ShowInfo("Zmeny sa prejavia úplne až po reštartovaní programu.");
            ShouldRestart = true;
        }
        tsbApplyStyle.Enabled = false;
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
        }
    }

    private void TsbResetStyle_Click(object sender, EventArgs e)
    {
        if (tscbStyles.SelectedIndex == -1)
            return;

        var style = (Style)tscbStyles.SelectedItem!;
        var newStyle = OnResetStyle(style.Name == StyleNames.DARK);
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
        var newStyle = OnResetStyle(style.Name == StyleNames.DARK);
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
        }
    }

    private void TsbDeleteStyle_Click(object sender, EventArgs e)
    {
        if (UsingStyle == (Style) tscbStyles.SelectedItem!)
            UsingStyle = (Styles[0] as Style)!;

        Styles.Remove(tscbStyles.SelectedItem);
        tscbStyles.ComboBox.ResetBindings();
    }
}