namespace ToolsCore.Iniss.StateDgm;

/// <summary>
/// Starter - opakovane spustanie akcie podla casu (skupina <c>Starter</c>).
/// </summary>
public sealed class StateDgmStarter : StateDgmElement
{
    /// <summary>Kluc startera.</summary>
    public string Key { get; set; } = "";

    /// <summary>Akcia, ktoru spusta.</summary>
    public string EventKey { get; set; } = "";

    /// <summary>Trieda - jedina je <c>SDStarterShape</c>.</summary>
    public string Class { get; set; } = StateDgmKeys.ClassStarter;

    /// <summary>Casovy bod prveho spustenia.</summary>
    public string TimePointKey { get; set; } = "";

    /// <summary>Posun prveho spustenia v sekundach.</summary>
    public int TimeOffset { get; set; }

    /// <summary>Interval opakovania v sekundach; null = bez opakovania.</summary>
    public int? TimeOffsetStep { get; set; }

    /// <summary>Casovy bod posledneho spustenia; null = ako <see cref="TimePointKey" />.</summary>
    public string? TimePointKeyLast { get; set; }

    /// <summary>Posun posledneho spustenia v sekundach.</summary>
    public int? TimeOffsetLast { get; set; }

    /// <summary>Spustit hned, ked prvy termin uz uplynul.</summary>
    public bool StartLaterToo { get; set; }

    /// <inheritdoc />
    public override string ToString() => Key;
}
