using System.Globalization;
using System.Xml.Linq;

namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Stanice hlasene pri vlaku: zo smeru a do smeru, v kratkom a dlhom hlaseni. Prazdny zoznam znamena, ze server vezme vzor
/// alebo predvolene stanice.
/// </summary>
/// <param name="Number">cislo vlaku pri odchode zo stanice</param>
/// <param name="Validity">datumova poznamka; prazdna = plati vzdy</param>
public sealed record TrainReportStations(string Number, string Validity)
{
    /// <summary>Kratke hlasenie, stanice zo smeru (prvok <c>b</c>).</summary>
    public IReadOnlyList<int> ShortFrom { get; init; } = [];

    /// <summary>Dlhe hlasenie, stanice zo smeru (prvok <c>c</c>).</summary>
    public IReadOnlyList<int> LongFrom { get; init; } = [];

    /// <summary>Kratke hlasenie, stanice do smeru (prvok <c>d</c>).</summary>
    public IReadOnlyList<int> ShortTo { get; init; } = [];

    /// <summary>Dlhe hlasenie, stanice do smeru (prvok <c>e</c>).</summary>
    public IReadOnlyList<int> LongTo { get; init; } = [];
}

/// <summary>
/// Stanica vzoru trasy.
/// </summary>
/// <param name="Station">cislo stanice</param>
/// <param name="Reported">stanica sa povie</param>
public sealed record ReportPatternStop(int Station, bool Reported);

/// <summary>
/// Stanica grafikonu, ktora sa predvolene povie v hlaseni do smeru.
/// </summary>
/// <param name="Station">cislo stanice</param>
/// <param name="Name">nazov pre orientaciu v subore; server ho nepouziva</param>
public sealed record DefaultStation(int Station, string Name)
{
    /// <summary>Stanica sa povie v kratkom hlaseni.</summary>
    public bool Short { get; init; } = true;

    /// <summary>Stanica sa povie v dlhom hlaseni.</summary>
    public bool Long { get; init; } = true;
}

/// <summary>
/// Stanice hlasene pri vlakoch stanice - subor StaHlasVl.xml v priecinku stanice. Pre cislo a den plati prvy zaznam; server
/// subor prepise, ked obsluha zmeni stanice hlasenia vlaku.
/// </summary>
public static class TrainReportStationsFile
{
    /// <summary>
    /// Nazov suboru v priecinku stanice.
    /// </summary>
    public const string FileName = "StaHlasVl.xml";

    /// <summary>
    /// Nacita zaznamy v poradi suboru. Chybajuci subor je prazdny zoznam.
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    public static List<TrainReportStations> Read(string stationDir)
    {
        var file = Path.Combine(stationDir, FileName);
        if (!File.Exists(file))
            return [];

        return Iniss2Xml.Load(file).Root!.Elements("v")
            .Select(element => new TrainReportStations(Iniss2Xml.Text(element, "cv"), Iniss2Xml.Text(element, "jd"))
            {
                ShortFrom = Stations(element, "b"),
                LongFrom = Stations(element, "c"),
                ShortTo = Stations(element, "d"),
                LongTo = Stations(element, "e")
            })
            .ToList();
    }

    /// <summary>
    /// Zapise zaznamy.
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    /// <param name="trains">zaznamy</param>
    public static void Write(string stationDir, IEnumerable<TrainReportStations> trains) =>
        Iniss2Xml.Save(Path.Combine(stationDir, FileName), new XElement("STAHLASVL", trains.Select(train => new XElement("v",
            new XAttribute("cv", train.Number), Iniss2Xml.Optional("jd", train.Validity),
            Elements("b", train.ShortFrom), Elements("c", train.LongFrom), Elements("d", train.ShortTo), Elements("e", train.LongTo)))));

    private static List<int> Stations(XElement element, string name) =>
        [.. element.Elements(name).Select(station => int.Parse(station.Value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture))];

    private static IEnumerable<XElement> Elements(string name, IEnumerable<int> stations) =>
        stations.Select(station => new XElement(name, station.ToString(CultureInfo.InvariantCulture)));
}

/// <summary>
/// Vzory trasy pre stanice hlasenia - StaHlasB.xml (kratke hlasenie) a StaHlasC.xml (dlhe) v priecinku stanice. Vzor plati
/// pre trasu zo smeru (od vychodiskovej po tuto stanicu) alebo do smeru (od tejto stanice po cielovu) s rovnakymi stanicami
/// v rovnakom poradi; pouzije sa prvy vhodny.
/// </summary>
public static class ReportPatternsFile
{
    /// <summary>
    /// Vzory kratkeho hlasenia.
    /// </summary>
    public const string ShortFileName = "StaHlasB.xml";

    /// <summary>
    /// Vzory dlheho hlasenia.
    /// </summary>
    public const string LongFileName = "StaHlasC.xml";

    /// <summary>
    /// Nacita vzory v poradi suboru. Chybajuci subor je prazdny zoznam.
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    /// <param name="fileName"><see cref="ShortFileName" /> alebo <see cref="LongFileName" /></param>
    public static List<IReadOnlyList<ReportPatternStop>> Read(string stationDir, string fileName)
    {
        var file = Path.Combine(stationDir, fileName);
        if (!File.Exists(file))
            return [];

        return Iniss2Xml.Load(file).Root!.Elements("vzor")
            .Select(pattern => (IReadOnlyList<ReportPatternStop>)pattern.Elements("z")
                .Select(stop => new ReportPatternStop(Iniss2Xml.Number(stop, "s"), Iniss2Xml.Text(stop, "h") == "true"))
                .ToList())
            .ToList();
    }

    /// <summary>
    /// Zapise vzory.
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    /// <param name="fileName"><see cref="ShortFileName" /> alebo <see cref="LongFileName" /></param>
    /// <param name="patterns">vzory</param>
    public static void Write(string stationDir, string fileName, IEnumerable<IReadOnlyList<ReportPatternStop>> patterns) =>
        Iniss2Xml.Save(Path.Combine(stationDir, fileName), new XElement("STAHLAS", patterns.Select(pattern => new XElement("vzor",
            pattern.Select(stop => new XElement("z", new XAttribute("s", stop.Station.ToString(CultureInfo.InvariantCulture)),
                stop.Reported ? new XAttribute("h", "true") : null))))));
}

/// <summary>
/// Predvolene stanice hlasenia do smeru - subor Default.Stations.xml v priecinku grafikonu. Server ich pouzije, ked vlak nema
/// zaznam v StaHlasVl.xml ani vzor; stanica, ktora v subore nie je, sa nepovie, cielova vzdy.
/// </summary>
public static class DefaultStationsFile
{
    /// <summary>
    /// Nazov suboru v priecinku grafikonu.
    /// </summary>
    public const string FileName = "Default.Stations.xml";

    /// <summary>
    /// Nacita stanice v poradi suboru. Chybajuci subor je prazdny zoznam.
    /// </summary>
    /// <param name="groupDir">priecinok grafikonu</param>
    public static List<DefaultStation> Read(string groupDir)
    {
        var file = Path.Combine(groupDir, FileName);
        if (!File.Exists(file))
            return [];

        return Iniss2Xml.Load(file).Root!.Elements("s")
            .Select(element => new DefaultStation(Iniss2Xml.Number(element, "st"), Iniss2Xml.Text(element, "n"))
            {
                Short = Iniss2Xml.Text(element, "k") != "false",
                Long = Iniss2Xml.Text(element, "d") != "false"
            })
            .ToList();
    }

    /// <summary>
    /// Zapise stanice; cislo stanice sa nesmie opakovat, server by sa nespustil.
    /// </summary>
    /// <param name="groupDir">priecinok grafikonu</param>
    /// <param name="stations">stanice s roznymi cislami</param>
    public static void Write(string groupDir, IEnumerable<DefaultStation> stations) =>
        Iniss2Xml.Save(Path.Combine(groupDir, FileName), new XElement("STANICE", stations.Select(station => new XElement("s",
            new XAttribute("st", station.Station.ToString(CultureInfo.InvariantCulture)), Iniss2Xml.Optional("n", station.Name),
            station.Short ? null : new XAttribute("k", "false"), station.Long ? null : new XAttribute("d", "false")))));
}
