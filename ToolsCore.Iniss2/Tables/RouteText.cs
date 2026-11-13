namespace ToolsCore.Iniss2.Tables;

/// <summary>
/// Text pre trasu vlaku (TTTrasy.xml): plati, ked sa stanice vlaku od tejto stanice (pri prichode k nej) zhoduju so stanicami
/// trasy poctom aj poradim.
/// </summary>
/// <param name="Text">text, casti oddelene znakom <c>#</c></param>
/// <param name="Font">cislo pisma; <see cref="TrainTextSet.NoFont" /> = pismo predlohy</param>
public sealed record RouteText(string Text, int Font)
{
    /// <summary>Stanice trasy v poradi jazdy.</summary>
    public IReadOnlyList<RouteStation> Stations { get; init; } = [];
}
