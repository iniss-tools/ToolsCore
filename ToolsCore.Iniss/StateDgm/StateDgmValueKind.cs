namespace ToolsCore.Iniss.StateDgm;

/// <summary>
/// Druh hodnoty v subore StateDgm.txt.
/// </summary>
public enum StateDgmValueKind
{
    /// <summary><c>S:"kluc"="text"</c></summary>
    String,

    /// <summary><c>I:"kluc"=cislo</c> (strtol so zakladom 0 - <c>0x08</c> aj <c>010</c>)</summary>
    Int,

    /// <summary><c>B:"kluc"=Ano|Ne|Yes|No</c></summary>
    Bool
}
