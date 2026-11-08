namespace ToolsCore.Iniss2.Devices;

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
