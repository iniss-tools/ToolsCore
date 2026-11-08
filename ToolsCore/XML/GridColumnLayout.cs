using System.Xml.Serialization;

namespace ToolsCore.XML;

/// <summary>
/// Jeden stlpec v zapamatanom usporiadani tabulky.
/// </summary>
public sealed record GridColumnLayout
{
    /// <summary>
    /// Nazov stlpca (<see cref="DataGridViewColumn.Name" />).
    /// </summary>
    [XmlAttribute("name")]
    public string Name { get; set; } = "";

    /// <summary>
    /// Poradie zobrazenia.
    /// </summary>
    [XmlAttribute("order")]
    public int Order { get; set; }

    /// <summary>
    /// Stlpec je viditelny.
    /// </summary>
    [XmlAttribute("visible"), DefaultValue(true)]
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Sirka nastavena pouzivatelom v bodoch pri 96 DPI; 0 = sirka podla obsahu.
    /// </summary>
    [XmlAttribute("width"), DefaultValue(0)]
    public int Width { get; set; }
}
