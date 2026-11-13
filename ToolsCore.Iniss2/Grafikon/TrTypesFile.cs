namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Druhy vlakov TrTypes.xml v priecinku dat (server ho len cita, bez neho sa nespusti).
/// </summary>
public static class TrTypesFile
{
    /// <summary>
    /// Nazov suboru v priecinku dat.
    /// </summary>
    public const string FileName = "TrTypes.xml";

    /// <summary>
    /// Kluce druhov vlakov v poradi suboru; chybajuci subor = <see langword="null" />.
    /// </summary>
    /// <param name="dataDir">priecinok dat servera</param>
    public static List<string>? ReadKeys(string dataDir)
    {
        var file = Path.Combine(dataDir, FileName);
        return File.Exists(file) ? [.. Iniss2Xml.Load(file).Root!.Elements("t").Select(element => Iniss2Xml.Text(element, "key"))] : null;
    }
}
