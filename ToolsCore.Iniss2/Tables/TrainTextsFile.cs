using System.Xml.Linq;

namespace ToolsCore.Iniss2.Tables;

/// <summary>
/// Texty vlakov pre tabule stanice - subor TTVlaky.xml v priecinku stanice.
/// </summary>
public static class TrainTextsFile
{
    /// <summary>
    /// Nazov suboru v priecinku stanice.
    /// </summary>
    public const string FileName = "TTVlaky.xml";

    private const string FonetKey = "FONET";

    private static readonly Dictionary<string, int> SetFlagKeys = new() { ["NONE"] = 0, [FonetKey] = 1 };

    /// <summary>
    /// Nacita sady v poradi suboru. Chybajuci subor je prazdny zoznam.
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    public static List<TrainTextSet> Read(string stationDir)
    {
        var file = Path.Combine(stationDir, FileName);
        return File.Exists(file) ? Parse(Iniss2Xml.Load(file).Root!) : [];
    }

    /// <summary>
    /// Zapise sady. Sada bez textov vlakov sa nezapisuje ako u servera. Existujuci subor sa upravi na mieste.
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    /// <param name="sets">sady s roznymi nazvami</param>
    /// <returns><see langword="false" />, ak subor uz mal tento obsah.</returns>
    public static bool Write(string stationDir, IEnumerable<TrainTextSet> sets) =>
        Iniss2Xml.Write(Path.Combine(stationDir, FileName), Build(sets), root => Build(Parse(root)));

    /// <summary>
    /// Odstrani texty vlakov a sady s udajmi tabul ponecha (nova stanica z predlohy). Server taku sadu precita, pri najblizsom
    /// zapise ju vynecha. Chybajuci subor necha tak.
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    public static void RemoveTrains(string stationDir)
    {
        var file = Path.Combine(stationDir, FileName);
        if (!File.Exists(file))
            return;

        var root = Iniss2Xml.Load(file).Root!;
        root.Elements("b").Elements("t").Remove();
        Iniss2Xml.Save(file, root);
    }

    private static List<TrainTextSet> Parse(XElement root) => root.Elements("b").Select(set => new TrainTextSet(Iniss2Xml.Text(set, "n"))
    {
        Fonet = Iniss2Xml.Flags(set, "f", SetFlagKeys) != 0,
        Targets = set.Elements("r").Select(target => new TextTarget(Iniss2Xml.Text(target, "cat"), Iniss2Xml.Text(target, "div"))).ToList(),
        Trains = set.Elements("t").Select(train =>
        {
            var text = train.Element("t");
            return new TrainText(Iniss2Xml.Text(train, "v"), Iniss2Xml.Text(train, "jd"), text is null ? "" : Iniss2Xml.Text(text, "s"),
                text is null ? TrainTextSet.NoFont : Iniss2Xml.Number(text, "f", TrainTextSet.NoFont))
            {
                EditedForDay = Iniss2Xml.Bool(train, "de")
            };
        }).ToList()
    }).ToList();

    // poradie atributov a prvkov ako v XmlSerializer servera
    private static XElement Build(IEnumerable<TrainTextSet> sets) => new("TTEXTS", sets.Where(set => set.Trains.Count > 0).Select(set => new XElement("b",
        new XAttribute("n", set.Name),
        set.Fonet ? new XAttribute("f", FonetKey) : null,
        set.Targets.Select(target => new XElement("r", new XAttribute("cat", target.Catalog), new XAttribute("div", target.Column))),
        set.Trains.Select(train => new XElement("t", Iniss2Xml.Optional("jd", train.Validity), new XAttribute("v", train.Number),
            Iniss2Xml.Optional("de", train.EditedForDay),
            new XElement("t", Iniss2Xml.Optional("s", train.Text), Iniss2Xml.Optional("f", train.Font, TrainTextSet.NoFont)))))));
}
