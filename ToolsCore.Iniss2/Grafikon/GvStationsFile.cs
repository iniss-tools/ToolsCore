using System.Xml.Linq;

namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Cisla a nazvy stanic - subor GV.stanice.XML v priecinku grafikonu.
/// </summary>
public static class GvStationsFile
{
    /// <summary>
    /// Nazov suboru v priecinku grafikonu.
    /// </summary>
    public const string FileName = "GV.stanice.XML";

    /// <summary>
    /// Nacita stanice v poradi suboru. Chybajuci subor je prazdny zoznam, server staci jeden subor na vsetky grafikony.
    /// </summary>
    /// <param name="groupDir">priecinok grafikonu</param>
    public static List<GvStation> Read(string groupDir)
    {
        var file = Path.Combine(groupDir, FileName);
        if (!File.Exists(file))
            return [];

        return Iniss2Xml.Load(file).Root!.Elements("s")
            .Select(element => new GvStation(Iniss2Xml.Number(element, "k"), Iniss2Xml.Text(element, "n")))
            .ToList();
    }

    /// <summary>
    /// Zapise stanice. Server cislo stanice v subore nesmie najst dvakrat.
    /// </summary>
    /// <param name="groupDir">priecinok grafikonu</param>
    /// <param name="stations">stanice s roznymi cislami</param>
    public static void Write(string groupDir, IEnumerable<GvStation> stations) =>
        Iniss2Xml.Save(Path.Combine(groupDir, FileName),
            new XElement("ST", stations.Select(station => new XElement("s", new XAttribute("k", station.Number), new XAttribute("n", station.Name)))));
}
