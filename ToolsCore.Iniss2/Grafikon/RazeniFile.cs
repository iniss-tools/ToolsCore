using System.Xml.Linq;

namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Radenie vlakov stanice - subor Razeni.xml v priecinku stanice. Zvuky vsetkych vyhovujucich zaznamov sa spoja.
/// </summary>
public static class RazeniFile
{
    /// <summary>
    /// Nazov suboru v priecinku stanice.
    /// </summary>
    public const string FileName = "Razeni.xml";

    /// <summary>
    /// Nacita zaznamy v poradi suboru. Chybajuci subor je prazdny zoznam.
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    public static List<TrainComposition> Read(string stationDir)
    {
        var file = Path.Combine(stationDir, FileName);
        return File.Exists(file) ? Parse(Iniss2Xml.Load(file).Root!) : [];
    }

    /// <summary>
    /// Zapise zaznamy. Existujuci subor sa upravi na mieste.
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    /// <param name="compositions">zaznamy</param>
    /// <returns><see langword="false" />, ak subor uz mal tento obsah.</returns>
    public static bool Write(string stationDir, IEnumerable<TrainComposition> compositions) =>
        Iniss2Xml.Write(Path.Combine(stationDir, FileName), Build(compositions), root => Build(Parse(root)));

    private static List<TrainComposition> Parse(XElement root) => root.Elements("razvl")
        .Select(element => new TrainComposition(Iniss2Xml.Text(element, "v"), Iniss2Xml.Number(element, "est"), Iniss2Xml.Text(element, "rep"),
            Iniss2Xml.Text(element, "jd"), element.Elements("snd").Select(sound => sound.Value).ToList())
        {
            EditedForDay = Iniss2Xml.Bool(element, "de"),
            Origin = element.Element("editvl") is { } origin
                ? new CompositionOrigin(Iniss2Xml.Text(origin, "prepis"),
                    origin.Elements("text").Select(text => (Iniss2Xml.Text(text, "l"), Iniss2Xml.Text(text, "t"))).ToList())
                : null
        })
        .ToList();

    // poradie atributov a prvkov ako v XmlSerializer servera
    private static XElement Build(IEnumerable<TrainComposition> compositions) => new("RAZENI", compositions.Select(composition => new XElement("razvl",
        Iniss2Xml.Optional("jd", composition.Validity), Iniss2Xml.Optional("de", composition.EditedForDay), new XAttribute("v", composition.Number),
        Iniss2Xml.Optional("est", composition.EndStation), new XAttribute("rep", composition.Reports),
        composition.Origin is { } origin
            ? new XElement("editvl", Iniss2Xml.Optional("prepis", origin.Source),
                origin.Texts.Select(text => new XElement("text", new XAttribute("l", text.Language), new XAttribute("t", text.Text))))
            : null,
        composition.Sounds.Select(sound => new XElement("snd", sound)))));
}
