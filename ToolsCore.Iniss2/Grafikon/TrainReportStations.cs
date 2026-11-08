namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Stanice hlasene pri vlaku: zo smeru a do smeru, v kratkom a dlhom hlaseni. Prazdny zoznam znamena, ze server vezme vzor
/// alebo predvolene stanice.
/// </summary>
/// <param name="Number">cislo vlaku pri odchode zo stanice</param>
/// <param name="Validity">datumova poznamka; prazdna = plati vzdy</param>
public sealed record TrainReportStations(string Number, string Validity)
{
    /// <summary>Kratke hlasenie, stanice zo smeru (prvok <c>b</c>).</summary>
    public IReadOnlyList<int> ShortFrom { get; init; } = [];

    /// <summary>Dlhe hlasenie, stanice zo smeru (prvok <c>c</c>).</summary>
    public IReadOnlyList<int> LongFrom { get; init; } = [];

    /// <summary>Kratke hlasenie, stanice do smeru (prvok <c>d</c>).</summary>
    public IReadOnlyList<int> ShortTo { get; init; } = [];

    /// <summary>Dlhe hlasenie, stanice do smeru (prvok <c>e</c>).</summary>
    public IReadOnlyList<int> LongTo { get; init; } = [];
}
