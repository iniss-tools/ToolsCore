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

/// <summary>
/// Svetla nastupist - subor Lights.xml v priecinku dat. Server ho cita pri starte (CP1250) a nezapisuje.
/// </summary>
public static class LightsXml
{
    /// <summary>Nazov suboru v priecinku dat.</summary>
    public const string FileName = "Lights.xml";

    /// <summary>Druh ustredne s binarnym protokolom AKSIGNAL (predvoleny).</summary>
    public const string AkSignal = "AKSIGNAL";

    /// <summary>Druh ustredne s textovym protokolom AKSIGNALUH.</summary>
    public const string AkSignalUh = "AKSIGNALUH";

    /// <summary>Nacita svetla v poradi suboru. Chybajuci subor je prazdny zoznam - server potom svetla neriadi.</summary>
    /// <param name="dataDir">priecinok dat servera</param>
    public static List<LightDefinition> Read(string dataDir)
    {
        var file = Path.Combine(dataDir, FileName);
        if (!File.Exists(file))
            return [];

        return Iniss2Xml.Load(file).Root!.Elements("l").Select(element =>
        {
            var type = Iniss2Xml.Text(element, "typ");
            return new LightDefinition(Iniss2Xml.Number(element, "st"), Iniss2Xml.Text(element, "jm"), type.Length > 0 ? type : AkSignal, Iniss2Xml.Text(element, "lin"))
            {
                SubLine = Iniss2Xml.Text(element, "lin_podr"),
                Address = Iniss2Xml.Number(element, "addr", -1),
                BeforeArrival = Iniss2Xml.Number(element, "ppp"),
                BeforeDeparture = Iniss2Xml.Number(element, "ppo"),
                AfterDeparture = Iniss2Xml.Number(element, "pso"),
                AfterArrival = Iniss2Xml.Number(element, "psp"),
                MinOff = Iniss2Xml.Number(element, "zh")
            };
        }).ToList();
    }
}
