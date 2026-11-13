namespace ToolsCore.Iniss2.Tables;

/// <summary>
/// Sada textov vlakov pre udaje tabul.
/// </summary>
/// <param name="Name">nazov sady</param>
public sealed record TrainTextSet(string Name)
{
    /// <summary>Pismo udaja.</summary>
    public const int NoFont = -1;

    /// <summary>Priznak <c>FONET</c> (atribut <c>f</c>); vyznam nie je znamy, server aj klient ho len prenasaju.</summary>
    public bool Fonet { get; init; }

    /// <summary>Udaje, v ktorych sa texty prejavia.</summary>
    public IReadOnlyList<TextTarget> Targets { get; init; } = [];

    /// <summary>Texty vlakov.</summary>
    public IReadOnlyList<TrainText> Trains { get; init; } = [];
}
