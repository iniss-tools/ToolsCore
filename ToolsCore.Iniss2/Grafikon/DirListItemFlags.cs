namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Druh stanice v zozname grafikonov.
/// </summary>
[Flags]
public enum DirListItemFlags
{
    /// <summary>Zeleznicna stanica.</summary>
    None = 0,

    /// <summary>Zastavka (kluc ZASTAVKA).</summary>
    Stop = 1,

    /// <summary>Odbocka (kluc ODBOCKA).</summary>
    Junction = 2
}
