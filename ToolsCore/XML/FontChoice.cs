using System.Globalization;
using System.Xml.Serialization;

namespace ToolsCore.XML;

/// <summary>
/// Pismo zvolene pouzivatelom: rodina a velkost v bodoch (rez je vzdy obycajny, tucne urcuje farebna schema).
/// </summary>
public sealed record FontChoice
{
    /// <summary>
    /// Pre XmlSerializer.
    /// </summary>
    public FontChoice()
    {
    }

    /// <summary>
    /// Pismo s danou rodinou a velkostou.
    /// </summary>
    public FontChoice(string family, float size)
    {
        Family = family;
        Size = size;
    }

    /// <summary>
    /// Rodina pisma.
    /// </summary>
    [XmlAttribute("family")]
    public string Family { get; set; } = "";

    /// <summary>
    /// Velkost v bodoch.
    /// </summary>
    [XmlAttribute("size")]
    public float Size { get; set; }

    /// <summary>
    /// Volba podla existujuceho pisma (rodina a velkost v bodoch).
    /// </summary>
    public static FontChoice Of(Font font) => new(font.FontFamily.Name, font.SizeInPoints);

    /// <summary>
    /// Vytvori pismo; chybajucu rodinu nahradi system.
    /// </summary>
    public Font ToFont() => new(Family, Size, FontStyle.Regular, GraphicsUnit.Point);

    /// <summary>
    /// Ta ista rodina a velkost ako <paramref name="font" />.
    /// </summary>
    public bool Matches(Font font) =>
        string.Equals(Family, font.FontFamily.Name, StringComparison.OrdinalIgnoreCase) && Math.Abs(Size - font.SizeInPoints) < 0.01f;

    /// <inheritdoc />
    public override string ToString() => Family + ", " + Size.ToString(CultureInfo.CurrentCulture);
}
