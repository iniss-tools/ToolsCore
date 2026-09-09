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
    private static readonly string AppGuid = ((GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), true)[0]).Value;

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

        //Zistenie duplicitnej instancie programu (a ak je to v konfiguracii zakazane, ukoncit duplicitnu instanciu)
        using var mutex = new Mutex(false, "Global\\" + AppGuid);
        if (!mutex.WaitOne(0, false) && !config.MoreInstance)
        {
            Log.Info("Duplicitná inštancia ukončená");
            return;
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