namespace ToolsCore.Iniss2;

/// <summary>
/// Nastavenia servera INISS2 zo suboru Config.xml, ktore nastroje potrebuju.
/// </summary>
/// <param name="GtnPort">port TCP, na ktorom server pocuva spravy GTN; 0 = GTN je vypnute</param>
public sealed record Iniss2Config(int GtnPort)
{
    /// <summary>Predvolena hodnota <see cref="AgvPreTime" />.</summary>
    public const int DefaultAgvPreTime = 720;

    /// <summary>Server po starte nenacita AGV.xml (<c>NoLoadAGV</c>).</summary>
    public bool NoLoadAgv { get; init; }

    /// <summary>Kolko minut dopredu server doplna vlaky; starsi AGV.xml pri starte nenacita (<c>AGVPreTime</c>).</summary>
    public int AgvPreTime { get; init; } = DefaultAgvPreTime;

    /// <summary>Priecinok, z ktoreho server pri starte synchronizuje data PPV (<c>PPVDataDirectory</c>); prazdny = vypnute.</summary>
    public string PpvDataDirectory { get; init; } = "";

    /// <summary>Priecinok servera na zaloznom stroji (<c>DataBackupPath</c>); prazdny = zaloha vypnuta.</summary>
    public string DataBackupPath { get; init; } = "";

    /// <summary>
    /// Nacita nastavenia z priecinka dat. Chybajuci subor znamena predvolene hodnoty, rovnako ako na serveri.
    /// </summary>
    /// <param name="dataDir">priecinok dat servera</param>
    public static Iniss2Config Read(string dataDir)
    {
        var file = Path.Combine(dataDir, Iniss2Installation.ConfigFileName);
        if (!File.Exists(file))
            return new Iniss2Config(0);

        var root = Iniss2Xml.Load(file).Root!;
        return new Iniss2Config(Iniss2Xml.Number(root, "GTNPort"))
        {
            NoLoadAgv = Iniss2Xml.Bool(root, "NoLoadAGV"),
            AgvPreTime = Iniss2Xml.Number(root, "AGVPreTime", DefaultAgvPreTime),
            PpvDataDirectory = Iniss2Xml.Text(root, "PPVDataDirectory"),
            DataBackupPath = Iniss2Xml.Text(root, "DataBackupPath")
        };
    }
}
