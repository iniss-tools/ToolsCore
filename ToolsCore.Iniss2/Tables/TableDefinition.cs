namespace ToolsCore.Iniss2.Tables;

/// <summary>
/// Tabula stanice.
/// </summary>
/// <param name="Key">kluc</param>
/// <param name="Name">nazov</param>
/// <param name="Line">kluc komunikacnej linky; prazdny = tabula nema zariadenie</param>
/// <param name="Address">adresa na linke</param>
/// <param name="SaveFile">subor XML v priecinku logov; prazdny = tabula sa do XML neuklada</param>
/// <param name="Parts">casti tabule</param>
public sealed record TableDefinition(string Key, string Name, string Line, int Address, string SaveFile, IReadOnlyList<TablePart> Parts)
{
    /// <summary>Bez adresy - tabula so zariadenim nekomunikuje.</summary>
    public const int NoAddress = -1;

    /// <summary>Cisla zaznamov s otocenymi sipkami.</summary>
    public string ReverseArrows { get; init; } = "";
}
