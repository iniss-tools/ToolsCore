using ToolsCore.Iniss.Properties;

namespace ToolsCore.Iniss.Tables;

/// <summary>
/// Trieda reprezentujuca logicku tabulu.
/// </summary>
public sealed class TableLogical : ITable
{
    /// <summary>
    /// Vytvori novu instanciu triedy <see cref="TableLogical"/>.
    /// </summary>
    public TableLogical() => Records = [];

    /// <summary>
    /// Typ pouzitia logickej tabule.
    /// </summary>
    public TableViewType ViewType { get; set; } = null!;

    /// <summary>
    /// Dalsie data ohladom pouzitia tejto logickej tabule.
    /// </summary>
    public string TypeViewFlags { get; set; } = null!;

    /// <summary>
    /// Cislo stanice, na ktoru je logicka tabula viazana (kluc IDSTATION v TLogical.TXT); 0 = neuvedene.
    /// INISS hodnotu nacita, GVDEditor ju len zachovava.
    /// </summary>
    public int IdStation { get; set; }

    /// <summary>
    /// Zaznamy logickej tabule.
    /// </summary>
    public List<TableRecord> Records { get; set; }

    /// <summary>
    /// Odkaz na seba, pouzivane pre DataSource.
    /// </summary>
    public TableLogical This => this;

    /// <summary>
    /// Nazov logickej tatule.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Kluc logickej tatule.
    /// </summary>
    public string Key { get; set; } = null!;

    /// <summary>
    /// Textovy komentar ku tabuli.
    /// </summary>
    public string Comment { get; set; } = null!;

    /// <inheritdoc/>
    public string TypeName => Resources.TypeName_Logical;

    /// <inheritdoc />
    public override string ToString() => Name;
}