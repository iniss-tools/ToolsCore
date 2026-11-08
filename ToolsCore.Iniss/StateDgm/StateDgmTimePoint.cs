namespace ToolsCore.Iniss.StateDgm;

/// <summary>
/// Vlastny casovy bod odvodeny z dvoch inych (<c>TimePoint</c> v hlavicke).
/// </summary>
public sealed class StateDgmTimePoint : StateDgmElement
{
    /// <summary>Jedinecny kluc.</summary>
    public string Key { get; set; } = "";

    /// <summary>Nazov pre obsluhu.</summary>
    public string Name { get; set; } = "";

    /// <summary>Kluc prveho zdrojoveho casoveho bodu.</summary>
    public string TimePointKey1 { get; set; } = "";

    /// <summary>Kluc druheho zdrojoveho casoveho bodu.</summary>
    public string TimePointKey2 { get; set; } = "";

    /// <summary>Posun prveho casu v sekundach.</summary>
    public int Offset1 { get; set; }

    /// <summary>Posun druheho casu v sekundach.</summary>
    public int Offset2 { get; set; }

    /// <summary><c>min</c> alebo <c>max</c>.</summary>
    public string Operator { get; set; } = StateDgmKeys.OperatorMin;

    /// <inheritdoc />
    public override string ToString() => Key;
}
