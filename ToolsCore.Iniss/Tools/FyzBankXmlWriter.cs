using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Cryptography;
using System.Xml;
using ToolsCore.Iniss.Entities;

namespace ToolsCore.Iniss.Tools;

/// <summary>
/// Zapis jazyka banky zvukov vo formate INISS2 - subor FyzBank.xml v priecinku jazyka (RawBank\&lt;jazyk&gt;\).
/// </summary>
/// <remarks>
/// INISS2 nacita z kazdeho podpriecinka banky jeden FyzBank.xml s jednym jazykom. Zvuk hlada podla klucov
/// jazyk/skupina/zvuk a subor na ceste banka + priecinok jazyka + priecinok skupiny + subor. Zvuk bez atributu
/// <c>md5</c> povazuje za chybajuci a namiesto neho prehra ticho - <c>md5</c> je MD5 obsahu suboru (male pismena).
/// </remarks>
public static class FyzBankXmlWriter
{
    /// <summary>
    /// Nazov suboru banky INISS2 v priecinku jazyka.
    /// </summary>
    public const string FileName = "FyzBank.xml";

    /// <summary>
    /// Zapise jazyk do suboru FyzBank.xml. Subor sa zapise najprv vedla s priponou .tmp a az cely sa presunie na miesto.
    /// </summary>
    /// <param name="file">cielovy subor</param>
    /// <param name="language">jazyk s nacitanymi skupinami</param>
    /// <param name="isDefault">predvoleny jazyk INISS2 - z jazykov banky prave jeden, inak INISS2 banku nenacita</param>
    /// <param name="date">datum vytvorenia banky (atribut <c>date</c>)</param>
    /// <param name="md5">MD5 suboru zvuku; <see langword="null" />, ak subor neexistuje</param>
    public static void Write(string file, FyzLanguage language, bool isDefault, DateTimeOffset date, Func<FyzSound, string?> md5)
    {
        ArgumentException.ThrowIfNullOrEmpty(file);

        var tempFile = file + ".tmp";
        try
        {
            using (var stream = File.Create(tempFile))
                Write(stream, language, isDefault, date, md5);

            File.Move(tempFile, file, true);
        }
        catch
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
            throw;
        }
    }

    /// <summary>
    /// Zapise jazyk vo formate FyzBank.xml (UTF-8 s BOM, ako ho zapisuje INISS2).
    /// </summary>
    /// <param name="stream">vystup</param>
    /// <param name="language">jazyk s nacitanymi skupinami</param>
    /// <param name="isDefault">predvoleny jazyk INISS2 - z jazykov banky prave jeden, inak INISS2 banku nenacita</param>
    /// <param name="date">datum vytvorenia banky (atribut <c>date</c>)</param>
    /// <param name="md5">MD5 suboru zvuku; <see langword="null" />, ak subor neexistuje</param>
    public static void Write(Stream stream, FyzLanguage language, bool isDefault, DateTimeOffset date, Func<FyzSound, string?> md5)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(language);
        ArgumentNullException.ThrowIfNull(language.Groups);
        ArgumentNullException.ThrowIfNull(md5);

        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            Encoding = new UTF8Encoding(true)
        };

        using var writer = XmlWriter.Create(stream, settings);
        writer.WriteStartDocument();
        writer.WriteStartElement("FYZBANK");

        writer.WriteStartElement("lang");
        // INISS2 bez atributu povazuje jazyk za nepredvoleny
        if (isDefault)
            writer.WriteAttributeString("default", "true");
        writer.WriteAttributeString("k", language.Key);
        writer.WriteAttributeString("n", language.Name);
        writer.WriteAttributeString("d", language.RelativePath.TrimEnd('\\'));
        writer.WriteAttributeString("date", XmlConvert.ToString(date));

        foreach (var group in language.Groups)
        {
            writer.WriteStartElement("g");
            writer.WriteAttributeString("k", group.Key);
            writer.WriteAttributeString("d", group.RelativePath);

            foreach (var sound in group.Sounds)
            {
                writer.WriteStartElement("z");
                writer.WriteAttributeString("k", sound.Key);
                writer.WriteAttributeString("t", sound.Text);
                writer.WriteAttributeString("f", FilePath(sound));
                writer.WriteAttributeString("l", sound.Duration.ToString(CultureInfo.InvariantCulture));
                if (md5(sound) is { Length: > 0 } hash)
                    writer.WriteAttributeString("md5", hash);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndDocument();
    }

    /// <summary>
    /// Subor zvuku relativne k priecinku skupiny - s pridavnou cestou rovnako ako vo FYZZVUK.DAT.
    /// </summary>
    public static string FilePath(FyzSound sound)
    {
        ArgumentNullException.ThrowIfNull(sound);
        return RawBankParser.AdditionalPathIsEmpty(sound.AdditionalRelativePath) ? sound.FileName : sound.AdditionalRelativePath + sound.FileName;
    }

    /// <summary>
    /// MD5 obsahu suboru ako 32 hexadecimalnych znakov malymi pismenami (ako v suboroch INISS2).
    /// </summary>
    [SuppressMessage("Security", "CA5351:Do Not Use Broken Cryptographic Primitives",
        Justification = "MD5 vyzaduje format INISS2 - kontrolny sucet suboru, nie zabezpecenie.")]
    public static string FileMd5(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexStringLower(MD5.HashData(stream));
    }
}
