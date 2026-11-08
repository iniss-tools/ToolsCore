namespace ToolsCore.Iniss.StateDgm;

/// <summary>
/// Tlacidlo v paneli stavu (skupina <c>Control</c>).
/// </summary>
public sealed class StateDgmControl : StateDgmElement
{
    /// <summary>Poradie tlacidla v paneli od 0.</summary>
    public int CtrlId { get; set; }

    /// <summary>Vzhlad z bloku CtrlDesign.</summary>
    public string DesignKey { get; set; } = "";

    /// <summary>Akcia po stlaceni; prazdne = len obrazok.</summary>
    public string EventKey { get; set; } = "";

    /// <inheritdoc />
    public override string ToString() => $"{CtrlId}: {DesignKey} → {EventKey}";
}
