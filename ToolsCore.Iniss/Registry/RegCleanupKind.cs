namespace ToolsCore.Iniss.Registry;

/// <summary>Druh polozky na vycistenie konfiguracie.</summary>
public enum RegCleanupKind
{
    /// <summary>Pozostatok - nazov, ktory necita ziadna verzia INISSu.</summary>
    Leftover,

    /// <summary>Nazov, ktory INISS vobec nepozna (preklep, ina aplikacia).</summary>
    Unknown,

    /// <summary>Nazov farby inej jazykovej verzie INISSu.</summary>
    OtherLanguageColor,

    /// <summary>Sekcia, ktoru INISS nepozna.</summary>
    UnknownSection,

    /// <summary>Stary nazov bez jednotky vedla noveho - INISS ho uz necita.</summary>
    LegacyGhost,

    /// <summary>Hodnota len pod starym nazvom - INISS ju cita a pri zapise premenuje; da sa premenovat hned.</summary>
    LegacyName,

    /// <summary>Hodnota zleho typu - INISS ju ignoruje a pouzije predvolenu.</summary>
    WrongType,

    /// <summary>Kopia vo VirtualStore, ktora INISSu bez prav spravcu prebija odlisnu hodnotu v HKLM.</summary>
    VirtualStoreCopy
}
