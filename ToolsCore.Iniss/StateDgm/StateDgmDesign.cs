namespace ToolsCore.Iniss.StateDgm;

/// <summary>
/// Vzhlad tlacidla (<c>StateDgmCtrls\CtrlDesign</c>, skupina <c>Design</c>).
/// </summary>
public sealed class StateDgmDesign : StateDgmElement
{
    /// <summary>Kluc vzhladu, na ktory sa odkazuju ovladace.</summary>
    public string Key { get; set; } = "";

    /// <summary>Obrazky tlacidla (<c>posun-normalny,zameranie,stlaceny</c>).</summary>
    public string Bitmaps { get; set; } = "0-0,1,2";

    /// <summary>Predvolene tlacidlo (reaguje na Enter).</summary>
    public bool DefaultPushButton { get; set; }

    /// <summary>Trieda ovladaca.</summary>
    public string Class { get; set; } = StateDgmKeys.ClassDesignBtn;

    /// <summary>Rozlozene obrazky, null pri nespravnom tvare.</summary>
    public StateDgmBitmaps? ParsedBitmaps => StateDgmBitmaps.TryParse(Bitmaps, out var b) ? b : null;

    /// <inheritdoc />
    public override string ToString() => Key;
}
