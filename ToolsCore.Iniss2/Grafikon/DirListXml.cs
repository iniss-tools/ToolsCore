using System.Globalization;
using System.Xml.Linq;
using ToolsCore.Iniss.Tools;

namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Zoznam grafikonov a ich stanic - subor DirList.xml v priecinku dat.
/// </summary>
public static class DirListXml
{
    /// <summary>
    /// Nazov suboru v priecinku dat.
    /// </summary>
    public const string FileName = "DirList.xml";

    /// <summary>
    /// Subor s cislom stanice v priecinku stanice.
    /// </summary>
    public const string StationNumberFile = "SR70.TXT";

    private static readonly Dictionary<string, int> FlagKeys = new()
    {
        ["ZASTAVKA"] = (int)DirListItemFlags.Stop,
        ["ODBOCKA"] = (int)DirListItemFlags.Junction,
        ["MEZILEHLA"] = (int)(DirListItemFlags.Stop | DirListItemFlags.Junction)
    };

    /// <summary>
    /// Nacita zoznam grafikonov. Cislo stanice berie zo suboru SR70.TXT ako server, atribut <c>s</c> nepouziva.
    /// </summary>
    /// <param name="dataDir">priecinok dat servera</param>
    /// <exception cref="IOException">stanica nema subor SR70.TXT</exception>
    /// <exception cref="FormatException">v subore SR70.TXT nie je platne cislo</exception>
    public static List<DirListGroup> Read(string dataDir)
    {
        var root = Iniss2Xml.Load(Path.Combine(dataDir, FileName)).Root!;

        return root.Elements("g").Select(group =>
        {
            var name = Iniss2Xml.Text(group, "n");
            var items = group.Elements("i").Select(item =>
            {
                var directory = Iniss2Xml.Text(item, "d");
                return new DirListItem(directory, ReadStationNumber(Path.Combine(dataDir, name, directory)), Iniss2Xml.Text(item, "tl"), Iniss2Xml.Text(item, "al"),
                    (DirListItemFlags)Iniss2Xml.Flags(item, "f", FlagKeys));
            }).ToList();
            return new DirListGroup(name, items);
        }).ToList();
    }

    /// <summary>
    /// Doplni stanicu do zoznamu grafikonov. Chybajuci subor alebo grafikon zalozi. Stanicu, ktoru grafikon uz ma, necha tak.
    /// </summary>
    /// <param name="dataDir">priecinok dat servera</param>
    /// <param name="group">nazov priecinka grafikonu</param>
    /// <param name="item">stanica</param>
    public static void AddStation(string dataDir, string group, DirListItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var file = Path.Combine(dataDir, FileName);
        var root = File.Exists(file) ? Iniss2Xml.Load(file).Root! : new XElement("DL");

        var groupElement = root.Elements("g").FirstOrDefault(element => string.Equals(Iniss2Xml.Text(element, "n"), group, StringComparison.OrdinalIgnoreCase));
        if (groupElement is null)
        {
            groupElement = new XElement("g", new XAttribute("n", group));
            root.Add(groupElement);
        }

        if (groupElement.Elements("i").Any(element => string.Equals(Iniss2Xml.Text(element, "d"), item.Directory, StringComparison.OrdinalIgnoreCase)))
            return;

        groupElement.Add(new XElement("i", new XAttribute("s", item.Station), new XAttribute("d", item.Directory), Iniss2Xml.Optional("tl", item.TableLine),
            Iniss2Xml.Optional("al", item.AudioLine), Iniss2Xml.Optional("f", Iniss2Xml.FlagText((int)item.Flags, FlagKeys))));
        Iniss2Xml.Save(file, root);
    }

    /// <summary>
    /// Zapise cislo stanice do jej priecinka.
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    /// <param name="station">cislo stanice</param>
    public static void WriteStationNumber(string stationDir, int station)
    {
        Directory.CreateDirectory(stationDir);
        File.WriteAllText(Path.Combine(stationDir, StationNumberFile), string.Create(CultureInfo.InvariantCulture, $"{station}\r\n"), Encodings.Win1250);
    }

    private static int ReadStationNumber(string stationDir)
    {
        var file = Path.Combine(stationDir, StationNumberFile);

        foreach (var line in File.ReadLines(file, Encodings.Win1250).Select(line => line.Trim()))
        {
            if (line.Length > 0 && char.IsAsciiDigit(line[0]) && int.TryParse(line, NumberStyles.None, CultureInfo.InvariantCulture, out var number) && number > 0)
                return number;
        }

        throw new FormatException(string.Create(CultureInfo.InvariantCulture, $"No valid station number in {file}."));
    }
}
