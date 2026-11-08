namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Hlasenie stanice s pismenom, podla ktoreho sa vyberaju dodatky a radenie.
/// </summary>
/// <param name="Name">nazov hlasenia ako v LogText a statedgm.xml</param>
/// <param name="Key">pismeno; dlhe hlasenie velke, kratke male</param>
/// <param name="Categories">kategorie vlakov <c>V</c>, <c>P</c>, <c>K</c> oddelene medzerou</param>
public sealed record ReportKey(string Name, string Key, string Categories);
