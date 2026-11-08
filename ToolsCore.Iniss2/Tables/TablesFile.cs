using System.Xml.Linq;

namespace ToolsCore.Iniss2.Tables;

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
            table.Elements("part").Select(ReadPart).ToList())
        {
            ReverseArrows = Iniss2Xml.Text(table, "revarrows")
        }).ToList();
    }

    /// <summary>
    /// Zapise tabule. Adresa sa zapisuje vzdy, aj bez zariadenia (-1).
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    /// <param name="tables">tabule s roznymi klucmi</param>
    public static void Write(string stationDir, IEnumerable<TableDefinition> tables) =>
        Iniss2Xml.Save(Path.Combine(stationDir, FileName), new XElement("TABLES", tables.Select(table => new XElement("t",
            new XAttribute("k", table.Key), Iniss2Xml.Optional("n", table.Name), Iniss2Xml.Optional("lin", table.Line), new XAttribute("addr", table.Address),
            Iniss2Xml.Optional("save", table.SaveFile), Iniss2Xml.Optional("revarrows", table.ReverseArrows), table.Parts.Select(WritePart)))));

    private static TablePart ReadPart(XElement part) =>
        new(Iniss2Xml.Text(part, "n"), Iniss2Xml.Text(part, "ck"), Iniss2Xml.Text(part, "t"), Iniss2Xml.Number(part, "rec"))
        {
            Tracks = part.Elements("pos").Select(position => Iniss2Xml.Text(position, "k")).ToList(),
            TrackPriorities = part.Elements("pos").Where(position => position.Attribute("pri") is not null)
                .ToDictionary(position => Iniss2Xml.Text(position, "k"), position => Iniss2Xml.Number(position, "pri")),
            Stations = Iniss2Xml.Text(part, "st"),
            Previous = Iniss2Xml.Text(part, "prev")
        };

    private static XElement WritePart(TablePart part) => new("part",
        Iniss2Xml.Optional("n", part.Name), new XAttribute("ck", part.CatalogKey), new XAttribute("t", part.Kind), new XAttribute("rec", part.Records),
        Iniss2Xml.Optional("st", part.Stations), Iniss2Xml.Optional("prev", part.Previous),
        part.Tracks.Select(track => new XElement("pos", new XAttribute("k", track), Iniss2Xml.Optional("pri", part.TrackPriorities.GetValueOrDefault(track)))));
}
