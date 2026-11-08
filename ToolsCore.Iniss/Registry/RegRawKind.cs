namespace ToolsCore.Iniss.Registry;

/// <summary>Druh surovej hodnoty v registri.</summary>
public enum RegRawKind
{
    /// <summary>REG_DWORD.</summary>
    Dword,

    /// <summary>REG_SZ.</summary>
    String,

    /// <summary>REG_BINARY.</summary>
    Binary,

    /// <summary>Iny typ (REG_EXPAND_SZ, REG_MULTI_SZ, REG_QWORD…) - INISS ho necita.</summary>
    Other
}
