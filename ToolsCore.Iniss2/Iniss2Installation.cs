using System.Globalization;
using ToolsCore.Iniss.Tools;
using ToolsCore.Iniss2.RawBank;

namespace ToolsCore.Iniss2;

/// <summary>
/// Instalacia INISS2 - priecinok servera IniSrv s datami a zvukovou bankou.
/// </summary>
/// <param name="ServerDir">priecinok servera, v ktorom su priecinky <c>Data</c> a <c>RawBank</c></param>
public sealed record Iniss2Installation(string ServerDir)
{
    /// <summary>
    /// Priecinok servera v koreni instalacie.
    /// </summary>
    public const string ServerDirName = "Server";

    /// <summary>
    /// Priecinok zvukovej banky v priecinku servera.
    /// </summary>
    public const string RawBankDirName = "RawBank";

    /// <summary>
    /// Priecinok dat v priecinku servera.
    /// </summary>
    public const string DataDirName = "Data";

    /// <summary>
    /// Priecinok logov v priecinku servera.
    /// </summary>
    public const string LogDirName = "LOG";

    /// <summary>
    /// Konfiguracny subor servera v priecinku dat.
    /// </summary>
    public const string ConfigFileName = "Config.xml";

    /// <summary>
    /// Priecinok zvukovej banky s koncovou lomkou, ako ho ocakavaju cesty jazykov a zvukov.
    /// </summary>
    public string RawBankDir => Path.Combine(ServerDir, RawBankDirName) + Path.DirectorySeparatorChar;

    /// <summary>
    /// Priecinok dat servera.
    /// </summary>
    public string DataDir => Path.Combine(ServerDir, DataDirName);

    /// <summary>
    /// Priecinok logov servera.
    /// </summary>
    public string LogDir => Path.Combine(ServerDir, LogDirName);

    /// <summary>
    /// Log sprav GTN zo zadaneho dna. Server zacina kazdy den novy subor.
    /// </summary>
    /// <param name="day">den</param>
    public string GtnLogFile(DateOnly day) => Path.Combine(LogDir, "GTN" + day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ".log");

    /// <summary>
    /// Najde instalaciu INISS2 v zadanom priecinku - v koreni instalacie alebo priamo v priecinku servera.
    /// </summary>
    /// <remarks>
    /// Windows nerozlisuje velkost pismen, RAWBANK v INISS 3.39 je teda ten isty nazov ako RawBank v INISS2. Rozhoduje
    /// obsah: FyzBank.dat znamena banku 3.39, aj ked su v nej exportovane subory FyzBank.xml.
    /// </remarks>
    /// <param name="path">priecinok vybrany pouzivatelom</param>
    /// <returns>instalacia alebo <see langword="null" />, ak v priecinku nie je INISS2.</returns>
    public static Iniss2Installation? Find(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        foreach (var dir in new[] { path, Path.Combine(path, ServerDirName) })
        {
            if (File.Exists(Path.Combine(dir, RawBankDirName, RawBankParser.FileFyzbank)))
                return null;

            if (IsServerDir(dir))
                return new Iniss2Installation(dir);
        }

        return null;
    }

    /// <summary>
    /// Priecinok dat INISS2 podla vybraneho priecinka: instalacie, servera alebo priamo dat (s DirList.xml).
    /// </summary>
    /// <param name="path">priecinok vybrany pouzivatelom</param>
    /// <returns>priecinok dat alebo <see langword="null" />, ak to nie je INISS2.</returns>
    public static string? DataDirectory(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            return null;

        path = Path.GetFullPath(path);
        if (File.Exists(Path.Combine(path, Grafikon.DirListXml.FileName)))
            return path;

        return Find(path)?.DataDir;
    }

    private static bool IsServerDir(string dir) =>
        File.Exists(Path.Combine(dir, DataDirName, ConfigFileName)) || HasLanguage(Path.Combine(dir, RawBankDirName));

    private static bool HasLanguage(string bankDir) =>
        Directory.Exists(bankDir) && Directory.EnumerateDirectories(bankDir).Any(dir => File.Exists(Path.Combine(dir, FyzBankXmlWriter.FileName)));
}
