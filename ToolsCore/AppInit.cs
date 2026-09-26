using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using ExControls;
using ToolsCore.Tools;
using ToolsCore.XML;
using Application = System.Windows.Forms.Application;
using Style = ToolsCore.XML.Style;

namespace ToolsCore;

public static class AppInit
{
    // mutex musi zit cely beh programu - inak ho druha instancia hned ziska a duplicitu nezisti
    private static Mutex? _instanceMutex;

    /// <summary>
    ///     Kluc instancie podla spusteneho programu (nie ToolsCore) - kazdy nastroj ma vlastny.
    /// </summary>
    private static string InstanceKey
    {
        get
        {
            var entry = Assembly.GetEntryAssembly();
            var guid = entry?.GetCustomAttribute<GuidAttribute>()?.Value;
            return guid ?? entry?.GetName().Name ?? Application.ProductName ?? "ToolsCore";
        }
    }

    public static void Initialization<TC,TS>(out TC config, out Styles<TS> styles, out TS usingStyle) 
        where TC: ConfigBase, new() where TS : Style
    {
        //Nastavenie cesty, kde sa budu ukladat logovacie subory - musi byt prve,
        //aby uz aj chyba pri nacitani konfiguracie mala kam zapisat
        Log.DataDirPath = AppPaths.DataDir;

        //Nastavenie cesty, kde sa maju ukladat konfiguracne subory
        var configsDir = AppPaths.ConfigDir;
        if (!Directory.Exists(configsDir))
            Directory.CreateDirectory(configsDir);

        //nacitanie konfiguracneho suboru CONFIG.XML
        try
        {
            config = XmlSerialization.ReadData<TC>(Utils.CombinePath(configsDir, FileConsts.FILE_CONFIG)!);
            GlobSettings.Fonts = config.Fonts;
        }
        catch (Exception e)
        {
            Log.Error($"Chyba pri načítaní konfiguračného súboru: {e.Message}");
            throw;
        }

        // Predvolene pismo aplikacie = pismo formularov z konfiguracie. Formulare (AutoScaleDimensions podla pisma
        // z navrhu) sa tak preskaluju uz pri vytvoreni a SetFormFont potom pismo nemeni.
        // Musi sa volat pred vytvorenim prveho okna.
        Application.SetDefaultFont(config.Fonts.Labels.Font);

        //Nastavenie, ci sa maju ukladat logy do suborov podla konfiguracie
        Log.DoAppLogs = config.LoggingInfo;
        Log.DoErrorLogs = config.LoggingError;

        //nacitanie suboru so stylmi STYLES.XML
        try
        {
            styles = Styles<TS>.ReadData(Utils.CombinePath(configsDir, FileConsts.FILE_STYLES)!);
            usingStyle = styles.FirstOrDefault(s => s.Used) ?? styles.First();
            GlobSettings.UsingStyle = usingStyle;
        }
        catch (Exception e)
        {
            Log.Error($"Chyba pri načítaní súboru so štýlmi: {e.Message}");
            throw;
        }

        //Nastavenie dizajnu ovladacich prvkov
        if (config.ClassicGUI)
        {
            Application.SetCompatibleTextRenderingDefault(true);
        }
        else
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
        }

        //Nastavenie CultureInfo podla konfiguracie
        var culture = CultureInfo.CreateSpecificCulture(config.Language == AppLanguage.Czech ? "cs" : "sk");
        Thread.CurrentThread.CurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        //Nastavenie textov tlacitok na MessageBoxoch podla lokalizacie
        ExMessageBox.ButtonOKText = GlobalResources.Global_OK;
        ExMessageBox.ButtonCancelText = GlobalResources.Global_Cancel;
        ExMessageBox.ButtonRetryText = GlobalResources.Global_Retry;
        ExMessageBox.ButtonIgnoreText = GlobalResources.Global_Ignore;
        ExMessageBox.ButtonAbortText = GlobalResources.Global_Abort;
        ExMessageBox.ButtonYesText = GlobalResources.Global_Yes;
        ExMessageBox.ButtonNoText = GlobalResources.Global_No;

        //Nastavenie vizualu MessageBoxov
        MsgBoxStyleInit(usingStyle, config);

        //Zistenie duplicitnej instancie programu - ak je v konfiguracii zakazana, program sa ukonci
        //(az po nastaveni jazyka a MessageBoxov, aby sa dala zobrazit sprava)
        _instanceMutex = new Mutex(false, "Global\\" + InstanceKey);
        bool owned;
        try
        {
            owned = _instanceMutex.WaitOne(0, false);
        }
        catch (AbandonedMutexException)
        {
            // predchadzajuca instancia spadla bez uvolnenia - mutex teraz patri tejto
            owned = true;
        }

        if (!owned && !config.MoreInstance)
        {
            Log.Info("Duplicitná inštancia ukončená");
            Utils.ShowInfo(string.Format(GlobalResources.Global_AppAlreadyRunning, Application.ProductName));
            Environment.Exit(0);
        }

        //Konfiguracia ukoncena
        Log.Info($"Program spustený - v.{Application.ProductVersion} - \"{Application.ExecutablePath}\"");
    }

    public static void MsgBoxStyleInit(Style usingStyle, ConfigBase config)
    {
        //Nastavenie vizualu MessageBoxov
        ExMessageBox.Style = new ExMessageBoxStyle
        {
            UseDarkTitleBar = usingStyle.DarkTitleBar,
            ForeColor = usingStyle.ControlsColorScheme.Box.ForeColor,
            BackColor = usingStyle.ControlsColorScheme.Box.BackColor,
            FooterBackColor = usingStyle.ControlsColorScheme.Panel.BackColor,
            DefaultStyle = usingStyle.ControlsDefaultStyle,
            ButtonBackColor = usingStyle.ControlsColorScheme.Button.BackColor,
            ButtonBorderColor = usingStyle.ControlsColorScheme.Border.ForeColor,
            ButtonForeColor = usingStyle.ControlsColorScheme.Button.ForeColor,
            ButtonBorderSize = 1,
            ButtonMouseDownColor = usingStyle.ControlsColorScheme.Highlight.BackColor,
            ButtonMouseOverColor = usingStyle.ControlsColorScheme.Highlight.BackColor,
            ButtonsFont = config.Fonts.Buttons
        };
    }
}