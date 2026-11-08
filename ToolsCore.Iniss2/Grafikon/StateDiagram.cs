namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Stavovy diagram stanice - subor statedgm.xml.
/// </summary>
/// <param name="GraphIndex">vyraz VB.NET typu Integer, ktory vrati cislo grafu od 1</param>
public sealed record StateDiagram(string GraphIndex)
{
    /// <summary>Grafy v poradi suboru.</summary>
    public IReadOnlyList<StateGraph> Graphs { get; init; } = [];
}
