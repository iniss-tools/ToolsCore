using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using Microsoft.Win32;
using ToolsCore.Iniss.Tools;
using ToolsCore.Iniss.Updates;
using ToolsCore.Properties;
using ToolsCore.Tools;
using ToolsCore.XML;

namespace ToolsCore.Updates;

/// <summary>
/// Kontrola aktualizacii desktopoveho programu: pri spusteni (najviac raz za <see cref="CheckInterval" />, ak to nastavenie
/// dovoli) a prikazom z ponuky Pomocnik. Novu verziu ukaze oknom <see cref="FUpdateApp" /> - to stiahne instalator, program
/// sa zavrie (s otazkou na neulozene zmeny) a spusti sa ticha instalacia, ktora program po skonceni znova spusti
/// (AppSetup.iss, parameter /RELAUNCH). Funguje len v zostave z vydania (<see cref="UpdateSource" /> s adresou indexu).
/// </summary>
public static class AppUpdates
{
    /// <summary>Najkratsi cas medzi kontrolami pri spusteni.</summary>
    public static readonly TimeSpan CheckInterval = TimeSpan.FromHours(20);

    /// <summary>Kontrola pri spusteni pocka, kym sa program nacita.</summary>
    private static readonly TimeSpan StartupDelay = TimeSpan.FromSeconds(5);

    private static readonly Lazy<UpdateSource?> LazySource =
        new(() => Assembly.GetEntryAssembly() is { } entry ? UpdateSource.FromAssembly(entry) : null);

    private static HttpClient? _http;

    /// <summary>Zdroj aktualizacii programu; <c>null</c> = zostava mimo vydania, aktualizacie sa nekontroluju.</summary>
    public static UpdateSource? Source => LazySource.Value;

    /// <summary>Prikaz Skontrolovat aktualizacie ma zmysel - zostava z vydania.</summary>
    public static bool Available => Source is not null;

    private static HttpClient Http => _http ??= UpdateChecker.CreateClient(Source!);

    /// <summary>Priecinok, kam sa stahuje instalator novej verzie.</summary>
    private static string DownloadFolder => Path.Combine(Path.GetTempPath(), Source!.Program + "-update");

    /// <summary>
    /// Volat z <c>Load</c> hlavneho okna: po jeho zobrazeni zisti na pozadi novu verziu a ukaze ju, az ked je hlavne okno
    /// aktivne. Chyby siete ticho zaloguje.
    /// </summary>
    public static void CheckOnStartup(Form main, ConfigBase config)
    {
        ArgumentNullException.ThrowIfNull(main);
        ArgumentNullException.ThrowIfNull(config);
        if (Source is not { } source)
            return;
        DeleteDownloads();
        if (!config.CheckUpdates || AppRegistry.GetUpdateChecked() is { } last && DateTime.UtcNow - last < CheckInterval && last <= DateTime.UtcNow)
            return;
        main.Shown += OnShown;

        async void OnShown(object? sender, EventArgs e)
        {
            main.Shown -= OnShown;
            try
            {
                await Task.Delay(StartupDelay);
                if (main.IsDisposed)
                    return;
                var release = await UpdateChecker.FindNewerAsync(Http, source, CancellationToken.None);
                AppRegistry.SetUpdateChecked(DateTime.UtcNow);
                if (release is null || main.IsDisposed || string.Equals(release.Version, AppRegistry.GetUpdateSkipped(), StringComparison.Ordinal))
                    return;
                ShowWhenIdle(main, release);
            }
            catch (Exception ex) when (IsUpdateError(ex))
            {
                Log.Info("Kontrola aktualizacii zlyhala: " + ex.Message);
            }
        }
    }

    /// <summary>Prikaz Skontrolovat aktualizacie: zisti novu verziu hned a ukaze vysledok aj ked nova verzia nie je.</summary>
    public static async void CheckNow(Form main)
    {
        ArgumentNullException.ThrowIfNull(main);
        if (Source is null)
            return;
        UpdateRelease? release;
        main.UseWaitCursor = true;
        try
        {
            release = await UpdateChecker.FindNewerAsync(Http, Source, CancellationToken.None);
            AppRegistry.SetUpdateChecked(DateTime.UtcNow);
        }
        catch (Exception ex) when (IsUpdateError(ex))
        {
            Log.Exception(ex, "Kontrola aktualizacii");
            Utils.ShowWarning(string.Format(CultureInfo.CurrentCulture, Resources.Update_CheckFailed, ex.Message));
            return;
        }
        finally
        {
            main.UseWaitCursor = false;
        }

        if (release is null)
            Utils.ShowInfo(string.Format(CultureInfo.CurrentCulture, Resources.Update_UpToDate, Source.Program, ReleaseVersion.Format(Source.Version)));
        else
            Show(main, release);
    }

    /// <summary>Chyby siete, indexu a suborov pri kontrole a stahovani - ostatne su chyby programu.</summary>
    internal static bool IsUpdateError(Exception ex) =>
        ex is HttpRequestException or OperationCanceledException or JsonException or InvalidDataException or IOException or UnauthorizedAccessException;

    private static void ShowWhenIdle(Form main, UpdateRelease release)
    {
        if (IsIdle(main))
        {
            Show(main, release);
            return;
        }
        main.Activated += OnActivated;

        void OnActivated(object? sender, EventArgs e)
        {
            if (!IsIdle(main))
                return;
            main.Activated -= OnActivated;
            // az po dokonceni aktivacie - inak by dialog prisiel o fokus
            main.BeginInvoke(() => Show(main, release));
        }
    }

    /// <summary>Hlavne okno je aktivne a neblokuje ho ziadny modalny dialog.</summary>
    private static bool IsIdle(Form main) => !main.IsDisposed && main.Visible && main.CanFocus && Form.ActiveForm == main;

    private static void Show(Form main, UpdateRelease release)
    {
        var installed = InstalledHere();
        string? installer;
        using (var dialog = new FUpdateApp(Source!, release, Http, installed, DownloadFolder))
        {
            dialog.ShowDialog(main);
            if (dialog.Choice == UpdateChoice.Skip)
                AppRegistry.SetUpdateSkipped(release.Version);
            if (dialog.Choice != UpdateChoice.Install)
                return;
            installer = dialog.InstallerPath;
        }

        // zavretie sa moze zrusit (neulozene zmeny) - instalator sa potom nespusti
        main.Close();
        if (!main.IsDisposed || installer is null)
            return;
        try
        {
            Process.Start(new ProcessStartInfo(installer)
            {
                UseShellExecute = true,
                Arguments = "/SILENT /RELAUNCH=1 /LANG=" + InstallerLanguage()
            });
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or IOException)
        {
            Log.Exception(ex, "Spustenie instalatora");
            Utils.ShowError(string.Format(CultureInfo.CurrentCulture, Resources.Update_StartFailed, ex.Message));
        }
    }

    /// <summary>Jazyk sprievodcu instalacie podla jazyka programu (nazvy jazykov v Common.iss).</summary>
    private static string InstallerLanguage() => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName switch
    {
        "sk" => "slovak",
        "cs" => "czech",
        _ => "english"
    };

    /// <summary>
    /// Program nainstaloval instalator do priecinka, z ktoreho bezi - len vtedy ho instalator aktualizuje (inak sa nova
    /// verzia len stiahne cez stranku vydania). AppId instalatora je nazov programu (Common.iss).
    /// </summary>
    private static bool InstalledHere()
    {
        var uninstall = $@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{Source!.Program}_is1";
        var here = Path.TrimEndingDirectorySeparator(AppContext.BaseDirectory);
        foreach (var hive in new[] { RegistryHive.CurrentUser, RegistryHive.LocalMachine })
        {
            using var root = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
            using var key = root.OpenSubKey(uninstall);
            if (key?.GetValue("InstallLocation") is string location &&
                string.Equals(Path.TrimEndingDirectorySeparator(location), here, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    /// <summary>Instalator z minulej aktualizacie uz netreba; ak este bezi, zmaze sa pri dalsom spusteni.</summary>
    private static void DeleteDownloads()
    {
        try
        {
            if (Directory.Exists(DownloadFolder))
                Directory.Delete(DownloadFolder, true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
        }
    }
}
