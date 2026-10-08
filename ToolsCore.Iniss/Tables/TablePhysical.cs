using ToolsCore.Iniss.Properties;

namespace ToolsCore.Iniss.Tables;

/// <summary>
/// Definuje fyzicku tabulu.
/// </summary>
public sealed class TablePhysical : ITable
{
    /// <summary>
    /// Identifikator tabule.
    /// </summary>
    public int ID { get; set; }

    /// <summary>
    /// Cislo komunikacneho portu ku fyzickej tabuli.
    /// </summary>
    public int CommunicationPort { get; set; }

    /// <summary>
    /// Pocet zaznamou, ktore dokaze tato tabula obsahovat.
    /// </summary>
    public int RecCount { get; set; }

    /// <summary>
    /// ?
    /// </summary>
    public string ReverseArrows { get; set; } = null!;

    /// <summary>
    /// ?
    /// </summary>
    public string Rem { get; set; } = null!;

    /// <summary>
    /// Nazov XML suboru, ktory bude generovat INISS. Tento subor obsahuje aktualny obsah tejto tabule.
    /// </summary>
    public string SaveXML { get; set; } = null!;

    /// <summary>
    /// Katalogova tabula definujuca obsah tejto fyzickej tabule.
    /// </summary>
    public TableCatalog TableCatalog { get; set; } = null!;

    /// <summary>
    /// Nazov fyzickej tatule.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Kluc fyzickej tatule.
    /// </summary>
    public string Key { get; set; } = null!;

    /// <summary>
    /// Textovy komentar ku tabuli.
    /// </summary>
    public string Comment { get; set; } = null!;

    /// <inheritdoc/>
    public string TypeName => Resources.TypeName_Physical;

    /// <inheritdoc />
    public override string ToString() => Name;
}