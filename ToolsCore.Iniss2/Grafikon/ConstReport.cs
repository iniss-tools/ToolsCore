namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Hlasenie definovane obsluhou: vzor z LogText so stanicami.
/// </summary>
/// <param name="Key">cislo, na ktore sa odkazuju dodatky, vyluky a odklony</param>
/// <param name="Name">nazov pre obsluhu</param>
/// <param name="Function">nazov vzoru v LogText</param>
/// <param name="Stations">cisla stanic v poradi</param>
public sealed record ConstReport(int Key, string Name, string Function, IReadOnlyList<int> Stations);
