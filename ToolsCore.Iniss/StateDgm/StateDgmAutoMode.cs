namespace ToolsCore.Iniss.StateDgm;

/// <summary>
/// Rezim automatiky stavu (kluc <c>AutoMode</c>).
/// </summary>
public enum StateDgmAutoMode
{
    /// <summary>Rucne - stav sa prepne len tlacidlom.</summary>
    Manual = 0,

    /// <summary>Poloautomat - obsluha prepnutie potvrdzuje.</summary>
    SemiAutomatic = 1,

    /// <summary>Automat - prepne sa bez zasahu.</summary>
    Automatic = 2
}
