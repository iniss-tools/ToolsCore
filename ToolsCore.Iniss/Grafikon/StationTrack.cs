namespace ToolsCore.Iniss.Grafikon;

/// <summary>
/// Kolaj stanice z <c>Pozice_A.TXT</c> - co z nej potrebuju tabule: text kolaje, nastupiste a logicke tabule nad nou.
/// </summary>
/// <param name="Key">kluc kolaje (1. pole)</param>
/// <param name="Text">text kolaje pre tabule (5. pole); prazdny = kluc</param>
/// <param name="Platform">kluc nastupista (6. pole)</param>
/// <param name="Tables">kluce logickych tabul nad kolajou</param>
public sealed record StationTrack(string Key, string Text, string Platform, IReadOnlyList<string> Tables);
