using System.Globalization;
using System.Xml.Linq;

namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Datumy obdobia vyluk a odklonov v tvare servera.
/// </summary>
internal static class NoticeDates
{
    private const string Format = "yyyy-MM-dd";

    /// <summary>
    /// Datum z atributu; chybajuci je prvy den kalendara ako u servera.
    /// </summary>
    public static DateOnly Read(XElement element, string attribute) =>
        (string?)element.Attribute(attribute) is { Length: > 0 } text
            ? DateOnly.ParseExact(text, Format, CultureInfo.InvariantCulture)
            : DateOnly.MinValue;

    /// <summary>
    /// Atribut s datumom.
    /// </summary>
    public static XAttribute Attribute(string name, DateOnly date) => new(name, date.ToString(Format, CultureInfo.InvariantCulture));
}
