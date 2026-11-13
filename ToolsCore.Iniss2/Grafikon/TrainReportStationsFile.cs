using System.Globalization;
using System.Xml.Linq;

namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Stanice hlasene pri vlakoch stanice - subor StaHlasVl.xml v priecinku stanice. Pre cislo a den plati prvy zaznam; server
/// subor prepise, ked obsluha zmeni stanice hlasenia vlaku.
/// </summary>
public static class TrainReportStationsFile
{
    /// <summary>
    /// Nazov suboru v priecinku stanice.
    /// </summary>
    public const string FileName = "StaHlasVl.xml";

    /// <summary>
    /// Nacita zaznamy v poradi suboru. Chybajuci subor je prazdny zoznam.
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    public static List<TrainReportStations> Read(string stationDir)
    {
        var file = Path.Combine(stationDir, FileName);
        return File.Exists(file) ? Parse(Iniss2Xml.Load(file).Root!) : [];
    }

    /// <summary>
    /// Zapise zaznamy. Existujuci subor sa upravi na mieste.
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    /// <param name="trains">zaznamy</param>
    /// <returns><see langword="false" />, ak subor uz mal tento obsah.</returns>
    public static bool Write(string stationDir, IEnumerable<TrainReportStations> trains) =>
        Iniss2Xml.Write(Path.Combine(stationDir, FileName), Build(trains), root => Build(Parse(root)));

    private static List<TrainReportStations> Parse(XElement root) => root.Elements("v")
        .Select(element => new TrainReportStations(Iniss2Xml.Text(element, "cv"), Iniss2Xml.Text(element, "jd"))
        {
            EditedForDay = Iniss2Xml.Bool(element, "de"),
            ShortFrom = Stations(element, "b"),
            LongFrom = Stations(element, "c"),
            ShortTo = Stations(element, "d"),
            LongTo = Stations(element, "e")
        })
        .ToList();

    // poradie atributov a prvkov ako v XmlSerializer servera
    private static XElement Build(IEnumerable<TrainReportStations> trains) => new("STAHLASVL", trains.Select(train => new XElement("v",
        Iniss2Xml.Optional("jd", train.Validity), new XAttribute("cv", train.Number), Iniss2Xml.Optional("de", train.EditedForDay),
        Elements("b", train.ShortFrom), Elements("c", train.LongFrom), Elements("d", train.ShortTo), Elements("e", train.LongTo))));

    private static List<int> Stations(XElement element, string name) =>
        [.. element.Elements(name).Select(station => int.Parse(station.Value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture))];

    private static IEnumerable<XElement> Elements(string name, IEnumerable<int> stations) =>
        stations.Select(station => new XElement(name, station.ToString(CultureInfo.InvariantCulture)));
}
