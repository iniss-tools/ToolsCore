using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;
using ExControls;
using ToolsCore.Forms;
using ToolsCore.Iniss.Tools;
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
    /// Kluc instancie podla spusteneho programu (nie ToolsCore) - kazdy nastroj ma vlastny.
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

    /// <summary>
    /// Pripravi program na spustenie: log, konfiguraciu a styly (poskodene nahradi predvolenymi), jazyk, vzhlad
    /// MessageBoxov a kontrolu duplicitnej instancie.
    /// </summary>
    /// <returns>nastavenia programu platne po cely jeho beh</returns>
    public static AppSession<TC, TS> Initialization<TC, TS>()
        where TC : ConfigBase, new() where TS : Style
    {
        //Nastavenie cesty, kde sa budu ukladat logovacie subory - musi byt prve,
        //aby uz aj chyba pri nacitani konfiguracie mala kam zapisat
        Log.DataDirPath = AppPaths.DataDir;

        //Nastavenie cesty, kde sa maju ukladat konfiguracne subory
        var configsDir = AppPaths.ConfigDir;
        if (!Directory.Exists(configsDir))
            Directory.CreateDirectory(configsDir);

        // poskodeny konfiguracny subor sa odlozi a program zacne s predvolenym - inak by sa vobec nespustil
        var resetFiles = new List<string>();

        //nacitanie konfiguracneho suboru CONFIG.XML
        var config = ReadOrReset(PathUtils.CombinePath(configsDir, FileConsts.FILE_CONFIG)!, XmlSerialization.ReadData<TC>, resetFiles);
        GlobSettings.Fonts = config.Fonts;

        // Predvolene pismo aplikacie = pismo formularov z konfiguracie. Formulare (AutoScaleDimensions podla pisma
        // z navrhu) sa tak preskaluju uz pri vytvoreni a SetFormFont potom pismo nemeni.
        // Musi sa volat pred vytvorenim prveho okna.
        Application.SetDefaultFont(config.Fonts.Labels.Font);

        //Nastavenie, ci sa maju ukladat logy do suborov podla konfiguracie
        Log.DoAppLogs = config.LoggingInfo;
        Log.DoErrorLogs = config.LoggingError;

        //nacitanie suboru so stylmi STYLES.XML
        var styles = ReadOrReset(PathUtils.CombinePath(configsDir, FileConsts.FILE_STYLES)!, Styles<TS>.ReadData, resetFiles);
        var usingStyle = styles.FirstOrDefault(s => s.Used) ?? styles.First();
        GlobSettings.UsingStyle = usingStyle;

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

        // oznamenie az v Run - okno vytvorene teraz by uz nedovolilo nastavit sposob spracovania vynimiek
        _resetFiles = resetFiles;
        return new AppSession<TC, TS>(config, styles, usingStyle);
    }

    // subory, ktore boli pri spusteni poskodene a nahradili sa predvolenymi
    private static List<string> _resetFiles = [];

    /// <summary>
    /// Spusti hlavne okno programu. Neosetrene vynimky (aj z obsluh udalosti a inych vlakien) zaloguje a podla
    /// <see cref="ConfigBase.DebugModeGUI" /> zobrazi; pri <see cref="DebugMode.AppCrash" /> program spadne.
    /// Volat po <see cref="Initialization{TC,TS}" />.
    /// </summary>
    /// <param name="config">konfiguracia programu</param>
    /// <param name="createMainForm">vytvori hlavne okno - az po nastaveni spracovania vynimiek</param>
    public static void Run(ConfigBase config, Func<Form> createMainForm)
    {
        var crash = config.DebugModeGUI == DebugMode.AppCrash;

        // musi byt pred vytvorenim prveho okna
        Application.SetUnhandledExceptionMode(crash ? UnhandledExceptionMode.ThrowException : UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) => ReportException(e.Exception, config);
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            // vlakno mimo UI - program sa aj tak ukonci, aspon nech je zaznam
            if (e.ExceptionObject is Exception exception)
                Log.Exception(exception);
        };
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            Log.Exception(e.Exception);
            e.SetObserved();
        };

        if (_resetFiles.Count > 0)
            Utils.ShowWarning(string.Format(GlobalResources.Global_ConfigReset, string.Join(", ", _resetFiles)));

        if (crash)
        {
            Application.Run(createMainForm());
        }
        else
        {
            try
            {
                Application.Run(createMainForm());
            }
            catch (Exception exception)
            {
                // napr. chyba v konstruktore hlavneho okna - nie je v slucke sprav, ThreadException ju nezachyti
                ReportException(exception, config);
            }
        }

        Log.Info("Program sa ukončuje\r\n");
    }

    private static void ReportException(Exception exception, ConfigBase config)
    {
        Log.Exception(exception);
        try
        {
            FError.ShowError(config.DebugModeGUI == DebugMode.OnlyMessage ? exception.Message : exception.ToString());
        }
        catch (Exception displayError)
        {
            // chyba v samotnom okne chyby by sa inak zacyklila cez ThreadException
            Log.Exception(displayError);
        }
    }

    /// <summary>
    /// Nacita konfiguracny subor. Ak je poskodeny, odlozi ho vedla ako <c>.bad</c> a nacita predvoleny.
    /// </summary>
    private static T ReadOrReset<T>(string fileName, Func<string, T> read, List<string> resetFiles)
    {
        try
        {
            return read(fileName);
        }
        catch (Exception e) when (e is InvalidOperationException or XmlException or ArgumentException or FormatException)
        {
            var bad = fileName + ".bad";
            Log.Error($"Súbor {fileName} je poškodený a bol odložený ako {bad}: {e.Message}");
            File.Move(fileName, bad, true);
            resetFiles.Add(Path.GetFileName(fileName));
            return read(fileName);
        }
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