namespace ToolsCore.Iniss.Registry;

/// <summary>Kam sa ma zmena zapisat.</summary>
public enum RegWriteTarget
{
    /// <summary>Do registra - koren (HKCU alebo HKLM) urci editor podla toho, odkial INISS hodnotu cita.</summary>
    Registry,

    /// <summary>Do suboru .INI vedla programu.</summary>
    Ini
}
