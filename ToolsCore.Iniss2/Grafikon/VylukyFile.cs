using System.Xml.Linq;

namespace ToolsCore.Iniss2.Grafikon;

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
