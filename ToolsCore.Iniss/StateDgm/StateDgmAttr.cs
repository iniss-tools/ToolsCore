namespace ToolsCore.Iniss.StateDgm;

/// <summary>
/// Priznaky stavu (kluc <c>Attr</c>); mena bitov su z komentarov v suboroch INISSu.
/// </summary>
[Flags]
public enum StateDgmAttr
{
    /// <summary>Bez priznakov.</summary>
    None = 0,

    /// <summary><c>SVSA_Stoji</c> - vlak stoji v stanici.</summary>
    Stoji = 0x08,

    /// <summary><c>SVSA_PotvrzenaKolej</c> - kolaj je potvrdena.</summary>
    PotvrzenaKolej = 0x10,

    /// <summary><c>SVSA_Odbaven</c> - vlak je odbaveny.</summary>
    Odbaven = 0x20,

    /// <summary><c>SVSA_Shadow</c> - vlak je „v tieni“, uz sa nezobrazuje.</summary>
    Shadow = 0x40,

    /// <summary><c>SVSA_NyniStoji</c> - vlak prave teraz stoji.</summary>
    NyniStoji = 0x80
}
