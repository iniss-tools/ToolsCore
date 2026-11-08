namespace ToolsCore.Tools;

/// <summary>Vysledok zapisu nastaveni INISSu.</summary>
public enum RegApplyResult
{
    /// <summary>Vsetko zapisane.</summary>
    Ok,

    /// <summary>Pouzivatel odmietol zvysenie prav - nic sa nezapisalo.</summary>
    Cancelled,

    /// <summary>Zapis do HKLM zlyhal - nic sa nezapisalo.</summary>
    Failed
}
