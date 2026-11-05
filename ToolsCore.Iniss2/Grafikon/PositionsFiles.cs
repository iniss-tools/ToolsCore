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
            TrackSound = Iniss2Xml.Text(element, "zvukkolej"),
            PlatformSound = Iniss2Xml.Text(element, "zvuknast"),
            Flags = (TrackFlags)Iniss2Xml.Flags(element, "f", FlagKeys)
        }).ToList();
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
}
