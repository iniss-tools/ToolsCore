using System.Xml.Linq;

namespace ToolsCore.Iniss2.Grafikon;

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
        return File.Exists(file) ? Parse(Iniss2Xml.Load(file).Root!) : [];
    }

    /// <summary>
    /// Zapise hlasenia.
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    /// <param name="reports">hlasenia s roznymi nazvami</param>
    /// <returns><see langword="false" />, ak subor uz mal tento obsah.</returns>
    public static bool Write(string stationDir, IEnumerable<ReportKey> reports) =>
        Iniss2Xml.Write(Path.Combine(stationDir, FileName), Build(reports), root => Build(Parse(root)));

    private static List<ReportKey> Parse(XElement root) => root.Elements("r")
        .Select(element => new ReportKey(Iniss2Xml.Text(element, "n"), Iniss2Xml.Text(element, "k"), Iniss2Xml.Text(element, "v")))
        .ToList();

    private static XElement Build(IEnumerable<ReportKey> reports) => new("ReportKeys", reports.Select(report => new XElement("r",
        new XAttribute("n", report.Name), new XAttribute("k", report.Key), new XAttribute("v", report.Categories))));
}
