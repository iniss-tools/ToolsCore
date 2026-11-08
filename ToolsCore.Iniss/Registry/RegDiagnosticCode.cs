namespace ToolsCore.Iniss.Registry;

/// <summary>Druh diagnostiky konfiguracie.</summary>
public enum RegDiagnosticCode
{
    /// <summary>Hodnota ma zly typ.</summary>
    WrongType,

    /// <summary>Prazdny text so spatnym zapisom sa nahradi predvolbou.</summary>
    EmptyText,

    /// <summary>Hodnotu v registri prebija .INI.</summary>
    ShadowedByIni,

    /// <summary>Hodnota v HKLM sa nepouzije, sekcia sa cita z HKCU.</summary>
    MachineIgnored,

    /// <summary>Hodnotu v HKLM prebija VirtualStore.</summary>
    ShadowedByVirtualStore,

    /// <summary>Kopia vo VirtualStore sa pri spusteni ako spravca nepouzije.</summary>
    VirtualStoreIgnored,

    /// <summary>Hodnota je pod starym nazvom, INISS ju premenuje.</summary>
    LegacyName,

    /// <summary>Stary nazov vedla noveho sa uz necita.</summary>
    LegacyGhost,

    /// <summary>Verzia INISSu hodnotu necita.</summary>
    NotInVersion,

    /// <summary>Hodnota, ktoru INISS len zapisuje.</summary>
    WriteOnly,

    /// <summary>Pozostatok - nazov necita ziadna verzia.</summary>
    Leftover,

    /// <summary>Neznamy nazov.</summary>
    Unknown,

    /// <summary>Neznama sekcia.</summary>
    UnknownSection,

    /// <summary>Farba inej jazykovej verzie.</summary>
    ColorOtherLanguage,

    /// <summary>Cislovana sekcia v .INI nahradza register.</summary>
    IniSectionReplacesRegistry,

    /// <summary>Sietovy kanal bez cisla linky.</summary>
    NetworkPortWithoutLine,

    /// <summary>Cislo v .INI nie je desiatkove.</summary>
    IniInvalidNumber
}
