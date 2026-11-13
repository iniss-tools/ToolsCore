namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Radenie vlaku v stanici ako postupnost zvukov.
/// </summary>
/// <param name="Number">cislo vlaku pri odchode zo stanice</param>
/// <param name="EndStation">cislo cielovej stanice; 0 = kazdy ciel</param>
/// <param name="Reports">pismena typov hlasenia ako v <see cref="TrainAddition" /></param>
/// <param name="Validity">datumova poznamka; prazdna = plati vzdy</param>
/// <param name="Sounds">zvuky v tvare <c>jazyk/skupina/zvuk</c> v poradi prehravania</param>
public sealed record TrainComposition(string Number, int EndStation, string Reports, string Validity, IReadOnlyList<string> Sounds)
{
    /// <summary>Obsluha zaznam upravila pre jeden den (atribut <c>de</c>).</summary>
    public bool EditedForDay { get; init; }

    /// <summary>Povod textu radenia; <see langword="null" /> = zaznam ho nema.</summary>
    public CompositionOrigin? Origin { get; init; }
}
