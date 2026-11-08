namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Dodatok vlaku v stanici.
/// </summary>
/// <param name="Number">cislo vlaku pri odchode zo stanice</param>
/// <param name="Key">nazov hlasenia v LogText alebo cislo zaznamu ConstReports.xml</param>
/// <param name="Reports">pismena typov hlasenia; prazdne = vsetky typy, <c>-</c> = ziadny</param>
/// <param name="Validity">datumova poznamka; prazdna = plati vzdy</param>
public sealed record TrainAddition(string Number, string Key, string Reports, string Validity);
