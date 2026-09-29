using System.Globalization;
using System.Xml.Serialization;

namespace ToolsCore.XML;

/// <summary>
/// Rozšírenie štruktúry <see cref="Color" /> o to, aby bola serializovateľná do XML.
/// </summary>
public struct XmlColor
{
    private Color _color;

    /// <summary>
    /// Vytvori novu instanciu struktury <see cref="XmlColor"/>.
    /// </summary>
    /// <param name="c"></param>
    public XmlColor(Color c) => _color = c;

    /// <summary>
    /// Farba ako text pre XML.
    /// </summary>
    [XmlText]
    public string Default
    {
        get => FromColor(_color);
        set => _color = ToColor(value);
    }

    /// <summary>
    /// Implicitny operator sluziaci na konvertovanie <see cref="XmlColor"/> na <see cref="Color"/>.
    /// </summary>
    /// <param name="x">Farba ako <see cref="XmlColor"/>.</param>
    /// <returns>farba ako <see cref="Color"/>.</returns>
    public static implicit operator Color(XmlColor x) => x._color;

    /// <summary>
    /// Implicitny operator sluziaci na konvertovanie <see cref="Color"/> na <see cref="XmlColor"/>.
    /// </summary>
    /// <param name="c">Farba ako <see cref="Color"/>.</param>
    /// <returns>farba ako <see cref="XmlColor"/>.</returns>
    public static implicit operator XmlColor(Color c) => new(c);

    /// <summary>
    /// Vygeneruje reťazec (string) podľa objektu typu <see cref="Color"/>.
    /// </summary>
    /// <param name="color">Farba ako objekt typu <see cref="Color"/>.</param>
    /// <returns>farba definovana textovej podobe.</returns>
    public static string FromColor(Color color)
    {
        if (color.IsNamedColor)
            return color.Name;

        var colorValue = color.ToArgb();

        return (uint)colorValue >> 24 == 0xFF ? $"#{colorValue & 0x00FFFFFF:X6}" : $"#{colorValue:X8}";
    }

    /// <summary>
    /// Konvertuje <see cref="string"/> value na objekt typu <see cref="Color"/>.
    /// </summary>
    /// <param name="value">Farba definovana textovej podobe.</param>
    /// <returns>farba ako objekt typu <see cref="Color"/>.</returns>
    public static Color ToColor(string value)
    {
        try
        {
            if (value[0] != '#') 
                return Color.FromName(value);
            var argb = (value.Length <= 7 ? unchecked((int)0xFF000000) : 0) + int.Parse(value.AsSpan(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            return Color.FromArgb(argb);
        }
        catch (Exception)
        {
            return Color.Empty;
        }
    }
}