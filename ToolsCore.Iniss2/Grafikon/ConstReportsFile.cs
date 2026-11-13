using System.Globalization;
using System.Xml.Linq;

namespace ToolsCore.Iniss2.Grafikon;

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
        return File.Exists(file) ? Parse(Iniss2Xml.Load(file).Root!) : [];
    }

    /// <summary>
    /// Zapise hlasenia.
    /// </summary>
    /// <param name="dataDir">priecinok dat servera</param>
    /// <param name="reports">hlasenia s roznymi cislami</param>
    /// <returns><see langword="false" />, ak subor uz mal tento obsah.</returns>
    public static bool Write(string dataDir, IEnumerable<ConstReport> reports) =>
        Iniss2Xml.Write(Path.Combine(dataDir, FileName), Build(reports), root => Build(Parse(root)));

    private static List<ConstReport> Parse(XElement root) => root.Elements("r")
        .Select(element => new ConstReport(Iniss2Xml.Number(element, "k"), Iniss2Xml.Text(element, "n"), Iniss2Xml.Text(element, "ik"),
            element.Elements("s").Select(station => int.Parse(station.Value, NumberStyles.Integer, CultureInfo.InvariantCulture)).ToList()))
        .ToList();

    private static XElement Build(IEnumerable<ConstReport> reports) => new("CONSTREPORTS", reports.Select(report => new XElement("r",
        new XAttribute("k", report.Key), new XAttribute("n", report.Name), new XAttribute("ik", report.Function),
        report.Stations.Select(station => new XElement("s", station)))));

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
