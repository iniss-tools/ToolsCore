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
        if (!File.Exists(file))
            return [];

        return Iniss2Xml.Load(file).Root!.Elements("pos").Select(element =>
        {
            var arrival = Iniss2Xml.Text(element, "p");
            var departure = Iniss2Xml.Text(element, "o");
            return new TrainPosition(Iniss2Xml.Text(element, "cv"), arrival, departure.Length > 0 ? departure : arrival, Iniss2Xml.Text(element, "jd"));
        }).ToList();
    }

    /// <summary>
    /// Zapise kolaje vlakov. Zaznam s poznamkou musi byt pred zaznamom bez poznamky.
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    /// <param name="positions">zaznamy v poradi hladania</param>
    public static void Write(string stationDir, IEnumerable<TrainPosition> positions) =>
        Iniss2Xml.Save(Path.Combine(stationDir, FileName), new XElement("POZICESPOJU", positions.Select(position => new XElement("pos",
            Iniss2Xml.Optional("jd", position.Validity),
            new XAttribute("cv", position.Train),
            Iniss2Xml.Optional("p", position.ArrivalTrack),
            Iniss2Xml.Optional("o", position.DepartureTrack)))));
}
