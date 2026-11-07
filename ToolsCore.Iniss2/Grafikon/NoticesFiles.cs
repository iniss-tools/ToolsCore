using System.Globalization;
using System.Xml.Linq;

namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Vyluka vlaku v grafikone.
/// </summary>
/// <param name="Number">cislo vlaku</param>
/// <param name="Key">cislo zaznamu ConstReports.xml alebo nazov funkcie <c>Vyluka</c> v LogText</param>
/// <param name="From">prvy den obdobia</param>
/// <param name="To">posledny den obdobia</param>
/// <param name="Validity">datumova poznamka v obdobi; prazdna = vsetky dni</param>
public sealed record TrainLockout(string Number, string Key, DateOnly From, DateOnly To, string Validity);

/// <summary>
/// Odklon vlaku v stanici grafikonu.
/// </summary>
/// <param name="Number">cislo vlaku pri odchode zo stanice</param>
/// <param name="Station">cislo stanice</param>
/// <param name="Key">cislo zaznamu ConstReports.xml alebo nazov funkcie <c>Odklon</c> v LogText</param>
/// <param name="From">prvy den obdobia</param>
/// <param name="To">posledny den obdobia</param>
/// <param name="Validity">datumova poznamka v obdobi; prazdna = vsetky dni</param>
public sealed record TrainDiversion(string Number, int Station, string Key, DateOnly From, DateOnly To, string Validity);

/// <summary>
/// Hlasenie definovane obsluhou: vzor z LogText so stanicami.
/// </summary>
/// <param name="Key">cislo, na ktore sa odkazuju dodatky, vyluky a odklony</param>
/// <param name="Name">nazov pre obsluhu</param>
/// <param name="Function">nazov vzoru v LogText</param>
/// <param name="Stations">cisla stanic v poradi</param>
public sealed record ConstReport(int Key, string Name, string Function, IReadOnlyList<int> Stations);

/// <summary>
/// Vyluky vlakov grafikonu - subor Vyluky.xml v priecinku grafikonu.
/// </summary>
public static class VylukyFile
{
    /// <summary>
    /// Nazov suboru v priecinku grafikonu.
    /// </summary>
    public const string FileName = "Vyluky.xml";

    /// <summary>
    /// Nacita zaznamy v poradi suboru. Chybajuci subor je prazdny zoznam.
    /// </summary>
    /// <param name="groupDir">priecinok grafikonu</param>
    public static List<TrainLockout> Read(string groupDir)
    {
        var file = Path.Combine(groupDir, FileName);
        if (!File.Exists(file))
            return [];

        return Iniss2Xml.Load(file).Root!.Elements("v")
            .Select(element => new TrainLockout(Iniss2Xml.Text(element, "cv"), Iniss2Xml.Text(element, "k"), NoticeDates.Read(element, "od"),
                NoticeDates.Read(element, "do"), Iniss2Xml.Text(element, "jd")))
            .ToList();
    }

    /// <summary>
    /// Zapise zaznamy.
    /// </summary>
    /// <param name="groupDir">priecinok grafikonu</param>
    /// <param name="lockouts">zaznamy</param>
    public static void Write(string groupDir, IEnumerable<TrainLockout> lockouts) =>
        Iniss2Xml.Save(Path.Combine(groupDir, FileName), new XElement("VYLUKY", lockouts.Select(lockout => new XElement("v",
            Iniss2Xml.Optional("jd", lockout.Validity), new XAttribute("cv", lockout.Number), new XAttribute("k", lockout.Key),
            NoticeDates.Attribute("od", lockout.From), NoticeDates.Attribute("do", lockout.To)))));
}

/// <summary>
/// Odklony vlakov grafikonu - subor Odklony.xml v priecinku grafikonu. Pre vlak v stanici plati prvy vyhovujuci zaznam.
/// </summary>
public static class OdklonyFile
{
    /// <summary>
    /// Nazov suboru v priecinku grafikonu.
    /// </summary>
    public const string FileName = "Odklony.xml";

    /// <summary>
    /// Nacita zaznamy v poradi suboru. Chybajuci subor je prazdny zoznam.
    /// </summary>
    /// <param name="groupDir">priecinok grafikonu</param>
    public static List<TrainDiversion> Read(string groupDir)
    {
        var file = Path.Combine(groupDir, FileName);
        if (!File.Exists(file))
            return [];

        return Iniss2Xml.Load(file).Root!.Elements("v")
            .Select(element => new TrainDiversion(Iniss2Xml.Text(element, "cv"), Iniss2Xml.Number(element, "s"), Iniss2Xml.Text(element, "k"),
                NoticeDates.Read(element, "od"), NoticeDates.Read(element, "do"), Iniss2Xml.Text(element, "jd")))
            .ToList();
    }

    /// <summary>
    /// Zapise zaznamy. Stanica sa zapisuje vzdy ako u servera, bez nej by odklon neplatil nikde.
    /// </summary>
    /// <param name="groupDir">priecinok grafikonu</param>
    /// <param name="diversions">zaznamy</param>
    public static void Write(string groupDir, IEnumerable<TrainDiversion> diversions) =>
        Iniss2Xml.Save(Path.Combine(groupDir, FileName), new XElement("ODKLONY", diversions.Select(diversion => new XElement("v",
            Iniss2Xml.Optional("jd", diversion.Validity), new XAttribute("cv", diversion.Number), new XAttribute("k", diversion.Key),
            NoticeDates.Attribute("od", diversion.From), NoticeDates.Attribute("do", diversion.To), new XAttribute("s", diversion.Station)))));
}

/// <summary>
/// Hlasenia definovane obsluhou - subor ConstReports.xml v priecinku dat. Je spolocny pre vsetky grafikony.
/// </summary>
public static class ConstReportsFile
{
    /// <summary>
    /// Nazov suboru v priecinku dat.
    /// </summary>
    public const string FileName = "ConstReports.xml";

    /// <summary>
    /// Nacita hlasenia v poradi suboru. Chybajuci subor je prazdny zoznam.
    /// </summary>
    /// <param name="dataDir">priecinok dat servera</param>
    public static List<ConstReport> Read(string dataDir)
    {
        var file = Path.Combine(dataDir, FileName);
        if (!File.Exists(file))
            return [];

        return Iniss2Xml.Load(file).Root!.Elements("r")
            .Select(element => new ConstReport(Iniss2Xml.Number(element, "k"), Iniss2Xml.Text(element, "n"), Iniss2Xml.Text(element, "ik"),
                element.Elements("s").Select(station => int.Parse(station.Value, NumberStyles.Integer, CultureInfo.InvariantCulture)).ToList()))
            .ToList();
    }

    /// <summary>
    /// Zapise hlasenia.
    /// </summary>
    /// <param name="dataDir">priecinok dat servera</param>
    /// <param name="reports">hlasenia s roznymi cislami</param>
    public static void Write(string dataDir, IEnumerable<ConstReport> reports) =>
        Iniss2Xml.Save(Path.Combine(dataDir, FileName), new XElement("CONSTREPORTS", reports.Select(report => new XElement("r",
            new XAttribute("k", report.Key), new XAttribute("n", report.Name), new XAttribute("ik", report.Function),
            report.Stations.Select(station => new XElement("s", station))))));

    /// <summary>
    /// Doplni hlasenia do zoznamu. Hlasenie s rovnakym vzorom a stanicami, ake uz v zozname je, sa nepridava a pouzije sa jeho
    /// cislo. Nove dostanu cisla za najvyssim v zozname ako pri ulozeni zo servera.
    /// </summary>
    /// <param name="existing">hlasenia instalacie</param>
    /// <param name="added">pridavane hlasenia; ich cisla sa nepouziju</param>
    /// <returns>cely zoznam a cislo pre kazde pridavane hlasenie v poradi</returns>
    public static (List<ConstReport> Reports, List<int> Keys) Merge(IReadOnlyList<ConstReport> existing, IReadOnlyList<ConstReport> added)
    {
        var reports = existing.ToList();
        var keys = new List<int>();
        var next = reports.Count == 0 ? 1 : reports.Max(report => report.Key) + 1;

        foreach (var report in added)
        {
            var same = reports.Find(known => known.Function == report.Function && known.Stations.SequenceEqual(report.Stations));
            if (same is null)
            {
                same = report with { Key = next++ };
                reports.Add(same);
            }

            keys.Add(same.Key);
        }

        return (reports, keys);
    }
}

/// <summary>
/// Datumy obdobia vyluk a odklonov v tvare servera.
/// </summary>
internal static class NoticeDates
{
    private const string Format = "yyyy-MM-dd";

    /// <summary>
    /// Datum z atributu; chybajuci je prvy den kalendara ako u servera.
    /// </summary>
    public static DateOnly Read(XElement element, string attribute) =>
        (string?)element.Attribute(attribute) is { Length: > 0 } text
            ? DateOnly.ParseExact(text, Format, CultureInfo.InvariantCulture)
            : DateOnly.MinValue;

    /// <summary>
    /// Atribut s datumom.
    /// </summary>
    public static XAttribute Attribute(string name, DateOnly date) => new(name, date.ToString(Format, CultureInfo.InvariantCulture));
}
