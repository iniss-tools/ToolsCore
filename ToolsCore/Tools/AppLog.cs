using System.Diagnostics;
using System.Globalization;
using Serilog;
using Serilog.Events;
using ToolsCore.Iniss.Tools;

namespace ToolsCore.Tools;

/// <summary>
/// Zaznamy desktopoveho programu cez Serilog: denne subory v priecinku <c>logs</c> udajov programu.
/// </summary>
/// <remarks>
/// Kod zapisuje cez staticky <see cref="Log" /> (aj domena v ToolsCore.Iniss). Do spustenia <see cref="Start" />
/// sa zaznamy zahadzuju - preto ho <c>AppInit.Initialization</c> vola hned na zaciatku.
/// </remarks>
public static class AppLog
{
    private const string Template = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] ({ProcessId}) {Message:lj}{NewLine}{Exception}";

    /// <summary>
    /// Pocet dennych suborov, ktore sa ponechaju.
    /// </summary>
    private const int RetainedFiles = 31;

    /// <summary>
    /// Najvacsia velkost jedneho suboru; vacsi pokracuje v dalsom subore toho isteho dna.
    /// </summary>
    private const long FileSizeLimit = 10 * 1024 * 1024;

    /// <summary>
    /// Priecinok so zaznamami programu.
    /// </summary>
    public static string Directory => PathUtils.CombinePath(AppPaths.DataDir, "logs")!;

    /// <summary>
    /// Zacne zapisovat zaznamy do suboru <c>&lt;program&gt;-&lt;datum&gt;.log</c>. Viac instancii programu zapisuje
    /// do toho isteho suboru (cislo procesu je v kazdom riadku). Pri ladeni v debuggeri aj podrobne zaznamy.
    /// </summary>
    public static void Start()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(Debugger.IsAttached ? LogEventLevel.Debug : LogEventLevel.Information)
            .Enrich.WithProperty("ProcessId", Environment.ProcessId)
            .WriteTo.File(Path.Combine(Directory, AppPaths.AppName + "-.log"), outputTemplate: Template, formatProvider: CultureInfo.InvariantCulture,
                rollingInterval: RollingInterval.Day, retainedFileCountLimit: RetainedFiles, fileSizeLimitBytes: FileSizeLimit, rollOnFileSizeLimit: true,
                shared: true)
            .CreateLogger();
    }

    /// <summary>
    /// Zapise zvysne zaznamy a ukonci zapisovanie (pri ukonceni programu).
    /// </summary>
    public static void Stop() => Log.CloseAndFlush();
}
