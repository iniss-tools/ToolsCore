using System.Globalization;
using System.IO.Compression;
using System.Xml;
using System.Xml.Linq;
using Serilog;
using ToolsCore.Properties;

namespace ToolsCore.Tools;

/// <summary>
/// Prenos nastaveni programu (<c>CONFIG.cfg</c> a <c>STYLES.cfg</c>) do suboru ZIP a spat a obnova predvolenych
/// nastaveni. Import a obnova sa vykonaju az pri dalsom starte programu (<see cref="ApplyPending" /> pred nacitanim
/// konfiguracie) - beziaci program by nastavenia inak prepisal svojimi; povodne subory sa zalohuju.
/// </summary>
public static class SettingsTransfer
{
    private const string ManifestName = "settings.xml";
    private const string PendingImport = "import.pending.zip";
    private const string PendingReset = "reset.pending";
    private const string BackupDir = "backup";
    private const int KeptBackups = 5;

    private static readonly string[] Files = [FileConsts.FileConfig, FileConsts.FileStyles];

    /// <summary>
    /// Ulozi ulozene nastavenia programu do suboru ZIP.
    /// </summary>
    /// <param name="zipPath">cielovy subor (existujuci sa prepise)</param>
    /// <param name="configDir">priecinok s nastaveniami</param>
    /// <param name="program">nazov programu (kontroluje sa pri importe)</param>
    public static void Export(string zipPath, string configDir, string program)
    {
        if (File.Exists(zipPath))
            File.Delete(zipPath);

        using var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create);
        foreach (var name in Files)
        {
            var path = Path.Combine(configDir, name);
            if (File.Exists(path))
                zip.CreateEntryFromFile(path, name);
        }

        var manifest = new XElement("INISSToolsSettings",
            new XAttribute("program", program),
            new XAttribute("created", DateTime.Now.ToString("s", CultureInfo.InvariantCulture)));
        using var stream = zip.CreateEntry(ManifestName).Open();
        manifest.Save(stream);
    }

    /// <summary>
    /// Skontroluje subor s nastaveniami pred importom.
    /// </summary>
    /// <returns>popis chyby; <see langword="null" /> = subor sa da importovat</returns>
    public static string? Check(string zipPath, string program)
    {
        var notSettings = string.Format(CultureInfo.CurrentCulture, Resources.SettingsTransfer_NotSettings, Path.GetFileName(zipPath));
        try
        {
            using var zip = ZipFile.OpenRead(zipPath);
            if (zip.GetEntry(ManifestName) is not { } manifestEntry)
                return notSettings;

            using (var stream = manifestEntry.Open())
            {
                var other = (string?)XDocument.Load(stream).Root?.Attribute("program");
                if (!string.Equals(other, program, StringComparison.OrdinalIgnoreCase))
                    return string.Format(CultureInfo.CurrentCulture, Resources.SettingsTransfer_OtherProgram, other);
            }

            var files = zip.Entries.Where(e => Files.Contains(e.FullName, StringComparer.Ordinal)).ToList();
            if (files.Count == 0)
                return notSettings;

            // obsah musi byt XML - poskodeny subor by pri starte nahradili predvolene nastavenia
            foreach (var entry in files)
            {
                using var stream = entry.Open();
                XDocument.Load(stream);
            }

            return null;
        }
        catch (Exception e) when (e is InvalidDataException or IOException or XmlException or UnauthorizedAccessException)
        {
            Log.Warning(e, "Subor {File} nie je platna zaloha nastaveni", zipPath);
            return notSettings;
        }
    }

    /// <summary>
    /// Pripravi import nastaveni zo suboru pri dalsom starte programu.
    /// </summary>
    public static void ScheduleImport(string zipPath, string configDir)
    {
        File.Delete(Path.Combine(configDir, PendingReset));
        File.Copy(zipPath, Path.Combine(configDir, PendingImport), true);
        Log.Information("Import nastaveni zo suboru {File} pripraveny na dalsie spustenie", zipPath);
    }

    /// <summary>
    /// Pripravi obnovu predvolenych nastaveni pri dalsom starte programu.
    /// </summary>
    public static void ScheduleReset(string configDir)
    {
        File.Delete(Path.Combine(configDir, PendingImport));
        File.WriteAllText(Path.Combine(configDir, PendingReset), "");
        Log.Information("Obnova predvolenych nastaveni pripravena na dalsie spustenie");
    }

    /// <summary>
    /// Vykona pripraveny import alebo obnovu: povodne nastavenia zalohuje do <c>backup\&lt;cas&gt;</c> (poslednych 5)
    /// a nahradi ich nastaveniami zo suboru, pri obnove ich zmaze (program zacne s predvolenymi).
    /// </summary>
    /// <returns><see langword="true" />, ak sa nieco vykonalo.</returns>
    public static bool ApplyPending(string configDir)
    {
        var import = Path.Combine(configDir, PendingImport);
        var reset = Path.Combine(configDir, PendingReset);
        if (!File.Exists(import) && !File.Exists(reset))
            return false;

        Backup(configDir);
        foreach (var name in Files)
            File.Delete(Path.Combine(configDir, name));

        if (File.Exists(import))
        {
            using (var zip = ZipFile.OpenRead(import))
            {
                // len zname nazvy suborov - ziadne cesty zo suboru ZIP
                foreach (var entry in zip.Entries.Where(e => Files.Contains(e.FullName, StringComparer.Ordinal)))
                    entry.ExtractToFile(Path.Combine(configDir, entry.FullName), true);
            }

            File.Delete(import);
            Log.Information("Nastavenia programu nahradene importovanymi");
        }
        else
        {
            Log.Information("Nastavenia programu obnovene na predvolene");
        }

        File.Delete(reset);
        return true;
    }

    private static void Backup(string configDir)
    {
        var root = Path.Combine(configDir, BackupDir);
        var target = Path.Combine(root, DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture));
        Directory.CreateDirectory(target);
        foreach (var name in Files)
        {
            var path = Path.Combine(configDir, name);
            if (File.Exists(path))
                File.Copy(path, Path.Combine(target, name), true);
        }

        foreach (var old in Directory.GetDirectories(root).OrderByDescending(d => d, StringComparer.Ordinal).Skip(KeptBackups))
            Directory.Delete(old, true);
    }
}
