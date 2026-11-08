namespace ToolsCore.Iniss2.Tables;

/// <summary>
/// Nova linka na doplnenie do CommLines.xml.
/// </summary>
/// <param name="Key">kluc linky</param>
/// <param name="Type">sposob spojenia</param>
/// <param name="Protocol">protokol, napriklad ELEN</param>
/// <param name="Address">atribut <c>line</c> - port alebo adresa</param>
public sealed record CommLineDefinition(string Key, CommLineType Type, string Protocol, string Address)
{
    /// <summary>Parametre serioveho portu; prazdne = predvolene protokolu.</summary>
    public string Parameters { get; init; } = "";

    /// <summary>Cakanie na odpoved tabule pri kontrolach v ms; server ho sam nedoplni.</summary>
    public int ShortTimeout { get; init; } = 1000;

    /// <summary>Cakanie na odpoved tabule pri zapise v ms; server ho sam nedoplni.</summary>
    public int LongTimeout { get; init; } = 3000;
}
