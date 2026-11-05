namespace ToolsCore.Iniss2;

/// <summary>
/// Nastavenia servera INISS2 zo suboru Config.xml, ktore nastroje potrebuju.
/// </summary>
/// <param name="GtnPort">port TCP, na ktorom server pocuva spravy GTN; 0 = GTN je vypnute</param>
public sealed record Iniss2Config(int GtnPort)
{
    /// <summary>
    /// Nacita nastavenia z priecinka dat. Chybajuci subor znamena predvolene hodnoty, rovnako ako na serveri.
    /// </summary>
    /// <param name="dataDir">priecinok dat servera</param>
    public static Iniss2Config Read(string dataDir)
    {
        var file = Path.Combine(dataDir, Iniss2Installation.ConfigFileName);
        if (!File.Exists(file))
            return new Iniss2Config(0);

        return new Iniss2Config(Iniss2Xml.Number(Iniss2Xml.Load(file).Root!, "GTNPort"));
    }
}
