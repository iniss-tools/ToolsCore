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
