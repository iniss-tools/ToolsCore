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

    /// <summary>
    /// Nacita sady v poradi suboru. Chybajuci subor je prazdny zoznam.
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    public static List<TrainTextSet> Read(string stationDir)
    {
        var file = Path.Combine(stationDir, FileName);
        if (!File.Exists(file))
            return [];

        return Iniss2Xml.Load(file).Root!.Elements("b").Select(set => new TrainTextSet(Iniss2Xml.Text(set, "n"))
        {
            Targets = set.Elements("r").Select(target => new TextTarget(Iniss2Xml.Text(target, "cat"), Iniss2Xml.Text(target, "div"))).ToList(),
            Trains = set.Elements("t").Select(train =>
            {
                var text = train.Element("t");
                return new TrainText(Iniss2Xml.Text(train, "v"), Iniss2Xml.Text(train, "jd"), text is null ? "" : Iniss2Xml.Text(text, "s"),
                    text is null ? TrainTextSet.NoFont : Iniss2Xml.Number(text, "f", TrainTextSet.NoFont));
            }).ToList()
        }).ToList();
    }

    /// <summary>
    /// Zapise sady. Sada bez textov vlakov sa nezapisuje ako u servera.
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    /// <param name="sets">sady s roznymi nazvami</param>
    public static void Write(string stationDir, IEnumerable<TrainTextSet> sets) =>
        Iniss2Xml.Save(Path.Combine(stationDir, FileName), new XElement("TTEXTS", sets.Where(set => set.Trains.Count > 0).Select(set => new XElement("b",
            new XAttribute("n", set.Name),
            set.Targets.Select(target => new XElement("r", new XAttribute("cat", target.Catalog), new XAttribute("div", target.Column))),
            set.Trains.Select(train => new XElement("t", Iniss2Xml.Optional("jd", train.Validity), new XAttribute("v", train.Number),
                new XElement("t", Iniss2Xml.Optional("s", train.Text), Iniss2Xml.Optional("f", train.Font, TrainTextSet.NoFont))))))));
}
