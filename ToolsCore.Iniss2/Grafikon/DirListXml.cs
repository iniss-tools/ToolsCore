using System.Globalization;
using ToolsCore.Iniss.Tools;

namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Druh stanice v zozname grafikonov.
/// </summary>
[Flags]
public enum DirListItemFlags
{
    /// <summary>Zeleznicna stanica.</summary>
    None = 0,

    /// <summary>Zastavka (kluc ZASTAVKA).</summary>
    Stop = 1,

    /// <summary>Odbocka (kluc ODBOCKA).</summary>
    Junction = 2
}

/// <summary>
/// Stanica grafikonu.
/// </summary>
/// <param name="Directory">priecinok stanice v priecinku grafikonu</param>
/// <param name="Station">cislo stanice SR70</param>
/// <param name="TableLine">linka tabul; prazdna = urcuje ju tabula</param>
/// <param name="AudioLine">linka zvuku</param>
/// <param name="Flags">druh stanice</param>
public sealed record DirListItem(string Directory, int Station, string TableLine, string AudioLine, DirListItemFlags Flags);

/// <summary>
/// Grafikon - priecinok v datach servera so spolocnym GV.XML a priecinkami stanic.
/// </summary>
/// <param name="Name">nazov priecinka grafikonu</param>
/// <param name="Items">stanice grafikonu</param>
public sealed record DirListGroup(string Name, IReadOnlyList<DirListItem> Items);

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
    /// Nacita zoznam grafikonov. Cislo stanice berie zo suboru SR70.TXT ako server, atribut <c>s</c> len ked subor chyba.
    /// </summary>
    /// <param name="dataDir">priecinok dat servera</param>
    public static List<DirListGroup> Read(string dataDir)
    {
        var root = Iniss2Xml.Load(Path.Combine(dataDir, FileName)).Root!;

        return root.Elements("g").Select(group =>
        {
            var name = Iniss2Xml.Text(group, "n");
            var items = group.Elements("i").Select(item =>
            {
                var directory = Iniss2Xml.Text(item, "d");
                var station = ReadStationNumber(Path.Combine(dataDir, name, directory)) ?? Iniss2Xml.Number(item, "s");
                return new DirListItem(directory, station, Iniss2Xml.Text(item, "tl"), Iniss2Xml.Text(item, "al"),
                    (DirListItemFlags)Iniss2Xml.Flags(item, "f", FlagKeys));
            }).ToList();
            return new DirListGroup(name, items);
        }).ToList();
    }

    private static int? ReadStationNumber(string stationDir)
    {
        var file = Path.Combine(stationDir, StationNumberFile);
        if (!File.Exists(file))
            return null;

        foreach (var line in File.ReadLines(file, Encodings.Win1250).Select(line => line.Trim()))
        {
            if (line.Length > 0 && char.IsAsciiDigit(line[0]) && int.TryParse(line, NumberStyles.None, CultureInfo.InvariantCulture, out var number) && number > 0)
                return number;
        }

        return null;
    }
}
