using ToolsCore.Iniss.Properties;

namespace ToolsCore.Iniss.Tables;

/// <summary>
/// Definuje jeden stlpec katalogovej tabule a jeho vlastnosti.
/// </summary>
public sealed class TableItem : ITable
{
    /// <summary>
    /// Kluc polozky.
    /// </summary>
    public string Key { get; set; } = null!;

    /// <summary>
    /// Nazov stlpca.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Typ obsahu stlpca (aky typ dat ma stlpec obsahovat).
    /// </summary>
    public TableFillSection FillSection { get; set; } = null!;

    /// <summary>
    /// Riadok zaznamu, na ktorom sa tento stlpec zobrazuje (zero-based).
    /// </summary>
    public int Line { get; set; }

    /// <summary>
    /// Pociatocny pixel (odkial stlpec zacina) (prvy pixel je 0).
    /// </summary>
    public int Start { get; set; }

    /// <summary>
    /// Posledny pixel (kde stlpec konci).
    /// </summary>
    public int End { get; set; }

    /// <summary>
    /// Identifikator pisma.
    /// </summary>
    public int FontIdx { get; set; }

    /// <summary>
    /// Zarovnanie pisma.
    /// </summary>
    public TableAlign Align { get; set; } = null!;

    /// <summary>
    /// Typ zadavania udajov do polozky.
    /// </summary>
    public TableDivType DivType { get; set; } = null!;

    /// <summary>
    /// Referencia na TabTab1.
    /// </summary>
    public TableTabTab Tab1 { get; set; } = null!;

    /// <summary>
    /// Referencia na TabTab2.
    /// </summary>
    public TableTabTab Tab2 { get; set; } = null!;

    /// <inheritdoc/>
    public string TypeName => Resources.TypeName_Item;

    /// <inheritdoc />
    public override string ToString() => Name;
}