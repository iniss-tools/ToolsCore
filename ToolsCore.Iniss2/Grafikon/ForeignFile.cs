using System.Xml.Linq;

namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Jazyky hlaseni vlakov stanice - subor Foreign.xml v priecinku stanice. Pre cislo a den plati prvy zaznam.
/// </summary>
public static class ForeignFile
{
    /// <summary>
    /// Nazov suboru v priecinku stanice.
    /// </summary>
    public const string FileName = "Foreign.xml";

    /// <summary>
    /// Hodnota jazyka, ktora znamena vsetky jazyky banky.
    /// </summary>
    public const string AllLanguages = "1";

    /// <summary>
    /// Nacita zaznamy v poradi suboru. Chybajuci subor je prazdny zoznam.
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    public static List<TrainLanguages> Read(string stationDir)
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
    public static bool Write(string stationDir, IEnumerable<TrainLanguages> trains) =>
        Iniss2Xml.Write(Path.Combine(stationDir, FileName), Build(trains), root => Build(Parse(root)));

    private static List<TrainLanguages> Parse(XElement root) => root.Elements("v")
        .Select(element => new TrainLanguages(Iniss2Xml.Text(element, "n"), Iniss2Xml.Text(element, "jd"),
            element.Elements("l").Select(language => language.Value).ToList())
        {
            EditedForDay = Iniss2Xml.Bool(element, "de")
        })
        .ToList();

    // poradie atributov ako v XmlSerializer servera
    private static XElement Build(IEnumerable<TrainLanguages> trains) => new("FOREIGN", trains.Select(train => new XElement("v",
        Iniss2Xml.Optional("jd", train.Validity), new XAttribute("n", train.Number), Iniss2Xml.Optional("de", train.EditedForDay),
        train.Languages.Select(language => new XElement("l", language)))));
}
