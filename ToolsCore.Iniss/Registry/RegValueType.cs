namespace ToolsCore.Iniss.Registry;

/// <summary>Typ hodnoty v registri INISSu.</summary>
public enum RegValueType
{
    /// <summary>REG_DWORD - cislo, program rozlisuje viac urovni.</summary>
    Dword,

    /// <summary>REG_DWORD redukovany na nula / nenula.</summary>
    Bool,

    /// <summary>REG_SZ v CP1250.</summary>
    String,

    /// <summary>REG_BINARY - ulozene rozlozenie okna, rucne needitovatelne.</summary>
    Binary,

    /// <summary>REG_DWORD s farbou 0x00BBGGRR; 0xFE000000 = systemova farba.</summary>
    Color
}
