using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using ToolsCore.Iniss.Tools;

namespace ToolsCore.Iniss2.Devices;

/// <summary>
/// Zaznam logu svetiel servera INISS2 (<c>LOG\LIGHT&lt;datum&gt;.xml</c>, prvok <c>L</c>).
/// </summary>
/// <param name="At">cas zapisu (cas servera s posunom)</param>
/// <param name="Station">nazov stanice (<c>s</c>) - server pise nazov, cislo len pri stanici bez nazvu</param>
/// <param name="Name">nazov svetla (<c>n</c>)</param>
/// <param name="Event">udalost (<c>u</c>): <see cref="LightLogFile.ChangeForTrain" />, <see cref="LightLogFile.SetMode" />, <see cref="LightLogFile.ChangeState" /></param>
public sealed record LightLogEntry(DateTimeOffset At, string Station, string Name, string Event)
{
    /// <summary>Rucna zmena obsluhou (<c>m</c>).</summary>
    public bool Manual { get; init; }

    /// <summary>Rezim (<c>r</c>): <c>OFF</c>, <c>AUTO</c>, <c>FORCELIGHT</c>; prazdny = nezadany.</summary>
    public string Mode { get; init; } = "";

    /// <summary>Vlak potrebuje svetlo (<c>t</c>).</summary>
    public bool TrainNeedsLight { get; init; }

    /// <summary>Priznaky stavu (<c>st</c>) bez rezimu, napr. <c>LIGHTON</c>, <c>TWILIGHTON</c>, <c>NOTCONNECTED</c>.</summary>
    public IReadOnlyList<string> State { get; init; } = [];

    /// <summary>Kedy sa svetlo najskor rozsvieti pre dalsi vlak (<c>on</c>); null = nezadane.</summary>
    public DateTimeOffset? TurnOn { get; init; }

    /// <summary>Kedy svetlo zhasne po vlaku, pre ktory svieti (<c>off</c>); null = nezadane.</summary>
    public DateTimeOffset? TurnOff { get; init; }

    /// <summary>Cislo vlaku, pre ktory svetlo svieti (<c>dcv</c>); prazdne = ziadny.</summary>
    public string Train { get; init; } = "";
}

/// <summary>
/// Log svetiel servera INISS2: kazdy riadok je jeden prvok <c>L</c> zapisany serializatorom XML (CP1250). Server ho pise
/// pri zmene potreby svietit pre vlak, rucnej zmene rezimu a zmene stavu svetla.
/// </summary>
public static partial class LightLogFile
{
    /// <summary>Zmena potreby svietit pre vlak.</summary>
    public const string ChangeForTrain = "ChangeForTrain";

    /// <summary>Rucna zmena rezimu obsluhou.</summary>
    public const string SetMode = "SetMode";

    /// <summary>Zmena stavu svetla podla ustredne.</summary>
    public const string ChangeState = "ChangeState";

    /// <summary>Subor logu svetiel za den v priecinku logov servera.</summary>
    public static string PathFor(string logDir, DateOnly day) =>
        Path.Combine(logDir, "LIGHT" + day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ".xml");

    /// <summary>Zaznamy suboru v poradi zapisu; chybajuci subor je prazdny zoznam, poskodeny riadok sa preskoci.</summary>
    public static List<LightLogEntry> Read(string file)
    {
        if (!File.Exists(file))
            return [];

        // server do suboru prave pise - citat so zdielanim zapisu
        using var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        using var reader = new StreamReader(stream, Encodings.Win1250);
        var entries = new List<LightLogEntry>();
        while (reader.ReadLine() is { } line)
        {
            if (Parse(line) is { } entry)
                entries.Add(entry);
        }

        return entries;
    }

    /// <summary>Jeden riadok logu; null = prazdny alebo poskodeny.</summary>
    public static LightLogEntry? Parse(string line)
    {
        ArgumentNullException.ThrowIfNull(line);
        var text = line.Trim();
        if (text.Length == 0)
            return null;

        XElement element;
        try
        {
            element = XElement.Parse(text);
        }
        catch (XmlException)
        {
            return null;
        }

        if (element.Name.LocalName != "L" || Time(Iniss2Xml.Text(element, "d"), Iniss2Xml.Text(element, "c")) is not { } at)
            return null;

        return new LightLogEntry(at, Iniss2Xml.Text(element, "s"), Iniss2Xml.Text(element, "n"), Iniss2Xml.Text(element, "u"))
        {
            Manual = Iniss2Xml.Text(element, "m") == "true",
            Mode = Iniss2Xml.Text(element, "r"),
            TrainNeedsLight = Iniss2Xml.Text(element, "t") == "true",
            State = Iniss2Xml.Text(element, "st").Split(' ', StringSplitOptions.RemoveEmptyEntries),
            TurnOn = Moment(Iniss2Xml.Text(element, "on")),
            TurnOff = Moment(Iniss2Xml.Text(element, "off")),
            Train = Iniss2Xml.Text(element, "dcv")
        };
    }

    /// <summary>Datum <c>yyyy-MM-dd</c> a cas <c>HH:mm:ss.fffffff+hh:mm</c>; bez posunu plati miestny cas.</summary>
    private static DateTimeOffset? Time(string date, string time)
    {
        if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var day))
            return null;
        var match = TimePattern().Match(time);
        if (!match.Success)
            return null;

        var local = day.ToDateTime(TimeOnly.MinValue)
            .AddHours(int.Parse(match.Groups["h"].Value, CultureInfo.InvariantCulture))
            .AddMinutes(int.Parse(match.Groups["m"].Value, CultureInfo.InvariantCulture))
            .AddSeconds(double.Parse(match.Groups["s"].Value, CultureInfo.InvariantCulture));
        var offset = match.Groups["z"].Success ? Offset(match.Groups["z"].Value) : TimeZoneInfo.Local.GetUtcOffset(local);
        return new DateTimeOffset(local, offset);
    }

    private static TimeSpan Offset(string zone) => zone == "Z"
        ? TimeSpan.Zero
        : TimeSpan.ParseExact(zone[1..], @"hh\:mm", CultureInfo.InvariantCulture) * (zone[0] == '-' ? -1 : 1);

    /// <summary>Cas <c>yyyy-MM-ddTHH:mm:ss</c> s posunom alebo bez neho (miestny); predvoleny <c>0001-01-01</c> server nepise.</summary>
    private static DateTimeOffset? Moment(string value) =>
        DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var moment) && moment.Year > 1 ? moment : null;

    [GeneratedRegex(@"^(?<h>\d{1,2}):(?<m>\d{2}):(?<s>\d{2}(\.\d+)?)(?<z>Z|[+-]\d{2}:\d{2})?$", RegexOptions.CultureInvariant)]
    private static partial Regex TimePattern();
}
