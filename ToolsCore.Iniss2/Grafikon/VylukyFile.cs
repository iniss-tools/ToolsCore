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
        return File.Exists(file) ? Parse(Iniss2Xml.Load(file).Root!) : [];
    }

    /// <summary>
    /// Zapise zaznamy. Existujuci subor sa upravi na mieste.
    /// </summary>
    /// <param name="groupDir">priecinok grafikonu</param>
    /// <param name="lockouts">zaznamy</param>
    /// <returns><see langword="false" />, ak subor uz mal tento obsah.</returns>
    public static bool Write(string groupDir, IEnumerable<TrainLockout> lockouts) =>
        Iniss2Xml.Write(Path.Combine(groupDir, FileName), Build(lockouts), root => Build(Parse(root)));

    private static List<TrainLockout> Parse(XElement root) => root.Elements("v")
        .Select(element => new TrainLockout(Iniss2Xml.Text(element, "cv"), Iniss2Xml.Text(element, "k"), NoticeDates.Read(element, "od"),
            NoticeDates.Read(element, "do"), Iniss2Xml.Text(element, "jd"))
        {
            EditedForDay = Iniss2Xml.Bool(element, "de")
        })
        .ToList();

    // poradie atributov ako v XmlSerializer servera
    private static XElement Build(IEnumerable<TrainLockout> lockouts) => new("VYLUKY", lockouts.Select(lockout => new XElement("v",
        Iniss2Xml.Optional("jd", lockout.Validity), new XAttribute("cv", lockout.Number), Iniss2Xml.Optional("de", lockout.EditedForDay),
        new XAttribute("k", lockout.Key), NoticeDates.Attribute("od", lockout.From), NoticeDates.Attribute("do", lockout.To))));
}
