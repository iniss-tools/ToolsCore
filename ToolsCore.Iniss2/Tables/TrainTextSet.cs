namespace ToolsCore.Iniss2.Tables;

/// <summary>
/// Sada textov vlakov pre udaje tabul.
/// </summary>
/// <param name="Name">nazov sady</param>
public sealed record TrainTextSet(string Name)
{
    /// <summary>Pismo udaja.</summary>
    public const int NoFont = -1;

    /// <summary>Udaje, v ktorych sa texty prejavia.</summary>
    public IReadOnlyList<TextTarget> Targets { get; init; } = [];

    /// <summary>Texty vlakov.</summary>
    public IReadOnlyList<TrainText> Trains { get; init; } = [];
}
