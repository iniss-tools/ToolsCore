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
        if (!File.Exists(file))
            return [];

        return Iniss2Xml.Load(file).Root!.Elements("razvl")
            .Select(element => new TrainComposition(Iniss2Xml.Text(element, "v"), Iniss2Xml.Number(element, "est"), Iniss2Xml.Text(element, "rep"),
                Iniss2Xml.Text(element, "jd"), element.Elements("snd").Select(sound => sound.Value).ToList()))
            .ToList();
    }

    /// <summary>
    /// Zapise zaznamy.
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    /// <param name="compositions">zaznamy</param>
    public static void Write(string stationDir, IEnumerable<TrainComposition> compositions) =>
        Iniss2Xml.Save(Path.Combine(stationDir, FileName), new XElement("RAZENI", compositions.Select(composition => new XElement("razvl",
            Iniss2Xml.Optional("jd", composition.Validity), new XAttribute("v", composition.Number), Iniss2Xml.Optional("est", composition.EndStation),
            new XAttribute("rep", composition.Reports), composition.Sounds.Select(sound => new XElement("snd", sound))))));
}
