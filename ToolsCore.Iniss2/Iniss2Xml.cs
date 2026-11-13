using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using Serilog;
using ToolsCore.Iniss.Tools;
using ToolsCore.Iniss2.Xml;

namespace ToolsCore.Iniss2;

/// <summary>
/// Spolocne citanie suborov INISS2. Server vsetky okrem FyzBank.xml cita ako Windows-1250 bez ohladu na hlavicku XML.
/// </summary>
internal static class Iniss2Xml
{
    /// <summary>
    /// Nacita subor ako server.
    /// </summary>
    public static XDocument Load(string file) => XDocument.Parse(File.ReadAllText(file, Encodings.Win1250));

    /// <summary>
    /// Zapise subor ako server: CP1250, hlavicka windows-1250, odsadenie a konce riadkov CRLF. Novy obsah vznikne vedla a nahradi
    /// subor naraz, server tak nikdy nenacita rozpisany subor.
    /// </summary>
    /// <param name="file">subor</param>
    /// <param name="root">korenovy prvok</param>
    public static void Save(string file, XElement root)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(file))!);

        var temporary = file + ".tmp";
        var settings = new XmlWriterSettings { Encoding = Encodings.Win1250, Indent = true, IndentChars = "  ", NewLineChars = "\r\n" };
        using (var writer = XmlWriter.Create(temporary, settings))
            new XDocument(root).Save(writer);

        File.Move(temporary, file, true);
    }

    // novy obsah vznikne vedla a nahradi subor naraz
    private static void Replace(string file, string text)
    {
        var temporary = file + ".tmp";
        File.WriteAllText(temporary, text, Encodings.Win1250);
        File.Move(temporary, file, true);
    }

    /// <summary>
    /// Priznak <c>true</c>; <c>false</c> server nezapisuje.
    /// </summary>
    public static XAttribute? Optional(string name, bool value) => value ? new XAttribute(name, "true") : null;

    /// <summary>
    /// Logicka hodnota atributu ako u XmlSerializer servera; chybajuci atribut je <see langword="false" />.
    /// </summary>
    public static bool Bool(XElement element, string attribute) =>
        element.Attribute(attribute) is { } value && XmlConvert.ToBoolean(value.Value.Trim());

    /// <summary>
    /// Zapise subor ako server. Existujuci subor upravi na mieste: zmenia sa len prvky s inym obsahom, komentare, atributy
    /// a prvky, ktore zapisovac nepozna, a zapis nezmenenych casti ostanu po znakoch. Subor bez zmeny sa nezapisuje.
    /// </summary>
    /// <param name="file">subor</param>
    /// <param name="desired">novy obsah zo zapisovaca</param>
    /// <param name="canonical">zapisovac nad precitanym obsahom: z korena suboru zostavi to, co by zapisal on</param>
    /// <returns><see langword="false" />, ak subor uz mal tento obsah.</returns>
    public static bool Write(string file, XElement desired, Func<XElement, XElement> canonical)
    {
        ArgumentNullException.ThrowIfNull(desired);
        ArgumentNullException.ThrowIfNull(canonical);

        if (!File.Exists(file))
        {
            Save(file, desired);
            return true;
        }

        var text = File.ReadAllText(file, Encodings.Win1250);
        var older = canonical(XDocument.Parse(text).Root!);
        if (XNode.DeepEquals(older, desired))
            return false;

        string updated;
        try
        {
            updated = XmlTextPatch.Apply(text, older, desired);
            if (!XNode.DeepEquals(canonical(XDocument.Parse(updated).Root!), desired))
                throw new InvalidOperationException("Patched file does not read back as written.");
        }
        catch (InvalidOperationException e)
        {
            Log.Warning(e, "Subor {File} sa nepodarilo upravit na mieste, zapise sa cely bez komentarov", file);
            Save(file, desired);
            return true;
        }

        Replace(file, updated);
        return true;
    }

    /// <summary>
    /// Textovy atribut, ktory v subore nie je, ked je prazdny.
    /// </summary>
    public static XAttribute? Optional(string name, string value) => value.Length > 0 ? new XAttribute(name, value) : null;

    /// <summary>
    /// Ciselny atribut, ktory v subore nie je, ked ma predvolenu hodnotu.
    /// </summary>
    public static XAttribute? Optional(string name, int value, int missing = 0) => value != missing ? new XAttribute(name, value) : null;

    /// <summary>
    /// Hodnota atributu; chybajuci atribut je prazdny text.
    /// </summary>
    public static string Text(XElement element, string attribute) => (string?)element.Attribute(attribute) ?? "";

    /// <summary>
    /// Ciselna hodnota atributu.
    /// </summary>
    /// <param name="element">prvok</param>
    /// <param name="attribute">nazov atributu</param>
    /// <param name="missing">hodnota, ak atribut chyba</param>
    public static int Number(XElement element, string attribute, int missing = 0) =>
        element.Attribute(attribute) is { } value ? int.Parse(value.Value, CultureInfo.InvariantCulture) : missing;

    /// <summary>
    /// Text priznakov: kluce oddelene medzerou, hodnotu s vlastnym klucom jej klucom. Z viacerych klucov jedneho priznaku plati prvy.
    /// </summary>
    /// <param name="flags">priznaky</param>
    /// <param name="keys">kluce priznakov a ich hodnoty</param>
    public static string FlagText(int flags, IReadOnlyDictionary<string, int> keys)
    {
        foreach (var (key, value) in keys)
        {
            if (value == flags)
                return key;
        }

        var names = new List<string>();
        var rest = flags;

        foreach (var (key, value) in keys)
        {
            if (value != 0 && (rest & value) == value)
            {
                names.Add(key);
                rest &= ~value;
            }
        }

        return string.Join(' ', names);
    }

    /// <summary>
    /// Priznaky zapisane klucmi oddelenymi medzerou. Server subor s neznamym klucom nenacita.
    /// </summary>
    /// <param name="element">prvok</param>
    /// <param name="attribute">nazov atributu</param>
    /// <param name="keys">kluce priznakov a ich hodnoty</param>
    public static int Flags(XElement element, string attribute, IReadOnlyDictionary<string, int> keys)
    {
        var flags = 0;
        foreach (var key in Text(element, attribute).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
        {
            if (!keys.TryGetValue(key, out var value))
                throw new FormatException(string.Create(CultureInfo.InvariantCulture, $"Unknown flag {key} in attribute {attribute} of element {element.Name.LocalName}."));

            flags |= value;
        }

        return flags;
    }
}
