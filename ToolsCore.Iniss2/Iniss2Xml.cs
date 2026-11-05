using System.Globalization;
using System.Xml.Linq;
using Serilog;
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
    /// Priznaky zapisane klucmi oddelenymi medzerou. Neznamy kluc sa preskoci.
    /// </summary>
    /// <param name="element">prvok</param>
    /// <param name="attribute">nazov atributu</param>
    /// <param name="keys">kluce priznakov a ich hodnoty</param>
    public static int Flags(XElement element, string attribute, IReadOnlyDictionary<string, int> keys)
    {
        var flags = 0;
        foreach (var key in Text(element, attribute).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
        {
            if (keys.TryGetValue(key, out var value))
                flags |= value;
            else
                Log.Warning("Neznamy priznak {Key} v atribute {Attribute} prvku {Element}", key, attribute, element.Name.LocalName);
        }

        return flags;
    }
}
