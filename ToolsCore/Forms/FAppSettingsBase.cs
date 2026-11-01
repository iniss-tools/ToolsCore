using System.Globalization;
using ExControls;
using ToolsCore.Commands;
using ToolsCore.Properties;
using ToolsCore.Tools;
using ToolsCore.Updates;
using ToolsCore.XML;
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable UnusedParameter.Global
// ReSharper disable VirtualMemberNeverOverridden.Global

namespace ToolsCore.Forms;

public partial class FAppSettingsBase : Form
{
    private string? _preselectMenuItem;
    // vlastnosti nizsie nastavuje len "skutocny" konstruktor - bezparametrovy je len pre navrhara
    private bool _changingStyleSelection;

    // konfiguracia pri otvoreni okna - na zistenie zmien, ktore sa prejavia az po restarte
    private ConfigBase _original = null!;

    public ConfigBase Config { get; } = null!;
    /// <summary>
    /// Kopia farebnych schem a vyberu pouzivanej schemy.
    /// </summary>
    public IStyleSet Styles { get; } = null!;

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
    /// Popis pouzitia pisma editora na stranke Pisma; <see langword="null" /> = program editor nema.
    /// </summary>
    protected virtual string? EditorFontText => null;

    protected bool ShouldRestart { get; set; }

    /// <summary>
    /// Nastavenia sa ulozili (tlacidlo Ulozit alebo Pouzit) - hlavne okno ich prevezme.
    /// </summary>
    public event EventHandler? Applied;
    /// <summary>
    /// Pouzivana schema - po ulozeni podla volby na stranke Vzhlad (a rezimu Windows).
    /// </summary>
    public Style UsingStyle { get; private set; } = null!;

    /// <summary>
    /// Not use - only for designer
    /// </summary>
    public FAppSettingsBase()
    {
        InitializeComponent();
    }

    public FAppSettingsBase(ConfigBase config, IStyleSet copiedStyles, Style usingStyle)
    {
        InitializeComponent();

        _original = config;
        Config = config with {};
        Styles = copiedStyles;

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

        keymapPage.LoadCommands(KeymapTree.Build(_commandCatalog, _boundCommands, Resources.Keymap_MainWindow), Config.Shortcuts);
        configBindingSource.DataSource = Config;
        fontsPage.LoadFonts(Config.Fonts, EditorFontText);
        RefreshLastCheck();
        RefreshStyleChoice();
        colorSchemePage.LoadSchemes(Styles, Styles.Find(UsingStyle.Name) ?? Styles[0]);
    }

    private void FAppSettingsBase_Load(object sender, EventArgs e)
    {
        if (DesignMode) 
            return;

        this.ApplyThemeAndFonts();
        FormUtils.ChangeColorContextMenu(GlobSettings.UsingStyle, cmsManage);

        optionsView.TreeView.ExpandAll();
        OnLoad();
    }

    public void PreselectMenuItem(string name) => _preselectMenuItem = name;

    /// <summary>
    /// Skupiny a popisy roztiahnute na sirku stranky (s kotvou vpravo) siahaju po pravy okraj ako skupina zo zakladu
    /// okna - kotvy z navrhu pocitaju s inou sirkou panela, preto sa sirka nastavuje podla skutocneho panela.
    /// Plati pre obsah vsetkych stranok (panel vyplnajuci stranku) vratane stranok programu.
    /// </summary>
    private void FitPageGroups()
    {
        foreach (var page in optionsView.Panels.OfType<ExOptionsPanel>())
        foreach (var content in page.Controls.OfType<Panel>().Where(c => c.GetType() == typeof(Panel) && c.Dock == DockStyle.Fill))
        {
            // skupiny vzdy, ostatne prvky len roztiahnute v navrhu (kotva vlavo aj vpravo)
            var stretched = content.Controls.Cast<Control>()
                .Where(c => c.Dock == DockStyle.None && (c is GroupBox || c.Anchor.HasFlag(AnchorStyles.Left | AnchorStyles.Right))).ToList();
            if (stretched.Count == 0)
                continue;

            foreach (var control in stretched)
                control.Anchor &= ~AnchorStyles.Right;
            content.ClientSizeChanged += (_, _) => FitWidth(content, stretched);
            FitWidth(content, stretched);
        }
    }

    private static void FitWidth(Control content, List<Control> stretched)
    {
        foreach (var control in stretched)
            control.Width = Math.Max(control.MinimumSize.Width, content.ClientSize.Width - control.Left);
    }

    protected virtual void OnLoad()
    {
        LoadComponents();
        FitPageGroups();

        if (!string.IsNullOrWhiteSpace(_preselectMenuItem)) 
            optionsView.ShowPanel(_preselectMenuItem);
    }

    private void FAppSettingsBase_HelpButtonClicked(object sender, CancelEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(Config.LinkAppSettingsGuide))
            return;

        Utils.OpenShell(Config.LinkAppSettingsGuide);
    }

    private void BSave_Click(object sender, EventArgs e) => DialogResult = Commit() ? DialogResult.OK : DialogResult.None;

    private void BApply_Click(object? sender, EventArgs e)
    {
        // okno nastaveni prevezme pripadnu novu schemu ako ostatne okna
        if (Commit())
            this.ApplyThemeAndFonts();
    }

    /// <summary>
    /// Ulozi nastavenia a oznami ich hlavnemu oknu (<see cref="Applied" />); okno ostava otvorene.
    /// </summary>
    /// <returns><see langword="false" />, ak pouzivatel ulozenie zrusil.</returns>
    private bool Commit()
    {
        configBindingSource.EndEdit();
        Config.Startup = cboxReopenLast.Checked ? StartupType.LastProject : StartupType.EmptyWindow;
        fontsPage.SaveTo(Config.Fonts);
        ShouldRestart = Config.Language != _original.Language || !Equals(Config.Fonts.Interface, _original.Fonts.Interface);
        // svetla schema ma systemovy vzhlad prvkov - po tmavej (vlastne kreslenie) sa obnovi len restartom
        var style = Styles.Current(SystemTheme.IsDark);
        if (UsingStyle.IsDark && !style.IsDark)
            ShouldRestart = true;
        var previous = UsingStyle;
        UsingStyle = style;
        if (!OnSaving())
        {
            UsingStyle = previous;
            return false;
        }

        SaveData();
        // tabulky a editory prevezmu pisma hned; pismo rozhrania az po restarte (meni rozlozenie okien)
        GlobSettings.Fonts = Config.Fonts with { Interface = GlobSettings.Fonts.Interface };
        FormUtils.RefreshFonts();
        _original = Config with { };
        Applied?.Invoke(this, EventArgs.Empty);
        return true;
    }

    #region Aktualizacie

    private void RefreshLastCheck()
    {
        // vyvojova zostava aktualizacie nekontroluje (nema index vydania)
        bCheckNow.Enabled = AppUpdates.Available;
        lLastCheck.Text = !AppUpdates.Available ? Resources.Settings_UpdatesUnavailable
            : AppRegistry.GetUpdateChecked() is { } last
                ? string.Format(CultureInfo.CurrentCulture, Resources.Settings_LastUpdateCheck, last.ToLocalTime().ToString("g", CultureInfo.CurrentCulture))
                : Resources.Settings_NeverUpdateChecked;
    }

    private async void BCheckNow_Click(object? sender, EventArgs e)
    {
        bCheckNow.Enabled = false;
        try
        {
            await AppUpdates.CheckNowAsync(this);
        }
        finally
        {
            if (!IsDisposed)
                RefreshLastCheck();
        }
    }

    #endregion

    #region Sprava nastaveni

    private void TsmiExport_Click(object? sender, EventArgs e)
    {
        var program = AppPaths.AppName;
        using var dialog = new SaveFileDialog
        {
            Filter = Resources.SettingsTransfer_Filter,
            FileName = string.Format(CultureInfo.CurrentCulture, Resources.SettingsTransfer_FileName, program,
                DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            SettingsTransfer.Export(dialog.FileName, AppPaths.ConfigDir, program);
            Utils.ShowInfo(string.Format(CultureInfo.CurrentCulture, Resources.SettingsTransfer_Exported, dialog.FileName));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            ErrorReport.Show(ex, this);
        }
    }

    private void TsmiImport_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog { Filter = Resources.SettingsTransfer_Filter };
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        if (SettingsTransfer.Check(dialog.FileName, AppPaths.AppName) is { } error)
        {
            Utils.ShowError(error);
            return;
        }

        var question = string.Format(CultureInfo.CurrentCulture, Resources.SettingsTransfer_ImportConfirm, Path.GetFileName(dialog.FileName));
        if (Utils.ShowQuestion(question) != DialogResult.Yes)
            return;

        try
        {
            SettingsTransfer.ScheduleImport(dialog.FileName, AppPaths.ConfigDir);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            ErrorReport.Show(ex, this);
            return;
        }

        OfferRestart();
    }

    private void TsmiResetAll_Click(object? sender, EventArgs e)
    {
        if (Utils.ShowQuestion(Resources.SettingsTransfer_ResetConfirm) != DialogResult.Yes)
            return;

        try
        {
            SettingsTransfer.ScheduleReset(AppPaths.ConfigDir);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            ErrorReport.Show(ex, this);
            return;
        }

        OfferRestart();
    }

    /// <summary>
    /// Po pripravenom importe alebo obnove sa okno zavrie bez ulozenia (zmeny v nom by aj tak nahradili nastavenia
    /// zo suboru) a ponukne restart.
    /// </summary>
    private void OfferRestart()
    {
        configBindingSource.CancelEdit();
        DialogResult = DialogResult.Cancel;
        if (Utils.ShowQuestion(Resources.SettingsTransfer_RestartNow) == DialogResult.Yes)
            Utils.RestartApp();
    }

    #endregion

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

    #region Vzhlad a farebna schema

    /// <summary>
    /// Zoznamy schem na stranke Vzhlad (zvolena schema, schema pre svetly a tmavy rezim Windows).
    /// </summary>
    private void RefreshStyleChoice()
    {
        _changingStyleSelection = true;
        Bind(cbStyle, Styles.Active);
        Bind(cbLightScheme, Styles.LightScheme);
        Bind(cbDarkScheme, Styles.DarkScheme);
        cboxFollowSystem.Checked = Styles.FollowSystem;
        _changingStyleSelection = false;
        RefreshFollowSystem();
    }

    private void Bind(ComboBox combo, string selected)
    {
        var list = Styles.Cast<Style>().ToList();
        combo.DataSource = null;
        combo.DataSource = list;
        combo.SelectedItem = list.FirstOrDefault(s => s.Name == selected) ?? list[0];
    }

    private void RefreshFollowSystem()
    {
        // popisy ostavaju povolene - zakazany Label kresli WinForms systemovou sedou, v tmavej teme necitatelne
        var follow = cboxFollowSystem.Checked;
        cbStyle.Enabled = !follow;
        cbLightScheme.Enabled = cbDarkScheme.Enabled = follow;
    }

    private void CbStyle_SelectionChangeCommitted(object? sender, EventArgs e)
    {
        if (cbStyle.SelectedItem is not Style style)
            return;

        Styles.Active = style.Name;
        colorSchemePage.SelectScheme(style);
    }

    private void CboxFollowSystem_CheckedChanged(object? sender, EventArgs e)
    {
        if (_changingStyleSelection)
            return;

        Styles.FollowSystem = cboxFollowSystem.Checked;
        RefreshFollowSystem();
    }

    private void CbSystemScheme_SelectionChangeCommitted(object? sender, EventArgs e)
    {
        if (cbLightScheme.SelectedItem is Style light)
            Styles.LightScheme = light.Name;
        if (cbDarkScheme.SelectedItem is Style dark)
            Styles.DarkScheme = dark.Name;
    }

    private void ColorSchemePage_SchemesChanged(object? sender, EventArgs e) => RefreshStyleChoice();

    #endregion
}
