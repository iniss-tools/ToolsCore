using System.Globalization;
using System.Xml.Linq;

namespace ToolsCore.Iniss2.Grafikon;

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
