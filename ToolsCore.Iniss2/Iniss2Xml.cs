using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using ToolsCore.Iniss.Tools;

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
