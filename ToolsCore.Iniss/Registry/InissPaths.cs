namespace ToolsCore.Iniss.Registry;

/// <summary>Priecinky instalacie INISSu podla nastavenia, ktore INISS nacita.</summary>
public static class InissPaths
{
    /// <summary>
    /// Priecinok logov INISSu - <c>PathNames\LogPath</c> (relativne k priecinku programu), bez nastavenia DATA.
    /// </summary>
    /// <param name="resolved">nastavenie INISSu</param>
    /// <param name="installationDir">priecinok programu INISSu</param>
    public static string LogDirectory(ResolvedConfig resolved, string installationDir)
    {
        ArgumentNullException.ThrowIfNull(resolved);
        var value = resolved.Find("PathNames", "LogPath")?.Value as string;
        return Path.GetFullPath(Path.Combine(installationDir, string.IsNullOrWhiteSpace(value) ? "DATA" : value.Trim()));
    }
}
