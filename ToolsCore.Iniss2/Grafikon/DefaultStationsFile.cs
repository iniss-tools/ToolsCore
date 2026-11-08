using System.Globalization;
using System.Xml.Linq;

namespace ToolsCore.Iniss2.Grafikon;

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
