namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Priznaky zastavenia vlaku v stanici.
/// </summary>
[Flags]
public enum GvStopFlags
{
    /// <summary>Vlak v stanici zastavuje.</summary>
    None = 0,

    /// <summary>Vlak stanicou prechadza bez zastavenia (kluc P).</summary>
    Passing = 1,

    /// <summary>Stanica je zaujmova - vlak v nej riadi INISS (kluc Z).</summary>
    Interest = 2,

    /// <summary>Nutny prestup (kluc PR) - kolaj zo spravy GTN sa v takom bode neprenasa medzi prichodom a odchodom.</summary>
    Pr = 4,

    /// <summary>Dopravny bod (kluc DB).</summary>
    OperatingPoint = 8,

    /// <summary>Zastavka na znamenie (kluc X).</summary>
    OnRequest = 0x10
}
