namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Radenie vlaku v stanici ako postupnost zvukov.
/// </summary>
/// <param name="Number">cislo vlaku pri odchode zo stanice</param>
/// <param name="EndStation">cislo cielovej stanice; 0 = kazdy ciel</param>
/// <param name="Reports">pismena typov hlasenia ako v <see cref="TrainAddition" /></param>
/// <param name="Validity">datumova poznamka; prazdna = plati vzdy</param>
/// <param name="Sounds">zvuky v tvare <c>jazyk/skupina/zvuk</c> v poradi prehravania</param>
public sealed record TrainComposition(string Number, int EndStation, string Reports, string Validity, IReadOnlyList<string> Sounds);
