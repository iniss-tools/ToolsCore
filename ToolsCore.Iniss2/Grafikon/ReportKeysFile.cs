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
