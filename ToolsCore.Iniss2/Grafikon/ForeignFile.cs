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
        if (!File.Exists(file))
            return [];

        return Iniss2Xml.Load(file).Root!.Elements("v")
            .Select(element => new TrainLanguages(Iniss2Xml.Text(element, "n"), Iniss2Xml.Text(element, "jd"),
                element.Elements("l").Select(language => language.Value).ToList()))
            .ToList();
    }

    /// <summary>
    /// Zapise zaznamy.
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    /// <param name="trains">zaznamy</param>
    public static void Write(string stationDir, IEnumerable<TrainLanguages> trains) =>
        Iniss2Xml.Save(Path.Combine(stationDir, FileName), new XElement("FOREIGN", trains.Select(train => new XElement("v",
            Iniss2Xml.Optional("jd", train.Validity), new XAttribute("n", train.Number), train.Languages.Select(language => new XElement("l", language))))));
}
