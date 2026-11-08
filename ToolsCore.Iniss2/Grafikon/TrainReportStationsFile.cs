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
        if (!File.Exists(file))
            return [];

        return Iniss2Xml.Load(file).Root!.Elements("v")
            .Select(element => new TrainReportStations(Iniss2Xml.Text(element, "cv"), Iniss2Xml.Text(element, "jd"))
            {
                ShortFrom = Stations(element, "b"),
                LongFrom = Stations(element, "c"),
                ShortTo = Stations(element, "d"),
                LongTo = Stations(element, "e")
            })
            .ToList();
    }

    /// <summary>
    /// Zapise zaznamy.
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    /// <param name="trains">zaznamy</param>
    public static void Write(string stationDir, IEnumerable<TrainReportStations> trains) =>
        Iniss2Xml.Save(Path.Combine(stationDir, FileName), new XElement("STAHLASVL", trains.Select(train => new XElement("v",
            new XAttribute("cv", train.Number), Iniss2Xml.Optional("jd", train.Validity),
            Elements("b", train.ShortFrom), Elements("c", train.LongFrom), Elements("d", train.ShortTo), Elements("e", train.LongTo)))));

    private static List<int> Stations(XElement element, string name) =>
        [.. element.Elements(name).Select(station => int.Parse(station.Value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture))];

    private static IEnumerable<XElement> Elements(string name, IEnumerable<int> stations) =>
        stations.Select(station => new XElement(name, station.ToString(CultureInfo.InvariantCulture)));
}
