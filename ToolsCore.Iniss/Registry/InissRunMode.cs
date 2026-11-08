namespace ToolsCore.Iniss.Registry;

/// <summary>Ako je INISS spusteny - rozhoduje o virtualizacii registra (VirtualStore).</summary>
public enum InissRunMode
{
    /// <summary>Bez prav spravcu: zapisy do HKLM idu do VirtualStore a pri citani ma VirtualStore prednost.</summary>
    Normal,

    /// <summary>Ako spravca: INISS cita a zapisuje priamo HKLM, VirtualStore nevidi.</summary>
    Elevated
}
