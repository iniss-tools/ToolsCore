namespace ToolsCore.Iniss2.Devices;

/// <summary>
/// Svetlo nastupista z Lights.xml, ktore server rozsvecuje cez ustrednu AK Signal podla vlakov.
/// </summary>
/// <param name="Station">cislo stanice (<c>st</c>)</param>
/// <param name="Name">nazov svetla (<c>jm</c>), v stanici jedinecny</param>
/// <param name="Type">druh ustredne (<c>typ</c>): <see cref="LightsXml.AkSignal" /> alebo <see cref="LightsXml.AkSignalUh" /></param>
/// <param name="Line">kluc linky (<c>lin</c>); prazdny = jedina linka s vhodnym protokolom</param>
public sealed record LightDefinition(int Station, string Name, string Type, string Line)
{
    /// <summary>Nazov linky na podriadenom serveri pri linke MASTER_LIGHT (<c>lin_podr</c>).</summary>
    public string SubLine { get; init; } = "";

    /// <summary>Adresa ustredne 1-255 pri AKSIGNAL (<c>addr</c>); -1 = nezadana.</summary>
    public int Address { get; init; } = -1;

    /// <summary>Minut pred planovanym prichodom (<c>ppp</c>).</summary>
    public int BeforeArrival { get; init; }

    /// <summary>Minut pred planovanym odchodom z vychodzej stanice (<c>ppo</c>).</summary>
    public int BeforeDeparture { get; init; }

    /// <summary>Minut po skutocnom odchode (<c>pso</c>).</summary>
    public int AfterDeparture { get; init; }

    /// <summary>Minut po skutocnom prichode do cielovej stanice (<c>psp</c>).</summary>
    public int AfterArrival { get; init; }

    /// <summary>Kratsie zhasnutie sa nevykona [min] (<c>zh</c>).</summary>
    public int MinOff { get; init; }
}
