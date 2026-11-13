using System.Xml.Linq;

namespace ToolsCore.Iniss2.Tables;

/// <summary>
/// Texty tabul podla trasy vlaku - subor TTTrasy.xml v priecinku stanice. Server ho len cita.
/// </summary>
public static class RouteTextsFile
{
    /// <summary>
    /// Nazov suboru v priecinku stanice.
    /// </summary>
    public const string FileName = "TTTrasy.xml";

    private const string FonetKey = "FONET";
    private const string SelectedKey = "S";
    private static readonly Dictionary<string, int> SetFlagKeys = new() { ["NONE"] = 0, [FonetKey] = 1 };

    /// <summary>
    /// Nacita sady v poradi suboru. Chybajuci subor je prazdny zoznam.
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    public static List<RouteTextSet> Read(string stationDir)
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
    public static bool Write(string stationDir, IEnumerable<RouteTextSet> sets) =>
        Iniss2Xml.Write(Path.Combine(stationDir, FileName), Build(sets), root => Build(Parse(root)));

    private static List<RouteTextSet> Parse(XElement root) => root.Elements("b").Select(set => new RouteTextSet(Iniss2Xml.Text(set, "n"))
    {
        Fonet = Iniss2Xml.Flags(set, "f", SetFlagKeys) != 0,
        Targets = set.Elements("r").Select(target => new TextTarget(Iniss2Xml.Text(target, "cat"), Iniss2Xml.Text(target, "div"))).ToList(),
        Routes = set.Elements("t").Select(route => new RouteText(Iniss2Xml.Text(route, "t"), Iniss2Xml.Number(route, "i", TrainTextSet.NoFont))
        {
            Stations = route.Elements("s").Select(station => new RouteStation(Iniss2Xml.Number(station, "k"), Iniss2Xml.Text(station, "f") == SelectedKey))
                .ToList()
        }).ToList()
    }).ToList();

    private static XElement Build(IEnumerable<RouteTextSet> sets) => new("TTEXTSTRC", sets.Select(set => new XElement("b",
        new XAttribute("n", set.Name),
        set.Fonet ? new XAttribute("f", FonetKey) : null,
        set.Targets.Select(target => new XElement("r", new XAttribute("cat", target.Catalog), new XAttribute("div", target.Column))),
        set.Routes.Select(route => new XElement("t", Iniss2Xml.Optional("t", route.Text), Iniss2Xml.Optional("i", route.Font, TrainTextSet.NoFont),
            route.Stations.Select(station => new XElement("s", new XAttribute("k", station.Station), station.Selected ? new XAttribute("f", SelectedKey) : null)))))));
}
