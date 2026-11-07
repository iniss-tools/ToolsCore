using System.Xml.Linq;

namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Dodatok vlaku v stanici.
/// </summary>
/// <param name="Number">cislo vlaku pri odchode zo stanice</param>
/// <param name="Key">nazov hlasenia v LogText alebo cislo zaznamu ConstReports.xml</param>
/// <param name="Reports">pismena typov hlasenia; prazdne = vsetky typy, <c>-</c> = ziadny</param>
/// <param name="Validity">datumova poznamka; prazdna = plati vzdy</param>
public sealed record TrainAddition(string Number, string Key, string Reports, string Validity);

/// <summary>
/// Radenie vlaku v stanici ako postupnost zvukov.
/// </summary>
/// <param name="Number">cislo vlaku pri odchode zo stanice</param>
/// <param name="EndStation">cislo cielovej stanice; 0 = kazdy ciel</param>
/// <param name="Reports">pismena typov hlasenia ako v <see cref="TrainAddition" /></param>
/// <param name="Validity">datumova poznamka; prazdna = plati vzdy</param>
/// <param name="Sounds">zvuky v tvare <c>jazyk/skupina/zvuk</c> v poradi prehravania</param>
public sealed record TrainComposition(string Number, int EndStation, string Reports, string Validity, IReadOnlyList<string> Sounds);

/// <summary>
/// Jazyky hlasenia vlaku v stanici.
/// </summary>
/// <param name="Number">cislo vlaku pri odchode zo stanice</param>
/// <param name="Validity">datumova poznamka; prazdna = plati vzdy</param>
/// <param name="Languages">kluce jazykov banky; <c>1</c> = vsetky jazyky</param>
public sealed record TrainLanguages(string Number, string Validity, IReadOnlyList<string> Languages);

/// <summary>
/// Hlasenie stanice s pismenom, podla ktoreho sa vyberaju dodatky a radenie.
/// </summary>
/// <param name="Name">nazov hlasenia ako v LogText a statedgm.xml</param>
/// <param name="Key">pismeno; dlhe hlasenie velke, kratke male</param>
/// <param name="Categories">kategorie vlakov <c>V</c>, <c>P</c>, <c>K</c> oddelene medzerou</param>
public sealed record ReportKey(string Name, string Key, string Categories);

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

/// <summary>
/// Hlasenia stanice s pismenami - subor ReportKeys.xml v priecinku stanice. Server sa bez neho nespusti.
/// </summary>
public static class ReportKeysFile
{
    /// <summary>
    /// Nazov suboru v priecinku stanice.
    /// </summary>
    public const string FileName = "ReportKeys.xml";

    /// <summary>
    /// Nacita hlasenia v poradi suboru. Chybajuci subor je prazdny zoznam.
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    public static List<ReportKey> Read(string stationDir)
    {
        var file = Path.Combine(stationDir, FileName);
        if (!File.Exists(file))
            return [];

        return Iniss2Xml.Load(file).Root!.Elements("r")
            .Select(element => new ReportKey(Iniss2Xml.Text(element, "n"), Iniss2Xml.Text(element, "k"), Iniss2Xml.Text(element, "v")))
            .ToList();
    }

    /// <summary>
    /// Zapise hlasenia.
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    /// <param name="reports">hlasenia s roznymi nazvami</param>
    public static void Write(string stationDir, IEnumerable<ReportKey> reports) =>
        Iniss2Xml.Save(Path.Combine(stationDir, FileName), new XElement("ReportKeys", reports.Select(report => new XElement("r",
            new XAttribute("n", report.Name), new XAttribute("k", report.Key), new XAttribute("v", report.Categories)))));
}
