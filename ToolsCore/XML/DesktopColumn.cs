using System.Xml.Serialization;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace ToolsCore.XML;

/// <summary>
///     Reprezentuje jeden stlpec, ktory sa sa moze vyskytovat tabulke DataGridView na pracovnej ploche.
/// </summary>
public record DesktopColumn()
{
    public DesktopColumn(string name, string propname, int order, int minWidth, bool visible = true) : this()
    {
        Name = name;
        PropertyName = propname;
        MinWidth = minWidth;
        Order = order;
        Visible = visible;
    }

    /// <summary>
    ///     Nazov property.
    /// </summary>
    [XmlIgnore]
    public string PropertyName { get; set; } = null!;

    /// <summary>
    ///     Nazov stlpca, ktory sa zobrazuje v hlavicke stlpca.
    /// </summary>
    [XmlIgnore]
    public string Name { get; set; } = null!;

    /// <summary>
    ///     Aktualne poradove cislo umiestnenia tohto stlpca v tabulke.
    /// </summary>
    [XmlAttribute("o")]
    public int Order { get; set; }

    /// <summary>
    ///     Urcuje, ci sa ma stlpec zobrazovat pouzivatelovi v tabulke.
    /// </summary>
    [XmlAttribute("v")]
    public bool Visible { get; set; }

    /// <summary>
    ///     Urcuje minimalnu sirku stlpca (pouzivatel potom nemoze zmensit stlpec pod tuto hodnotu).
    /// </summary>
    [XmlAttribute("mw")]
    public int MinWidth { get; set; }
}