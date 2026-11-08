namespace ToolsCore.Iniss2.Devices;

/// <summary>
/// Audio linka servera z Audio.xml - kam ide hlasenie stanice. Obsahuje len to, co potrebuju nastroje (vystup, linka,
/// adresa a kanal ustredne).
/// </summary>
/// <param name="Station">cislo stanice (<c>st</c>); 0 = linka bez stanice</param>
/// <param name="Name">nazov a kluc audio linky (<c>jm</c>)</param>
/// <param name="Output">druh vystupu (<c>out</c>), napr. <see cref="AudioXml.AkSignalUh" /></param>
/// <param name="Port">kluc linky z CommLines.xml (<c>port</c>)</param>
public sealed record AudioLineDefinition(int Station, string Name, string Output, string Port)
{
    /// <summary>Nazov fronty hlaseni (<c>fr</c>); prazdny = linka bez fronty.</summary>
    public string Queue { get; init; } = "";

    /// <summary>Zvukove zariadenie (<c>wave</c>); prazdne = ziadne.</summary>
    public string Wave { get; init; } = "";

    /// <summary>Kanal zariadenia (<c>ch</c>), pri AKSIGNALUH 1 alebo 2; prazdny = predvoleny.</summary>
    public string Channel { get; init; } = "";

    /// <summary>Adresa ustredne alebo cislo pobocky (<c>addr</c>); -1 = nezadana.</summary>
    public int Address { get; init; } = -1;
}

/// <summary>
/// Audio linky servera - subor Audio.xml v priecinku dat. Server ho cita pri starte (CP1250) a nezapisuje.
/// </summary>
public static class AudioXml
{
    /// <summary>Nazov suboru v priecinku dat.</summary>
    public const string FileName = "Audio.xml";

    /// <summary>Vystup zvukovou kartou bez spinania (predvoleny).</summary>
    public const string None = "NONE";

    /// <summary>Vystup zvukovou kartou s ustrednou AK Signal.</summary>
    public const string AkSignal = "AKSIGNAL";

    /// <summary>Vystup na ustrednu AK Signal UH, ktora zvuky hra sama.</summary>
    public const string AkSignalUh = "AKSIGNALUH";

    /// <summary>Nacita audio linky v poradi suboru. Chybajuci subor je prazdny zoznam.</summary>
    /// <param name="dataDir">priecinok dat servera</param>
    public static List<AudioLineDefinition> Read(string dataDir)
    {
        var file = Path.Combine(dataDir, FileName);
        if (!File.Exists(file))
            return [];

        return Iniss2Xml.Load(file).Root!.Elements("line").Select(element =>
        {
            var output = Iniss2Xml.Text(element, "out");
            return new AudioLineDefinition(Iniss2Xml.Number(element, "st"), Iniss2Xml.Text(element, "jm"), output.Length > 0 ? output : None, Iniss2Xml.Text(element, "port"))
            {
                Queue = Iniss2Xml.Text(element, "fr"),
                Wave = Iniss2Xml.Text(element, "wave"),
                Channel = Iniss2Xml.Text(element, "ch"),
                Address = Iniss2Xml.Number(element, "addr", -1)
            };
        }).ToList();
    }
}
