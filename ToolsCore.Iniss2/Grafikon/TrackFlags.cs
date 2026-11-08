namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Priznaky kolaje stanice.
/// </summary>
[Flags]
public enum TrackFlags
{
    /// <summary>Bezna kolaj.</summary>
    None = 0,

    /// <summary>Oddelovac v ponuke, nie kolaj (kluc SEPARATOR alebo OMIT).</summary>
    Separator = 1,

    /// <summary>Kluc ALTERTRAN.</summary>
    AlterTran = 2,

    /// <summary>Bez hlasenia (kluc NOAUDIO).</summary>
    NoAudio = 4,

    /// <summary>Spolocna hodnota, nie skutocna kolaj, napriklad zruseny vlak (kluc COMMON).</summary>
    Common = 8
}
