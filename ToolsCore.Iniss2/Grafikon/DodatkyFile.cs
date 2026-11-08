using System.Xml.Linq;

namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Dodatky vlakov stanice - subor Dodatky.xml v priecinku stanice. Plati kazdy zaznam, ktory vlaku v den vyhovuje.
/// </summary>
public static class DodatkyFile
{
    /// <summary>
    /// Nazov suboru v priecinku stanice.
    /// </summary>
    public const string FileName = "Dodatky.xml";

    /// <summary>
    /// Nacita zaznamy v poradi suboru. Chybajuci subor je prazdny zoznam.
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    public static List<TrainAddition> Read(string stationDir)
    {
        var file = Path.Combine(stationDir, FileName);
        if (!File.Exists(file))
            return [];

        return Iniss2Xml.Load(file).Root!.Elements("v")
            .Select(element => new TrainAddition(Iniss2Xml.Text(element, "cv"), Iniss2Xml.Text(element, "k"), Iniss2Xml.Text(element, "rep"),
                Iniss2Xml.Text(element, "jd")))
            .ToList();
    }

    /// <summary>
    /// Zapise zaznamy. Typy hlasenia sa zapisuju vzdy ako u servera.
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    /// <param name="additions">zaznamy</param>
    public static void Write(string stationDir, IEnumerable<TrainAddition> additions) =>
        Iniss2Xml.Save(Path.Combine(stationDir, FileName), new XElement("DODATKY", additions.Select(addition => new XElement("v",
            Iniss2Xml.Optional("jd", addition.Validity), new XAttribute("cv", addition.Number), new XAttribute("k", addition.Key),
            new XAttribute("rep", addition.Reports)))));
}
