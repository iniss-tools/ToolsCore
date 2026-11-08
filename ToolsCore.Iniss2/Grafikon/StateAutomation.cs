namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Automatika stavu - kedy vlak do stavu prejde sam.
/// </summary>
/// <param name="Time">cas spustenia, vyraz VB.NET typu DateTime</param>
public sealed record StateAutomation(string Time)
{
    /// <summary>Dodatocna podmienka, vyraz typu Boolean; prazdna = plati vzdy.</summary>
    public string Condition { get; init; } = "";

    /// <summary>Rezim 0 rucne, 1 poloautomat, 2 automat - cislo alebo vyraz typu Byte; prazdny = 0.</summary>
    public string Mode { get; init; } = "";

    /// <summary>Interval opakovania v sekundach, cislo alebo vyraz; prazdny = bez opakovania.</summary>
    public string Repeat { get; init; } = "";

    /// <summary>Koncovy cas opakovania, vyraz typu DateTime; prazdny = bez konca.</summary>
    public string End { get; init; } = "";

    /// <summary>Variant hlasenia 1 kratke, 2 dlhe; null = server zvoli sam.</summary>
    public int? Variant { get; init; }

    /// <summary>Priznaky automatiky, pozri <see cref="StateNodeFlags.KeepWithoutClient" />.</summary>
    public IReadOnlyList<string> Flags { get; init; } = [];
}
