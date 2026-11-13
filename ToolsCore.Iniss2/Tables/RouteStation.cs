namespace ToolsCore.Iniss2.Tables;

/// <summary>
/// Stanica trasy v TTTrasy.xml.
/// </summary>
/// <param name="Station">cislo stanice</param>
/// <param name="Selected">stanica je vybrana do textu (<c>f="S"</c>); server priznak pri hladani nepouziva</param>
public sealed record RouteStation(int Station, bool Selected);
