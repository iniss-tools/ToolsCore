namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Druh vlaku z TrTypes.xml.
/// </summary>
/// <param name="Key">kluc druhu (druh vlaku v GV.XML)</param>
/// <param name="Tooltip">popis pre obsluhu</param>
/// <param name="Sound">posledna cast kluca zvuku v hlaseni</param>
/// <param name="Table">text druhu na tabuli</param>
public sealed record TrType(string Key, string Tooltip, string Sound, string Table);
