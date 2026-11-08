using System.Xml.Linq;

namespace ToolsCore.Iniss2.Tables;

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
            .Select(table => new CatalogTable(Iniss2Xml.Text(table, "k"), Iniss2Xml.Text(table, "n"), Iniss2Xml.Text(table, "cls"))
            {
                Columns = table.Elements("div").Select(ReadColumn).ToList(),
                FontBank = Iniss2Xml.Text(table, "f")
            })
            .ToList();
    }

    /// <summary>
    /// Zapise predlohy.
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    /// <param name="catalogs">predlohy s roznymi klucmi</param>
    public static void Write(string stationDir, IEnumerable<CatalogTable> catalogs) =>
        Iniss2Xml.Save(Path.Combine(stationDir, FileName), new XElement("CATALOG", catalogs.Select(catalog => new XElement("t",
            new XAttribute("k", catalog.Key), Iniss2Xml.Optional("n", catalog.Name), new XAttribute("cls", catalog.Class), Iniss2Xml.Optional("f", catalog.FontBank),
            catalog.Columns.Select(WriteColumn)))));

    private static CatalogColumn ReadColumn(XElement column)
    {
        var position = column.Element("pos");

        return new CatalogColumn(Iniss2Xml.Text(column, "k"), Iniss2Xml.Text(column, "inf"), position is null ? -1 : Iniss2Xml.Number(position, "r"),
            position is null ? 0 : Iniss2Xml.Number(position, "c1"), position is null ? 0 : Iniss2Xml.Number(position, "c2"), Iniss2Xml.Text(column, "align"))
        {
            Name = Iniss2Xml.Text(column, "n"),
            Font = Iniss2Xml.Number(column, "fnt"),
            Translation = Iniss2Xml.Text(column, "trn") is { Length: > 0 } translation ? translation : CatalogColumn.FreeTranslation,
            Table1 = Iniss2Xml.Text(column, "t1"),
            Table2 = Iniss2Xml.Text(column, "t2")
        };
    }

    private static XElement WriteColumn(CatalogColumn column) => new("div",
        new XAttribute("k", column.Key), Iniss2Xml.Optional("n", column.Name), new XAttribute("inf", column.Content), Iniss2Xml.Optional("fnt", column.Font),
        Iniss2Xml.Optional("align", column.Align),
        column.Translation == CatalogColumn.FreeTranslation ? null : new XAttribute("trn", column.Translation),
        Iniss2Xml.Optional("t1", column.Table1), Iniss2Xml.Optional("t2", column.Table2),
        new XElement("pos", Iniss2Xml.Optional("r", column.Row), Iniss2Xml.Optional("c1", column.Start), Iniss2Xml.Optional("c2", column.End)));
}
