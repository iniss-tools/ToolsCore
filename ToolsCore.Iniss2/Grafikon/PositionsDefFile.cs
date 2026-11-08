using System.Xml.Linq;

namespace ToolsCore.Iniss2.Grafikon;

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
