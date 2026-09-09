using ToolsCore.Entities;

namespace ToolsCore.Tools;

public static class RawBankParser
{
    /// <summary>
    ///     Precita subor FyzBank.dat, ktory obsahuje informacie o jazykoch zvukovej banky.
    /// </summary>
    /// <returns>list jazykov zvukovej banky.</returns>
    public static List<FyzLanguage> ReadFyzBankFile(string pathToBank, out int maxCountLangs)
    {
        var file = Utils.CombinePath(pathToBank, FileConsts.FILE_FYZBANK);

        if (!File.Exists(file))
            throw new FileNotFoundException($"Súbor s definíciou priečinkov zvukov sa na zvolenej ceste nenašiel: {file}");

        using var reader = new BinaryReader(File.Open(file, FileMode.Open), Encodings.Win1250);
        var countLangs = reader.ReadInt32();
        maxCountLangs = countLangs;
        var languages = new List<FyzLanguage>(countLangs);

        for (var i = 0; i < countLangs; i++)
        {
            var fileFyzZvukName = reader.ReadBytes(reader.ReadNumWithVarLength()).ANSItoUTF();
            var relativePath = reader.ReadBytes(reader.ReadNumWithVarLength()).ANSItoUTF();
            var key = reader.ReadBytes(reader.ReadNumWithVarLength()).ANSItoUTF();
            var name = reader.ReadBytes(reader.ReadNumWithVarLength()).ANSItoUTF();
            reader.ReadBytes(4); // 4 byte padding
            languages.Add(new FyzLanguage(key, name, fileFyzZvukName, relativePath));
        }

        return languages;
    }

    /// <summary>
    ///     Zapise subor FyzBank.dat, ktory obsahuje informacie o jazykoch zvukovej banky.
    /// </summary>
    public static void WriteFyzBankFile(string pathToBank, List<FyzLanguage> languages)
    {
        var file = Utils.CombinePath(pathToBank, FileConsts.FILE_FYZBANK)!;

        using var writer = new BinaryWriter(File.Open(file, FileMode.Create), Encodings.Win1250);
        writer.Write(languages.Count);

        foreach (var language in languages)
        {
            writer.WriteNumWithVarLength(language.FileDefName.Length);
            writer.Write(language.FileDefName.UTFtoANSI().ToCharArray());

            writer.WriteNumWithVarLength(language.RelativePath.Length);
            writer.Write(language.RelativePath.UTFtoANSI().ToCharArray());

            writer.WriteNumWithVarLength(language.Key.Length);
            writer.Write(language.Key.UTFtoANSI().ToCharArray());

            writer.WriteNumWithVarLength(language.Name.Length);
            writer.Write(language.Name.UTFtoANSI().ToCharArray());

            writer.Write(0xffffffff); // 4 byte padding
        }
    }

    /// <summary>
    ///     Precita subor FyzZvuk.dat, ktory obsahuje informacie o skupinach zvukov a zvukoch samotnych.
    /// </summary>
    /// <param name="pathToBank"></param>
    /// <param name="language">informacia o jazyku zvukovej banky, do ktorej sa budu vkladat informacie o skupinach a zvukoch.</param>
    /// <param name="worker">background worker pre asynchronnost</param>
    public static List<FyzSound> ReadFyzZvukFile(string pathToBank, FyzLanguage language, BackgroundWorker? worker = null)
    {
        ArgumentNullException.ThrowIfNull(language);

        var file = Utils.CombinePath(pathToBank, language.RelativePath, language.FileDefName)!;

        if (!File.Exists(file))
            throw new FileNotFoundException($"Súbor s definíciou zvukov sa na zvolenej ceste nenašiel: {file}");

        var allSounds = new LinkedList<FyzSound>();

        language.Groups = new List<FyzGroup>();

        using var reader = new BinaryReader(File.Open(file, FileMode.Open), Encodings.Win1250);
        var countGroups = reader.ReadInt32();

        var progress = new ProgressStatus("Analyzovanie súboru banky zvukov", countGroups);
        worker?.ReportProgress(0, progress);

        for (var i = 0; i < countGroups; i++)
        {
            var dirName = reader.ReadBytes(reader.ReadNumWithVarLength()).ANSItoUTF();
            var dirKey = reader.ReadBytes(reader.ReadNumWithVarLength()).ANSItoUTF();
            var dirRelativePath = reader.ReadBytes(reader.ReadNumWithVarLength()).ANSItoUTF();
            var countFiles = reader.ReadInt32();

            var grp = new FyzGroup(language, dirKey, dirName, dirRelativePath);

            for (var j = 0; j < countFiles; j++)
            {
                var text = reader.ReadBytes(reader.ReadNumWithVarLength()).ANSItoUTF();
                var sName = reader.ReadBytes(reader.ReadNumWithVarLength()).ANSItoUTF();
                var sKey = reader.ReadBytes(reader.ReadNumWithVarLength()).ANSItoUTF();
                var fileName = reader.ReadBytes(reader.ReadNumWithVarLength()).ANSItoUTF();
                
                var relativePath = "";
                if (fileName.Contains('\\'))
                {
                    var index = fileName.LastIndexOf('\\');
                    relativePath = fileName[..(index + 1)];
                    fileName = fileName.Replace(relativePath, "");
                }

                var duration = reader.ReadInt32();

                var snd = new FyzSound(grp, sKey, sName, fileName, relativePath, text, duration);
                allSounds.AddLast(snd);
                grp.Sounds.Add(snd);
            }

            language.Groups.Add(grp);
            worker?.ReportProgress(i, progress);
        }

        return allSounds.ToList();
    }

    /// <summary>
    ///     Zapise subor FyzZvuk.dat, ktory obsahuje informacie o skupinach zvukov a zvukoch samotnych.
    /// </summary>
    /// <param name="pathToBank"></param>
    /// <param name="language">informacia o jazyku zvukovej banky, z ktorej sa budu vytvarat informacie o skupinach a zvukoch.</param>
    public static void WriteFyzZvukFile(string pathToBank, FyzLanguage language)
    {
        ArgumentNullException.ThrowIfNull(language);

        var file = Utils.CombinePath(pathToBank, language.RelativePath, language.FileDefName)!;

        using var writer = new BinaryWriter(File.Open(file, FileMode.Create), Encodings.Win1250);
        writer.Write(language.Groups.Count);

        foreach (var grp in language.Groups)
        {
            writer.WriteNumWithVarLength(grp.Name.Length);
            writer.Write(grp.Name.UTFtoANSI().ToCharArray());

            writer.WriteNumWithVarLength(grp.Key.Length);
            writer.Write(grp.Key.UTFtoANSI().ToCharArray());

            writer.WriteNumWithVarLength(grp.RelativePath.Length);
            writer.Write(grp.RelativePath.UTFtoANSI().ToCharArray());

            writer.Write(grp.Sounds.Count);

            foreach (var sound in grp.Sounds)
            {
                writer.WriteNumWithVarLength(sound.Text.Length);
                writer.Write(sound.Text.UTFtoANSI().ToCharArray());

                writer.WriteNumWithVarLength(sound.Name.Length);
                writer.Write(sound.Name.UTFtoANSI().ToCharArray());

                writer.WriteNumWithVarLength(sound.Key.Length);
                writer.Write(sound.Key.UTFtoANSI().ToCharArray());

                var path = string.IsNullOrEmpty(sound.AdditionalRelativePath) ? sound.FileName : sound.AdditionalRelativePath + sound.FileName;
                writer.WriteNumWithVarLength(path.Length);
                writer.Write(path.UTFtoANSI().ToCharArray());

                writer.Write(sound.Duration);
            }
        }
    }
    
    private static int ReadNumWithVarLength(this BinaryReader reader)
    {
        var b = reader.ReadByte();
        return b == 0xff ? reader.ReadInt16() : b;
    }

    private static void WriteNumWithVarLength(this BinaryWriter writer, int value)
    {
        if (value > byte.MaxValue)
        {
            writer.Write((byte)0xff);
            writer.Write(checked((short) value));
        }
        else
            writer.Write((byte) value);
    }

    public static bool AdditionalPathIsEmpty(string path) => string.IsNullOrEmpty(path) || path is "." or ".\\";
}