namespace ToolsCore.Iniss.Registry;

/// <summary>Odkial pochadza ucinna hodnota.</summary>
public enum RegSource
{
    /// <summary>Subor .INI.</summary>
    Ini,

    /// <summary>HKCU.</summary>
    User,

    /// <summary>VirtualStore.</summary>
    VirtualStore,

    /// <summary>HKLM.</summary>
    Machine,

    /// <summary>Predvolena hodnota podla triedy linky.</summary>
    ClassDefault,

    /// <summary>Predvolena hodnota programu.</summary>
    Default,

    /// <summary>Predvolenu hodnotu urci program za behu a editor ju nepozna.</summary>
    UnknownDefault,

    /// <summary>Tato verzia INISSu hodnotu necita.</summary>
    NotRead
}
