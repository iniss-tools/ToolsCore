using System.Xml;
using System.Xml.Linq;
using Serilog;
using ToolsCore.Iniss.Entities;
using ToolsCore.Iniss.Tools;

namespace ToolsCore.Iniss2.RawBank;

/// <summary>
/// Citanie zvukovej banky INISS2 - suborov FyzBank.xml v priecinkoch jazykov (RawBank\&lt;jazyk&gt;\).
/// </summary>
/// <remarks>
/// Spolocny zoznam jazykov INISS2 nema: jazyk je kazdy podpriecinok banky so suborom, ktory ma prave jeden prvok
/// <c>lang</c>. Nazvy skupin a zvukov subor neobsahuje, namiesto nich sa pouzije kluc.
/// </remarks>
public static class FyzBankXmlReader
{
    /// <summary>
    /// Precita zoznam jazykov banky bez skupin a zvukov. Priecinky su zoradene podla nazvu.
    /// </summary>
    /// <param name="pathToBank">priecinok banky</param>
    public static List<FyzLanguage> ReadLanguages(string pathToBank)
    {
        ArgumentNullException.ThrowIfNull(pathToBank);

        var languages = new List<FyzLanguage>();
        foreach (var dir in Directory.EnumerateDirectories(pathToBank).Order(StringComparer.OrdinalIgnoreCase))
        {
            var file = Path.Combine(dir, FyzBankXmlWriter.FileName);
            if (File.Exists(file) && ReadLanguage(file, Path.GetFileName(dir)) is { } language)
                languages.Add(language);
        }

        return languages;
    }

    /// <summary>
    /// Precita skupiny a zvuky jazyka do <see cref="FyzLanguage.Groups" />.
    /// </summary>
    /// <param name="pathToBank">priecinok banky</param>
    /// <param name="language">jazyk zo zoznamu <see cref="ReadLanguages" /></param>
    public static void ReadSounds(string pathToBank, FyzLanguage language)
    {
        ArgumentNullException.ThrowIfNull(language);

        using var stream = File.OpenRead(RawBankParser.FyzZvukFile(pathToBank, language));
        var element = XDocument.Load(stream).Root?.Element("lang");

        language.Groups = [];
        foreach (var groupElement in element?.Elements("g") ?? [])
        {
            var key = Iniss2Xml.Text(groupElement, "k");
            var group = new FyzGroup(language, key, key, Iniss2Xml.Text(groupElement, "d"));
            foreach (var soundElement in groupElement.Elements("z"))
                group.Sounds.Add(ReadSound(group, soundElement));
            language.Groups.Add(group);
        }
    }

    private static FyzSound ReadSound(FyzGroup group, XElement element)
    {
        var key = Iniss2Xml.Text(element, "k");
        var path = Iniss2Xml.Text(element, "f");
        // pridavna cesta je sucastou atributu f, rovnako ako vo FYZZVUK.DAT
        var index = path.LastIndexOf('\\');

        return new FyzSound(group, key, key, path[(index + 1)..], path[..(index + 1)], Iniss2Xml.Text(element, "t"), Iniss2Xml.Number(element, "l"))
        {
            Fid = Iniss2Xml.Number(element, "fid"),
            Md5 = (string?)element.Attribute("md5")
        };
    }

    private static FyzLanguage? ReadLanguage(string file, string folder)
    {
        using var stream = File.OpenRead(file);
        using var reader = XmlReader.Create(stream);

        reader.MoveToContent();
        if (reader.LocalName != "FYZBANK")
        {
            Log.Warning("Subor {File} nie je banka INISS2 - preskakuje sa", file);
            return null;
        }

        FyzLanguage? language = null;
        var count = 0;
        while (reader.Read())
        {
            if (reader is not { NodeType: XmlNodeType.Element, Depth: 1, LocalName: "lang" })
                continue;

            count++;
            language ??= ReadLanguageAttributes(reader, folder, file);
        }

        // server subor s inym poctom jazykov ignoruje
        if (count == 1)
            return language;

        Log.Warning("Subor {File} ma {Count} jazykov namiesto jedneho - preskakuje sa", file, count);
        return null;
    }

    private static FyzLanguage ReadLanguageAttributes(XmlReader reader, string folder, string file)
    {
        // cesta jazyka je priecinok, v ktorom subor lezi - podla neho sa jazyk paruje so subormi na disku
        var directory = reader.GetAttribute("d");
        if (directory is not null && !string.Equals(directory.TrimEnd('\\'), folder, StringComparison.OrdinalIgnoreCase))
            Log.Warning("Subor {File} uvadza priecinok jazyka {Directory}, lezi v priecinku {Folder}", file, directory, folder);

        return new FyzLanguage(reader.GetAttribute("k") ?? folder, reader.GetAttribute("n") ?? folder, FyzBankXmlWriter.FileName, folder + "\\")
        {
            IsBasic = reader.GetAttribute("default") is { } isDefault && XmlConvert.ToBoolean(isDefault),
            Date = reader.GetAttribute("date") is { } date ? XmlConvert.ToDateTimeOffset(date) : null,
            Location = reader.GetAttribute("location")
        };
    }
}
