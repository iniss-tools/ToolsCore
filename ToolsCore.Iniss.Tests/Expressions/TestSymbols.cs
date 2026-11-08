using ToolsCore.Iniss.Expressions;

namespace ToolsCore.Tests.Expressions;

internal sealed class TestSymbols : IExprSymbolProvider
{
    public IReadOnlyDictionary<string, int>? TrainTypeKeys { get; init; }
    public HashSet<int>? Stations { get; init; }
    public HashSet<string>? Tracks { get; init; }
    public HashSet<string>? Operators { get; init; }

    public bool? StationExists(int id) => Stations?.Contains(id);
    public bool? TrackExists(string name) => Tracks?.Contains(name);
    public bool? OperatorExists(string name) => Operators?.Contains(name);
}
