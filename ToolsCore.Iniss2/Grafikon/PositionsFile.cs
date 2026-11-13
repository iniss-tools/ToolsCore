using System.Xml.Linq;

namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Kolaje vlakov v stanici - subor Positions.xml v priecinku stanice.
/// </summary>
public static class PositionsFile
{
    /// <summary>
    /// Nazov suboru v priecinku stanice.
    /// </summary>
    public const string FileName = "Positions.xml";

    /// <summary>
    /// Nacita kolaje vlakov v poradi suboru - pri hladani plati prvy vyhovujuci zaznam. Chybajuci subor je prazdny zoznam.
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    public static List<TrainPosition> Read(string stationDir)
    {
        var file = Path.Combine(stationDir, FileName);
        return File.Exists(file) ? Parse(Iniss2Xml.Load(file).Root!) : [];
    }

    /// <summary>
    /// Zapise kolaje vlakov. Zaznam s poznamkou musi byt pred zaznamom bez poznamky. Existujuci subor sa upravi na mieste.
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    /// <param name="positions">zaznamy v poradi hladania</param>
    /// <returns><see langword="false" />, ak subor uz mal tento obsah.</returns>
    public static bool Write(string stationDir, IEnumerable<TrainPosition> positions) =>
        Iniss2Xml.Write(Path.Combine(stationDir, FileName), Build(positions), root => Build(Parse(root)));

    private static List<TrainPosition> Parse(XElement root) => root.Elements("pos").Select(element =>
    {
        var arrival = Iniss2Xml.Text(element, "p");
        var departure = Iniss2Xml.Text(element, "o");
        return new TrainPosition(Iniss2Xml.Text(element, "cv"), arrival, departure.Length > 0 ? departure : arrival, Iniss2Xml.Text(element, "jd"))
        {
            EditedForDay = Iniss2Xml.Bool(element, "de")
        };
    }).ToList();

    // poradie atributov ako v XmlSerializer servera
    private static XElement Build(IEnumerable<TrainPosition> positions) => new("POZICESPOJU", positions.Select(position => new XElement("pos",
        Iniss2Xml.Optional("jd", position.Validity),
        new XAttribute("cv", position.Train),
        Iniss2Xml.Optional("de", position.EditedForDay),
        Iniss2Xml.Optional("p", position.ArrivalTrack),
        Iniss2Xml.Optional("o", position.DepartureTrack))));
}
