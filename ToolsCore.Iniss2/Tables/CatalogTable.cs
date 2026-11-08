namespace ToolsCore.Iniss2.Tables;

/// <summary>
/// Katalogova predloha tabule.
/// </summary>
/// <param name="Key">kluc - podla neho sa na predlohu odkazuje cast tabule</param>
/// <param name="Name">nazov</param>
/// <param name="Class">druh zariadenia, napriklad LCD1; INTERNET = tabula bez zariadenia</param>
public sealed record CatalogTable(string Key, string Name, string Class)
{
    /// <summary>Druh predlohy bez zariadenia.</summary>
    public const string Internet = "INTERNET";

    /// <summary>Udaje predlohy v poradi suboru.</summary>
    public IReadOnlyList<CatalogColumn> Columns { get; init; } = [];

    /// <summary>Nazov banky pisem; povinny pri ELEN, ELENOLD, ELEN10, ELEN16 a ELEN16KAM.</summary>
    public string FontBank { get; init; } = "";

    /// <summary>Predloha ma zariadenie.</summary>
    public bool IsDevice => !string.Equals(Class, Internet, StringComparison.Ordinal);

    /// <summary>Riadkov tabule na jeden zaznam - podla najvyssieho riadku udajov, ako ich pocita server.</summary>
    public int LinesPerRecord => Columns.Count == 0 ? 1 : Math.Max(0, Columns.Max(column => column.Row)) + 1;
}
