namespace ToolsCore.Tools;

/// <summary>
///     Zabezpečí, že sa skupina súborov zmení buď celá, alebo vôbec.
/// </summary>
/// <remarks>
///     Ukladanie grafikonu aj banky zvukov zapisuje viac súborov po sebe. Keď niektorý zápis zlyhá,
///     časť súborov je nová a časť pôvodná - INISS ich potom pri štarte odmietne.
///     Táto trieda si pred zápisom odloží kópiu pôvodných súborov a pri chybe ich vráti späť.
/// </remarks>
public sealed class FileTransaction
{
    // povodna cesta -> kopia v zalohe
    private readonly Dictionary<string, string> _backups = new(StringComparer.OrdinalIgnoreCase);

    // subory, ktore pred zapisom neexistovali - pri navrate sa zmazu
    private readonly List<string> _newFiles = [];

    // rezim priecinka - pri navrate sa zmazu vsetky subory, ktore v nom pribudli
    private readonly string? _directory;

    /// <summary>
    ///     Odloží si kópiu súborov priamo v priečinku.
    /// </summary>
    /// <remarks>
    ///     Podpriečinky (napr. písma tabúľ) sa nezálohujú - do tých ukladanie nezasahuje.
    ///     Súbory, ktoré v priečinku pribudnú, sa pri návrate zmažú.
    /// </remarks>
    /// <param name="directory">Priečinok, ktorého obsah sa bude meniť.</param>
    /// <exception cref="IOException">ak sa zálohu nepodarí vytvoriť - vtedy sa nesmie začať zapisovať</exception>
    public FileTransaction(string directory)
        : this(Directory.GetFiles(directory)) =>
        _directory = directory;

    /// <summary>
    ///     Odloží si kópiu vymenovaných súborov.
    /// </summary>
    /// <remarks>
    ///     Súbory môžu byť v rôznych priečinkoch. Tie, ktoré ešte neexistujú, sa pri návrate zmažú.
    /// </remarks>
    /// <param name="files">Súbory, ktoré sa budú zapisovať.</param>
    /// <exception cref="IOException">ak sa zálohu nepodarí vytvoriť - vtedy sa nesmie začať zapisovať</exception>
    public FileTransaction(IEnumerable<string> files)
    {
        BackupPath = Path.Combine(Path.GetTempPath(), "INISSTools", "save-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(BackupPath);

        foreach (var file in files.Select(Path.GetFullPath).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!File.Exists(file))
            {
                _newFiles.Add(file);
                continue;
            }

            // rovnake nazvy suborov z roznych priecinkov (napr. FYZZVUK.DAT jazykov) sa v zalohe nesmu prepisat
            var backup = Path.Combine(BackupPath, _backups.Count + "_" + Path.GetFileName(file));
            File.Copy(file, backup, true);
            _backups.Add(file, backup);
        }
    }

    /// <summary>
    ///     Priečinok so zálohou pôvodných súborov.
    /// </summary>
    public string BackupPath { get; }

    /// <summary>
    ///     Potvrdí zmeny - záloha sa zahodí.
    /// </summary>
    public void Commit() => TryDeleteBackup();

    /// <summary>
    ///     Vráti súbory do stavu spred zápisu.
    /// </summary>
    /// <remarks>
    ///     Volá sa z bloku catch, preto nikdy nevyhadzuje výnimku - keby to spravila,
    ///     nahradila by pôvodnú chybu a tá by sa k používateľovi nedostala.
    /// </remarks>
    /// <returns><see langword="false" />, ak sa obnovenie nepodarilo; záloha vtedy zostáva zachovaná.</returns>
    public bool TryRollback()
    {
        try
        {
            foreach (var (target, backup) in _backups)
            {
                // nezmeneny subor netreba vracat - moze byt napr. len na citanie a prave preto zapis zlyhal
                if (File.Exists(target) && SameContent(backup, target))
                    continue;

                File.Copy(backup, target, true);
            }

            foreach (var file in _newFiles)
                File.Delete(file);

            if (_directory is not null)
                foreach (var file in Directory.GetFiles(_directory))
                    if (!_backups.ContainsKey(Path.GetFullPath(file)))
                        File.Delete(file);
        }
        catch (Exception e)
        {
            Log.Exception(e);
            return false;
        }

        TryDeleteBackup();
        return true;
    }

    private static bool SameContent(string a, string b) =>
        new FileInfo(a).Length == new FileInfo(b).Length && File.ReadAllBytes(a).AsSpan().SequenceEqual(File.ReadAllBytes(b));

    private void TryDeleteBackup()
    {
        try
        {
            if (Directory.Exists(BackupPath))
                Directory.Delete(BackupPath, true);
        }
        catch (Exception e)
        {
            //neodstránená záloha nič nerozbíja, len zaberá miesto v TEMPe
            Log.Exception(e);
        }
    }
}
