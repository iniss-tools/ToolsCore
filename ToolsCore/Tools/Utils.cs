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
    /// Skombinuje cestu k suborom/priecinkom.
    /// </summary>
    /// <param name="paths">pole retazcov s cestami k suborom/priecinkom.</param>
    /// <returns>skombinovanú cestu.</returns>
    public static string? CombinePath(params string[] paths)
    {
        if (paths.Length == 0) return null;

        if (paths[0].Length == 0) return paths[0];

        for (var i = 1; i < paths.Length; i++)
        {
            paths[i] = paths[i].TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        return Path.Combine(paths);
    }

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
        Application.Restart();
        Log.Info("Program sa reštartuje\r\n");
        Environment.Exit(0);
    }
}
