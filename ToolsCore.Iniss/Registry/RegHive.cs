namespace ToolsCore.Iniss.Registry;

/// <summary>Vetva registra, z ktorej INISS hodnotu cita.</summary>
public enum RegHive
{
    /// <summary>HKCU, ak tam existuje kluc aplikacie, inak HKLM.</summary>
    User,

    /// <summary>Vzdy HKLM.</summary>
    Machine
}
