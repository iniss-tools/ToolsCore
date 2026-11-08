namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Zaznam motoroveho osobneho vlaku v stanici.
/// </summary>
/// <param name="Number">cislo vlaku</param>
/// <param name="Validity">datumova poznamka; prazdna = plati vzdy</param>
public sealed record MosTrain(string Number, string Validity);
