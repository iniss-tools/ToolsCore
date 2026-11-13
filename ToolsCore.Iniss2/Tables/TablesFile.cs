using System.Xml.Linq;

namespace ToolsCore.Iniss2.Tables;

/// <summary>
/// Tabule stanice - subor Tables.xml v priecinku stanice. Server ho nikdy nezapisuje.
/// </summary>
public static class TablesFile
{
    /// <summary>
    /// Nazov suboru v priecinku stanice.
    /// </summary>
    public const string FileName = "Tables.xml";

    private const string NoScrollKey = "NOSCROLL";

    /// <summary>
    /// Nacita tabule stanice. Chybajuci subor je prazdny zoznam.
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    public static List<TableDefinition> Read(string stationDir)
    {
        var file = Path.Combine(stationDir, FileName);
        return File.Exists(file) ? Parse(Iniss2Xml.Load(file).Root!) : [];
    }

    /// <summary>
    /// Zapise tabule. Existujuci subor sa upravi na mieste (komentare a nezmenene tabule ostanu po znakoch). Adresa sa zapisuje
    /// vzdy, aj bez zariadenia (-1).
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    /// <param name="tables">tabule s roznymi klucmi</param>
    /// <returns><see langword="false" />, ak subor uz mal tento obsah.</returns>
    public static bool Write(string stationDir, IEnumerable<TableDefinition> tables) =>
        Iniss2Xml.Write(Path.Combine(stationDir, FileName), Build(tables), root => Build(Parse(root)));

    private static List<TableDefinition> Parse(XElement root) => root.Elements("t").Select(table => new TableDefinition(
        Iniss2Xml.Text(table, "k"),
        Iniss2Xml.Text(table, "n"),
        Iniss2Xml.Text(table, "lin"),
        // tabula bez adresy nekomunikuje, server ju ma predvolene -1
        Iniss2Xml.Number(table, "addr", TableDefinition.NoAddress),
        Iniss2Xml.Text(table, "save"),
        table.Elements("part").Select(ParsePart).ToList())
    {
        ReverseArrows = Iniss2Xml.Text(table, "revarrows"),
        SubLine = Iniss2Xml.Text(table, "lin_podr"),
        NoScroll = Iniss2Xml.Text(table, "f").Split(' ', StringSplitOptions.RemoveEmptyEntries).Contains(NoScrollKey, StringComparer.OrdinalIgnoreCase),
        LightTime0 = Iniss2Xml.Text(table, "lighttime0"),
        LightTime1 = Iniss2Xml.Text(table, "lighttime1"),
        Light0 = Iniss2Xml.Text(table, "light0"),
        Light1 = Iniss2Xml.Text(table, "light1"),
        LightOff0 = Iniss2Xml.Text(table, "lightoff0"),
        LightOff1 = Iniss2Xml.Text(table, "lightoff1"),
        LightIfEmpty0 = Iniss2Xml.Text(table, "lightIfEmpty0"),
        LightIfEmpty1 = Iniss2Xml.Text(table, "lightIfEmpty1"),
        LightOnTime = Iniss2Xml.Text(table, "lightOnTime")
    }).ToList();

    private static TablePart ParsePart(XElement part) =>
        new(Iniss2Xml.Text(part, "n"), Iniss2Xml.Text(part, "ck"), Iniss2Xml.Text(part, "t"), Iniss2Xml.Number(part, "rec"))
        {
            Tracks = part.Elements("pos").Select(position => Iniss2Xml.Text(position, "k")).ToList(),
            TrackPriorities = part.Elements("pos").Where(position => position.Attribute("pri") is not null)
                .GroupBy(position => Iniss2Xml.Text(position, "k"))
                .ToDictionary(group => group.Key, group => Iniss2Xml.Number(group.First(), "pri")),
            Stations = Iniss2Xml.Text(part, "st"),
            Previous = Iniss2Xml.Text(part, "prev"),
            WebSource = Iniss2Xml.Text(part, "websource"),
            Condition = Iniss2Xml.Text(part, "cond"),
            WindowDefinition = Iniss2Xml.Text(part, "windef")
        };

    // atributy v poradi XmlSerializer servera (Table, Table.Part, Position)
    private static XElement Build(IEnumerable<TableDefinition> tables) => new("TABLES", tables.Select(table => new XElement("t",
        new XAttribute("k", table.Key), Iniss2Xml.Optional("n", table.Name), Iniss2Xml.Optional("lin", table.Line), Iniss2Xml.Optional("lin_podr", table.SubLine),
        new XAttribute("addr", table.Address), Iniss2Xml.Optional("save", table.SaveFile), Iniss2Xml.Optional("revarrows", table.ReverseArrows),
        table.NoScroll ? new XAttribute("f", NoScrollKey) : null,
        Iniss2Xml.Optional("lighttime0", table.LightTime0), Iniss2Xml.Optional("lighttime1", table.LightTime1), Iniss2Xml.Optional("light0", table.Light0),
        Iniss2Xml.Optional("light1", table.Light1), Iniss2Xml.Optional("lightoff0", table.LightOff0), Iniss2Xml.Optional("lightoff1", table.LightOff1),
        Iniss2Xml.Optional("lightIfEmpty0", table.LightIfEmpty0), Iniss2Xml.Optional("lightIfEmpty1", table.LightIfEmpty1),
        Iniss2Xml.Optional("lightOnTime", table.LightOnTime),
        table.Parts.Select(BuildPart))));

    private static XElement BuildPart(TablePart part) => new("part",
        Iniss2Xml.Optional("n", part.Name), new XAttribute("ck", part.CatalogKey), Iniss2Xml.Optional("t", part.Kind), Iniss2Xml.Optional("websource", part.WebSource),
        Iniss2Xml.Optional("st", part.Stations), new XAttribute("rec", part.Records), Iniss2Xml.Optional("prev", part.Previous),
        Iniss2Xml.Optional("cond", part.Condition), Iniss2Xml.Optional("windef", part.WindowDefinition),
        part.Tracks.Select(track => new XElement("pos", new XAttribute("k", track), Iniss2Xml.Optional("pri", part.TrackPriorities.GetValueOrDefault(track)))));
}
