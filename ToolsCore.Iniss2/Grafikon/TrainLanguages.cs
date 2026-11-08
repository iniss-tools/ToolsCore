namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Jazyky hlasenia vlaku v stanici.
/// </summary>
/// <param name="Number">cislo vlaku pri odchode zo stanice</param>
/// <param name="Validity">datumova poznamka; prazdna = plati vzdy</param>
/// <param name="Languages">kluce jazykov banky; <c>1</c> = vsetky jazyky</param>
public sealed record TrainLanguages(string Number, string Validity, IReadOnlyList<string> Languages);
