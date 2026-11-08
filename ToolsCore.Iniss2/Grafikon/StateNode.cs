namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Stav vlaku.
/// </summary>
/// <param name="Key">kluc stavu</param>
public sealed record StateNode(string Key)
{
    /// <summary>Nazov; prazdny = bez nazvu.</summary>
    public string Name { get; init; } = "";

    /// <summary>Nazov hlasenia z ReportKeys.xml; prazdny = bez hlasenia.</summary>
    public string Report { get; init; } = "";

    /// <summary>Priznaky stavu, pozri <see cref="StateNodeFlags" />.</summary>
    public IReadOnlyList<string> Flags { get; init; } = [];

    /// <summary>Podmienka stavu, vyraz VB.NET typu Boolean; prazdna = plati vzdy.</summary>
    public string Condition { get; init; } = "";

    /// <summary>Tlacidlo; null = stav nema tlacidlo.</summary>
    public StateButton? Button { get; init; }

    /// <summary>Automatika; null = stav nema automatiku.</summary>
    public StateAutomation? Automation { get; init; }
}
