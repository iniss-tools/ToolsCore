namespace ToolsCore.Iniss.Registry;

/// <summary>Miesto, kde hodnota lezi.</summary>
public enum RegLocation
{
    /// <summary>Subor .INI vedla programu.</summary>
    Ini,

    /// <summary>HKEY_CURRENT_USER\Software\CHAPS\aplikacia.</summary>
    User,

    /// <summary>Kopia HKLM pre pouzivatela (HKCU\Software\Classes\VirtualStore\MACHINE\SOFTWARE\WOW6432Node\CHAPS\…).</summary>
    VirtualStore,

    /// <summary>HKEY_LOCAL_MACHINE\SOFTWARE\(WOW6432Node\)CHAPS\aplikacia.</summary>
    Machine
}
