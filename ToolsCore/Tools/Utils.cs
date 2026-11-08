using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable UnusedMethodReturnValue.Global

namespace ToolsCore.Tools;

/// <summary>
/// Spolocne pomocky nastrojov. Rozdelene podla oblasti do suborov Utils.*.cs (Encoding, Dialogs, Parsing,
/// Graphics, Strings, Files, Controls).
/// </summary>
public static partial class Utils
{
    /// <summary>
    /// Vrati cestu k projektu zadanu ako argument prikazoveho riadka (napr. pri spusteni zo zoznamu odkazov
    /// na paneli uloh). Prepinace zacinajuce znakom / alebo - sa preskakuju.
    /// </summary>
    /// <returns>cesta k projektu alebo <see langword="null"/>, ak nebola zadana.</returns>
    [ExcludeFromCodeCoverage]
    public static string? GetProjectPathFromArgs()
    {
        string? path = null;
        foreach (var arg in Environment.GetCommandLineArgs().Skip(1))
            if (!arg.StartsWith('/') && !arg.StartsWith('-'))
                path = arg;

        return path;
    }

    /// <summary>
    /// Otvorí URL, mailto odkaz, súbor alebo priečinok cez asociovaný shell handler (predvolený prehliadač,
    /// poštový klient, Prieskumník...).
    /// </summary>
    /// <param name="target">URL, mailto: odkaz, cesta k súboru alebo priečinku.</param>
    public static void OpenShell(string target) => Process.Start(new ProcessStartInfo(target) { UseShellExecute = true });

    /// <summary>
    /// Reštartuje program.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static void RestartApp()
    {
        Serilog.Log.Information("Program sa restartuje");
        AppLog.Stop();
        Application.Restart();
        Environment.Exit(0);
    }
}
