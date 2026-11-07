using System.Xml.Linq;

namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Priznaky kolaje stanice.
/// </summary>
[Flags]
public enum TrackFlags
{
    /// <summary>Bezna kolaj.</summary>
    None = 0,

    /// <summary>Oddelovac v ponuke, nie kolaj (kluc SEPARATOR alebo OMIT).</summary>
    Separator = 1,

    /// <summary>Kluc ALTERTRAN.</summary>
    AlterTran = 2,

    /// <summary>Bez hlasenia (kluc NOAUDIO).</summary>
    NoAudio = 4,

    /// <summary>Spolocna hodnota, nie skutocna kolaj, napriklad zruseny vlak (kluc COMMON).</summary>
    Common = 8
}

/// <summary>
/// Kolaj alebo nastupiste stanice.
/// </summary>
public sealed record TrackDefinition
{
    /// <summary>Kluc - podla neho sa na kolaj odkazuje vlak aj sprava GTN.</summary>
    public string Key { get; init; } = "";

    /// <summary>Nazov pre obsluhu.</summary>
    public string Name { get; init; } = "";

    /// <summary>Text kolaje na tabuliach.</summary>
    public string TrackText { get; init; } = "";

    /// <summary>Text nastupista na tabuliach.</summary>
    public string PlatformText { get; init; } = "";

    /// <summary>Text nastupista a kolaje na tabuliach.</summary>
    public string PlatformTrackText { get; init; } = "";

    /// <summary>Nahradny text kolaje na tabuliach.</summary>
    public string AltTrackText { get; init; } = "";

    /// <summary>Zvuk kolaje.</summary>
    public string TrackSound { get; init; } = "";

    /// <summary>Zvuk nastupista.</summary>
    public string PlatformSound { get; init; } = "";

    /// <summary>Priznaky.</summary>
    public TrackFlags Flags { get; init; }

    /// <summary>
    /// Skutocna kolaj, na ktoru moze vlak prist.
    /// </summary>
    public bool IsRealTrack => (Flags & (TrackFlags.Separator | TrackFlags.Common)) == 0;
}

/// <summary>
/// Kolaje stanice - subor PositionsDef.xml v priecinku stanice.
/// </summary>
public static class PositionsDefFile
{
    /// <summary>
    /// Nazov suboru v priecinku stanice.
    /// </summary>
    public const string FileName = "PositionsDef.xml";

    private static readonly Dictionary<string, int> FlagKeys = new()
    {
        ["SEPARATOR"] = (int)TrackFlags.Separator,
        ["OMIT"] = (int)TrackFlags.Separator,
        ["ALTERTRAN"] = (int)TrackFlags.AlterTran,
        ["NOAUDIO"] = (int)TrackFlags.NoAudio,
        ["COMMON"] = (int)TrackFlags.Common
    };

    /// <summary>
    /// Nacita kolaje stanice v poradi suboru.
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    public static List<TrackDefinition> Read(string stationDir) =>
        Iniss2Xml.Load(Path.Combine(stationDir, FileName)).Root!.Elements("pos").Select(element => new TrackDefinition
        {
            Key = Iniss2Xml.Text(element, "k"),
            Name = Iniss2Xml.Text(element, "n"),
            TrackText = Iniss2Xml.Text(element, "tabkolej"),
            PlatformText = Iniss2Xml.Text(element, "tabnast"),
            PlatformTrackText = Iniss2Xml.Text(element, "tabnastkolej"),
            AltTrackText = Iniss2Xml.Text(element, "tabalttext"),
            TrackSound = Iniss2Xml.Text(element, "zvukkolej"),
            PlatformSound = Iniss2Xml.Text(element, "zvuknast"),
            Flags = (TrackFlags)Iniss2Xml.Flags(element, "f", FlagKeys)
        }).ToList();

    /// <summary>
    /// Zapise kolaje stanice.
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    /// <param name="tracks">kolaje v poradi, v akom ich vidi obsluha</param>
    public static void Write(string stationDir, IEnumerable<TrackDefinition> tracks) =>
        Iniss2Xml.Save(Path.Combine(stationDir, FileName), new XElement("POZICE", tracks.Select(track => new XElement("pos",
            new XAttribute("k", track.Key),
            new XAttribute("n", track.Name),
            new XAttribute("tabkolej", track.TrackText),
            new XAttribute("tabnast", track.PlatformText),
            Iniss2Xml.Optional("tabnastkolej", track.PlatformTrackText),
            Iniss2Xml.Optional("tabalttext", track.AltTrackText),
            new XAttribute("zvukkolej", track.TrackSound),
            new XAttribute("zvuknast", track.PlatformSound),
            Iniss2Xml.Optional("f", Iniss2Xml.FlagText((int)track.Flags, FlagKeys))))));
}

/// <summary>
/// Kolaj vlaku v stanici.
/// </summary>
/// <param name="Train">cislo vlaku</param>
/// <param name="ArrivalTrack">kluc kolaje pri prichode</param>
/// <param name="DepartureTrack">kluc kolaje pri odchode; bez nej plati kolaj prichodu</param>
/// <param name="Validity">datumova poznamka; prazdna = kazdy den, ked vlak stanicou ide</param>
public sealed record TrainPosition(string Train, string ArrivalTrack, string DepartureTrack, string Validity);

/// <summary>
/// Kolaje vlakov v stanici - subor Positions.xml v priecinku stanice.
/// </summary>
public static class PositionsFile
{
    /// <summary>
    /// Nazov suboru v priecinku stanice.
    /// </summary>
    public const string FileName = "Positions.xml";

    /// <summary>
    /// Nacita kolaje vlakov v poradi suboru - pri hladani plati prvy vyhovujuci zaznam. Chybajuci subor je prazdny zoznam.
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    public static List<TrainPosition> Read(string stationDir)
    {
        var file = Path.Combine(stationDir, FileName);
        if (!File.Exists(file))
            return [];

        return Iniss2Xml.Load(file).Root!.Elements("pos").Select(element =>
        {
            var arrival = Iniss2Xml.Text(element, "p");
            var departure = Iniss2Xml.Text(element, "o");
            return new TrainPosition(Iniss2Xml.Text(element, "cv"), arrival, departure.Length > 0 ? departure : arrival, Iniss2Xml.Text(element, "jd"));
        }).ToList();
    }

    /// <summary>
    /// Zapise kolaje vlakov. Zaznam s poznamkou musi byt pred zaznamom bez poznamky.
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    /// <param name="positions">zaznamy v poradi hladania</param>
    public static void Write(string stationDir, IEnumerable<TrainPosition> positions) =>
        Iniss2Xml.Save(Path.Combine(stationDir, FileName), new XElement("POZICESPOJU", positions.Select(position => new XElement("pos",
            Iniss2Xml.Optional("jd", position.Validity),
            new XAttribute("cv", position.Train),
            Iniss2Xml.Optional("p", position.ArrivalTrack),
            Iniss2Xml.Optional("o", position.DepartureTrack)))));
}
