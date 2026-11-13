namespace ToolsCore.Iniss2.Tables;

/// <summary>
/// Sada textov podla trasy (TTTrasy.xml): udaje tabul, pre ktore plati, a texty pre trasy vlaku.
/// </summary>
/// <param name="Name">nazov sady</param>
public sealed record RouteTextSet(string Name)
{
    /// <summary>Text je pre tyfloservis (<c>f="FONET"</c>).</summary>
    public bool Fonet { get; init; }

    /// <summary>Udaje tabul, v ktorych sa texty prejavia.</summary>
    public IReadOnlyList<TextTarget> Targets { get; init; } = [];

    /// <summary>Trasy v poradi suboru - plati prva, ktorej stanice sa zhoduju s trasou vlaku.</summary>
    public IReadOnlyList<RouteText> Routes { get; init; } = [];
}
