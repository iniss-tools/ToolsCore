namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Graf stavoveho diagramu.
/// </summary>
/// <param name="Description">popis, objavi sa v chybach prekladu</param>
public sealed record StateGraph(string Description)
{
    /// <summary>Stavy v poradi suboru - vlak postupuje v smere suboru.</summary>
    public IReadOnlyList<StateNode> Nodes { get; init; } = [];
}
