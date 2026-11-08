namespace ToolsCore.Iniss2.Devices;

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
