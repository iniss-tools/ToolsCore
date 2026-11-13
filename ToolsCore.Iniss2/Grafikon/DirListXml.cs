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
    /// Nacita zoznam grafikonov tak, ako je v subore: cislo stanice z atributu <c>s</c> (server ho prepise cislom zo SR70.TXT).
    /// Na upravu zoznamu - pohlad servera vracia <see cref="Read" />.
    /// </summary>
    /// <param name="dataDir">priecinok dat servera</param>
    public static List<DirListGroup> Load(string dataDir) => Parse(Iniss2Xml.Load(Path.Combine(dataDir, FileName)).Root!);

    /// <summary>
    /// Zapise zoznam grafikonov. Existujuci subor sa upravi na mieste (komentare a ostatne zaznamy ostanu po znakoch).
    /// </summary>
    /// <param name="dataDir">priecinok dat servera</param>
    /// <param name="groups">grafikony s cislami stanic do atributu <c>s</c></param>
    /// <returns><see langword="false" />, ak subor uz mal tento obsah.</returns>
    public static bool Write(string dataDir, IEnumerable<DirListGroup> groups) =>
        Iniss2Xml.Write(Path.Combine(dataDir, FileName), Build(groups), root => Build(Parse(root)));

    /// <summary>
    /// Doplni stanicu do zoznamu grafikonov. Chybajuci subor alebo grafikon zalozi. Stanicu, ktoru grafikon uz ma, necha tak.
    /// </summary>
    /// <param name="dataDir">priecinok dat servera</param>
    /// <param name="group">nazov priecinka grafikonu</param>
    /// <param name="item">stanica</param>
    public static void AddStation(string dataDir, string group, DirListItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var groups = File.Exists(Path.Combine(dataDir, FileName)) ? Load(dataDir) : [];
        var index = groups.FindIndex(candidate => string.Equals(candidate.Name, group, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
        {
            groups.Add(new DirListGroup(group, []));
            index = groups.Count - 1;
        }

        if (groups[index].Items.Any(existing => string.Equals(existing.Directory, item.Directory, StringComparison.OrdinalIgnoreCase)))
            return;

        groups[index] = groups[index] with { Items = [.. groups[index].Items, item] };
        Write(dataDir, groups);
    }

    private static List<DirListGroup> Parse(XElement root) => root.Elements("g").Select(group => new DirListGroup(Iniss2Xml.Text(group, "n"),
        group.Elements("i").Select(item => new DirListItem(Iniss2Xml.Text(item, "d"), Iniss2Xml.Number(item, "s"), Iniss2Xml.Text(item, "tl"),
            Iniss2Xml.Text(item, "al"), (DirListItemFlags)Iniss2Xml.Flags(item, "f", FlagKeys))).ToList())).ToList();

    private static XElement Build(IEnumerable<DirListGroup> groups) => new("DL", groups.Select(group => new XElement("g",
        new XAttribute("n", group.Name),
        group.Items.Select(item => new XElement("i", new XAttribute("s", item.Station), new XAttribute("d", item.Directory),
            Iniss2Xml.Optional("tl", item.TableLine), Iniss2Xml.Optional("al", item.AudioLine),
            Iniss2Xml.Optional("f", Iniss2Xml.FlagText((int)item.Flags, FlagKeys)))))));

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
