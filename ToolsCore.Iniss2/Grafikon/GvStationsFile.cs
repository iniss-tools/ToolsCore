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
        return File.Exists(file) ? Parse(Iniss2Xml.Load(file).Root!) : [];
    }

    /// <summary>
    /// Nazvy stanic ako ich pouzije server: prvy GV.stanice.XML v poradi grafikonov plati cely, dalsie grafikony doplnia len
    /// cisla, ktore v nom chybaju.
    /// </summary>
    /// <param name="dataDir">priecinok dat servera</param>
    /// <param name="groups">grafikony v poradi DirList.xml</param>
    public static Dictionary<int, string> ReadNames(string dataDir, IEnumerable<DirListGroup> groups)
    {
        ArgumentNullException.ThrowIfNull(groups);

        var names = new Dictionary<int, string>();
        foreach (var group in groups)
        {
            foreach (var station in Read(Path.Combine(dataDir, group.Name)))
                names.TryAdd(station.Number, station.Name);
        }

        return names;
    }

    /// <summary>
    /// Zapise stanice. Server cislo stanice v subore nesmie najst dvakrat.
    /// </summary>
    /// <param name="groupDir">priecinok grafikonu</param>
    /// <param name="stations">stanice s roznymi cislami</param>
    /// <returns><see langword="false" />, ak subor uz mal tento obsah.</returns>
    public static bool Write(string groupDir, IEnumerable<GvStation> stations) =>
        Iniss2Xml.Write(Path.Combine(groupDir, FileName), Build(stations), root => Build(Parse(root)));

    private static List<GvStation> Parse(XElement root) =>
        root.Elements("s").Select(element => new GvStation(Iniss2Xml.Number(element, "k"), Iniss2Xml.Text(element, "n"))).ToList();

    private static XElement Build(IEnumerable<GvStation> stations) =>
        new("ST", stations.Select(station => new XElement("s", new XAttribute("k", station.Number), new XAttribute("n", station.Name))));
}
