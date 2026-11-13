using System.Xml.Linq;

namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Kolaje stanice - subor PositionsDef.xml v priecinku stanice. Server ho len cita.
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
    public static List<TrackDefinition> Read(string stationDir) => Parse(Iniss2Xml.Load(Path.Combine(stationDir, FileName)).Root!);

    /// <summary>
    /// Zapise kolaje stanice. Existujuci subor sa upravi na mieste (komentare a ostatne kolaje ostanu po znakoch).
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    /// <param name="tracks">kolaje v poradi pre obsluhu</param>
    /// <returns><see langword="false" />, ak subor uz mal tento obsah.</returns>
    public static bool Write(string stationDir, IEnumerable<TrackDefinition> tracks) =>
        Iniss2Xml.Write(Path.Combine(stationDir, FileName), Build(tracks), root => Build(Parse(root)));

    private static List<TrackDefinition> Parse(XElement root) => root.Elements("pos").Select(element => new TrackDefinition
    {
        Key = Iniss2Xml.Text(element, "k"),
        Name = Iniss2Xml.Text(element, "n"),
        Sector = Iniss2Xml.Text(element, "s"),
        TrackText = Iniss2Xml.Text(element, "tabkolej"),
        SectorText = Iniss2Xml.Text(element, "tabsektor"),
        PlatformText = Iniss2Xml.Text(element, "tabnast"),
        PlatformTrackText = Iniss2Xml.Text(element, "tabnastkolej"),
        AltTrackText = Iniss2Xml.Text(element, "tabalttext"),
        TrackSound = Iniss2Xml.Text(element, "zvukkolej"),
        PlatformSound = Iniss2Xml.Text(element, "zvuknast"),
        SectorSound = Iniss2Xml.Text(element, "zvuksektor"),
        Flags = (TrackFlags)Iniss2Xml.Flags(element, "f", FlagKeys)
    }).ToList();

    // texty a zvuky kolaje a nastupista vzdy (ako export GVDEditora), sektor a ostatne len s hodnotou
    private static XElement Build(IEnumerable<TrackDefinition> tracks) => new("POZICE", tracks.Select(track => new XElement("pos",
        new XAttribute("k", track.Key),
        new XAttribute("n", track.Name),
        Iniss2Xml.Optional("s", track.Sector),
        new XAttribute("tabkolej", track.TrackText),
        Iniss2Xml.Optional("tabsektor", track.SectorText),
        new XAttribute("tabnast", track.PlatformText),
        Iniss2Xml.Optional("tabnastkolej", track.PlatformTrackText),
        Iniss2Xml.Optional("tabalttext", track.AltTrackText),
        new XAttribute("zvukkolej", track.TrackSound),
        new XAttribute("zvuknast", track.PlatformSound),
        Iniss2Xml.Optional("zvuksektor", track.SectorSound),
        Iniss2Xml.Optional("f", Iniss2Xml.FlagText((int)track.Flags, FlagKeys)))));
}
