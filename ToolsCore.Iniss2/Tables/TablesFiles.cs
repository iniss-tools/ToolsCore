namespace ToolsCore.Iniss2.Tables;

/// <summary>
/// Cast tabule - jej obsah a katalogova predloha.
/// </summary>
/// <param name="Name">nazov</param>
/// <param name="CatalogKey">kluc predlohy v katalogu stanice</param>
/// <param name="Kind">druh casti, napriklad DEPARTURE</param>
/// <param name="Records">pocet zaznamov</param>
public sealed record TablePart(string Name, string CatalogKey, string Kind, int Records);

/// <summary>
/// Tabula stanice.
/// </summary>
/// <param name="Key">kluc</param>
/// <param name="Name">nazov</param>
/// <param name="Line">kluc komunikacnej linky; prazdny = tabula nema zariadenie</param>
/// <param name="Address">adresa na linke</param>
/// <param name="SaveFile">subor XML v priecinku logov; prazdny = tabula sa do XML neuklada</param>
/// <param name="Parts">casti tabule</param>
public sealed record TableDefinition(string Key, string Name, string Line, int Address, string SaveFile, IReadOnlyList<TablePart> Parts);

/// <summary>
/// Tabule stanice - subor Tables.xml v priecinku stanice.
/// </summary>
public static class TablesFile
{
    /// <summary>
    /// Nazov suboru v priecinku stanice.
    /// </summary>
    public const string FileName = "Tables.xml";

    /// <summary>
    /// Nacita tabule stanice. Chybajuci subor je prazdny zoznam.
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    public static List<TableDefinition> Read(string stationDir)
    {
        var file = Path.Combine(stationDir, FileName);
        if (!File.Exists(file))
            return [];

        return Iniss2Xml.Load(file).Root!.Elements("t").Select(table => new TableDefinition(
            Iniss2Xml.Text(table, "k"),
            Iniss2Xml.Text(table, "n"),
            Iniss2Xml.Text(table, "lin"),
            Iniss2Xml.Number(table, "addr"),
            Iniss2Xml.Text(table, "save"),
            table.Elements("part").Select(part => new TablePart(Iniss2Xml.Text(part, "n"), Iniss2Xml.Text(part, "ck"), Iniss2Xml.Text(part, "t"), Iniss2Xml.Number(part, "rec"))).ToList())).ToList();
    }
}

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

    /// <summary>Predloha ma zariadenie.</summary>
    public bool IsDevice => !string.Equals(Class, Internet, StringComparison.Ordinal);
}

/// <summary>
/// Katalog tabul stanice - subor TCatalog.xml v priecinku stanice.
/// </summary>
public static class TableCatalogFile
{
    /// <summary>
    /// Nazov suboru v priecinku stanice.
    /// </summary>
    public const string FileName = "TCatalog.xml";

    /// <summary>
    /// Nacita predlohy stanice. Chybajuci subor je prazdny zoznam.
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    public static List<CatalogTable> Read(string stationDir)
    {
        var file = Path.Combine(stationDir, FileName);
        if (!File.Exists(file))
            return [];

        return Iniss2Xml.Load(file).Root!.Elements("t")
            .Select(table => new CatalogTable(Iniss2Xml.Text(table, "k"), Iniss2Xml.Text(table, "n"), Iniss2Xml.Text(table, "cls")))
            .ToList();
    }
}
