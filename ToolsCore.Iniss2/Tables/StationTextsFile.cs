using System.Xml.Linq;

namespace ToolsCore.Iniss2.Tables;

/// <summary>
/// Texty tabul podla stanice - subor TTStanice.xml v priecinku stanice. Server ho len cita.
/// </summary>
public static class StationTextsFile
{
    /// <summary>
    /// Nazov suboru v priecinku stanice.
    /// </summary>
    public const string FileName = "TTStanice.xml";

    private const string FonetKey = "FONET";
    private static readonly Dictionary<string, int> SetFlagKeys = new() { ["NONE"] = 0, [FonetKey] = 1 };

    /// <summary>
    /// Nacita sady v poradi suboru. Chybajuci subor je prazdny zoznam.
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    public static List<StationTextSet> Read(string stationDir)
    {
        var file = Path.Combine(stationDir, FileName);
        return File.Exists(file) ? Parse(Iniss2Xml.Load(file).Root!) : [];
    }

    /// <summary>
    /// Zapise sady. Existujuci subor sa upravi na mieste (komentare a ostatne zaznamy ostanu po znakoch).
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    /// <param name="sets">sady v poradi</param>
    /// <returns><see langword="false" />, ak subor uz mal tento obsah.</returns>
    public static bool Write(string stationDir, IEnumerable<StationTextSet> sets) =>
        Iniss2Xml.Write(Path.Combine(stationDir, FileName), Build(sets), root => Build(Parse(root)));

    private static List<StationTextSet> Parse(XElement root) => root.Elements("b").Select(set => new StationTextSet(Iniss2Xml.Text(set, "n"))
    {
        Fonet = Iniss2Xml.Flags(set, "f", SetFlagKeys) != 0,
        Targets = set.Elements("r").Select(target => new TextTarget(Iniss2Xml.Text(target, "cat"), Iniss2Xml.Text(target, "div"))).ToList(),
        Stations = set.Elements("s").Select(station => new StationText(Iniss2Xml.Number(station, "k"), Iniss2Xml.Text(station, "t"),
            Iniss2Xml.Number(station, "i", TrainTextSet.NoFont))).ToList()
    }).ToList();

    private static XElement Build(IEnumerable<StationTextSet> sets) => new("TTEXTSST", sets.Select(set => new XElement("b",
        new XAttribute("n", set.Name),
        set.Fonet ? new XAttribute("f", FonetKey) : null,
        set.Targets.Select(target => new XElement("r", new XAttribute("cat", target.Catalog), new XAttribute("div", target.Column))),
        set.Stations.Select(station => new XElement("s", new XAttribute("k", station.Station), Iniss2Xml.Optional("t", station.Text),
            Iniss2Xml.Optional("i", station.Font, TrainTextSet.NoFont))))));
}
