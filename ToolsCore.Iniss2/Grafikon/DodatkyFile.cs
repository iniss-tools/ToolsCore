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
        return File.Exists(file) ? Parse(Iniss2Xml.Load(file).Root!) : [];
    }

    /// <summary>
    /// Zapise zaznamy. Typy hlasenia sa zapisuju vzdy ako u servera. Existujuci subor sa upravi na mieste.
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    /// <param name="additions">zaznamy</param>
    /// <returns><see langword="false" />, ak subor uz mal tento obsah.</returns>
    public static bool Write(string stationDir, IEnumerable<TrainAddition> additions) =>
        Iniss2Xml.Write(Path.Combine(stationDir, FileName), Build(additions), root => Build(Parse(root)));

    private static List<TrainAddition> Parse(XElement root) => root.Elements("v")
        .Select(element => new TrainAddition(Iniss2Xml.Text(element, "cv"), Iniss2Xml.Text(element, "k"), Iniss2Xml.Text(element, "rep"),
            Iniss2Xml.Text(element, "jd"))
        {
            EditedForDay = Iniss2Xml.Bool(element, "de")
        })
        .ToList();

    // poradie atributov ako v XmlSerializer servera
    private static XElement Build(IEnumerable<TrainAddition> additions) => new("DODATKY", additions.Select(addition => new XElement("v",
        Iniss2Xml.Optional("jd", addition.Validity), new XAttribute("cv", addition.Number), Iniss2Xml.Optional("de", addition.EditedForDay),
        new XAttribute("k", addition.Key), new XAttribute("rep", addition.Reports))));
}
