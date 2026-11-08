namespace ToolsCore.Iniss.StateDgm;

/// <summary>
/// Udalosti ILTISu, na ktore moze automaticka akcia cakat (kluc <c>Wait</c>); hodnoty su konstanty jazyka vyrazov.
/// </summary>
[Flags]
public enum StateDgmWaitEvent : uint
{
    /// <summary>Necaka.</summary>
    None = 0,

    /// <summary>Odchod vlaku (sprava 080-3).</summary>
    Odj = 0x08000000,

    /// <summary>Vchod vlaku do stanice (sprava 080-2).</summary>
    Vj = 0x10000000,

    /// <summary>Zavedenie cisla vlaku - pri ILTISe sa nepouziva.</summary>
    Zcv = 0x20000000,

    /// <summary>Odchodova vlakova cesta (sprava 080-4).</summary>
    Ovc = 0x40000000,

    /// <summary>Vchodova vlakova cesta (sprava 080-4).</summary>
    Vvc = 0x80000000
}
