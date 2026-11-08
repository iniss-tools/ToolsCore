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
