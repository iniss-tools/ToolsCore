using System.Xml.Serialization;

namespace ToolsCore.XML;

/// <summary>
/// Zapamatane usporiadanie stlpcov jednej tabulky: poradie, viditelnost a sirka nastavena pouzivatelom.
/// </summary>
public sealed class GridLayout
{
    /// <summary>
    /// Identifikator tabulky v programe (napr. <c>Trains</c>).
    /// </summary>
    [XmlAttribute("id")]
    public string Id { get; set; } = "";

    /// <summary>
    /// Stlpce tabulky.
    /// </summary>
    [XmlElement("Column")]
    public List<GridColumnLayout> Columns { get; set; } = [];

    /// <summary>
    /// Kopia.
    /// </summary>
    public GridLayout Clone() => new() { Id = Id, Columns = Columns.Select(c => c with { }).ToList() };

    /// <summary>
    /// Rovnake usporiadanie stlpcov.
    /// </summary>
    public bool SameAs(GridLayout? other) => other is not null && Id == other.Id && Columns.SequenceEqual(other.Columns);
}

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
