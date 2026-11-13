namespace ToolsCore.Iniss2.Tables;

/// <summary>
/// Sada textov podla stanice (TTStanice.xml): udaje tabul, pre ktore plati, a texty pre stanice.
/// </summary>
/// <param name="Name">nazov sady</param>
public sealed record StationTextSet(string Name)
{
    /// <summary>Text je pre tyfloservis (<c>f="FONET"</c>).</summary>
    public bool Fonet { get; init; }

    /// <summary>Udaje tabul, v ktorych sa texty prejavia.</summary>
    public IReadOnlyList<TextTarget> Targets { get; init; } = [];

    /// <summary>Texty pre stanice v poradi suboru.</summary>
    public IReadOnlyList<StationText> Stations { get; init; } = [];
}
