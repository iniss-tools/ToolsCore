using System.Xml.Linq;

namespace ToolsCore.Iniss2.Grafikon;

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
