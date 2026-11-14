namespace ToolsCore.Iniss2.Grafikon.Script;

/// <summary>
/// Vstavane metody servera (trieda <c>LogTextBase</c>), ktore model hlasenia pozna ako krok. Parametre a predvolene
/// hodnoty podla servera; metody, ktore hlasenie riadia (sekcia, sprava klientovi), sem nepatria.
/// </summary>
public static class ScriptBuiltins
{
    private static ScriptParameter P(string name, ScriptParameterKind kind, bool optional = false) => new(name, kind, optional);

    private static readonly ScriptParameter Arrival = P("bPrijezd", ScriptParameterKind.Bool, true);

    /// <summary>Metody v poradi pre obsluhu.</summary>
    public static IReadOnlyList<ScriptBuiltin> All { get; } =
    [
        new("DruhVlaku",
        [
            P("sGroupTypVlaku", ScriptParameterKind.Group), P("iLimitZpozdeni", ScriptParameterKind.Int), Arrival,
            P("bMezinarodni", ScriptParameterKind.Bool, true), P("bPovinneMistenkovy", ScriptParameterKind.Bool, true),
            P("sDruhVlaku", ScriptParameterKind.Text, true)
        ]),
        new("DruhVlaku_BezZpozdeny",
        [
            P("sGroupTypVlaku", ScriptParameterKind.Group), Arrival, P("bMezinarodni", ScriptParameterKind.Bool, true),
            P("bPovinneMistenkovy", ScriptParameterKind.Bool, true), P("sDruhVlaku", ScriptParameterKind.Text, true)
        ]),
        new("CisloVlaku", [P("iCislo", ScriptParameterKind.Int)]),
        new("NazevVlaku", [P("sGroupName", ScriptParameterKind.Group, true), Arrival]),
        new("Dopravce", []),
        new("LinkaIDS", [Arrival]),
        new("LinkaND", [Arrival]),
        new("Hodiny", [P("sGroupName", ScriptParameterKind.Group), P("iTime", ScriptParameterKind.Time)]),
        new("Minuty", [P("sGroupName", ScriptParameterKind.Group), P("iTime", ScriptParameterKind.Time)]),
        new("Zpozdeni", [P("sGroupName", ScriptParameterKind.Group), P("iPocetMinut", ScriptParameterKind.Int)]),
        new("DuvodZpozdeni", [Arrival]),
        new("PoziceNastupiste", [P("sGroupName", ScriptParameterKind.Group), P("oPosition", ScriptParameterKind.Position), P("sNahled", ScriptParameterKind.Text, true)]),
        new("PoziceKolej", [P("sGroupName", ScriptParameterKind.Group), P("oPosition", ScriptParameterKind.Position), P("sNahled", ScriptParameterKind.Text, true)]),
        new("PoziceSektor", [P("sGroupName", ScriptParameterKind.Group), P("oPosition", ScriptParameterKind.Position), P("sNahled", ScriptParameterKind.Text, true)]),
        new("SeznamStanic",
        [
            P("aiStanice", ScriptParameterKind.Stations), P("sGroupNePosledni", ScriptParameterKind.Group), P("sGroupPosledni", ScriptParameterKind.Group),
            P("sGroupJedina", ScriptParameterKind.Group), P("PrefixPosl", ScriptParameterKind.Delegate, true), P("iIndexOd", ScriptParameterKind.Int, true),
            P("iIndexDo", ScriptParameterKind.Int, true)
        ]),
        new("SeznamStanicVYCH", [P("sGroupName", ScriptParameterKind.Group)]),
        new("SeznamStanicCIL", [P("sGroupName", ScriptParameterKind.Group)]),
        new("Razeni", []),
        new("Dodatky", []),
        new("VylukaVlaku", []),
        new("OdklonovaTrasa", []),
        new("Pripoje", [P("Pripoj", ScriptParameterKind.Delegate)]),
        new("Cislo", [P("iCislo", ScriptParameterKind.Int), P("sGroupName", ScriptParameterKind.Group)]),
        new("TextProNahled", [P("sText", ScriptParameterKind.Text)])
    ];

    /// <summary>
    /// Metoda podla nazvu bez ohladu na velkost pismen (ako VB.NET); <see langword="null" />, ak ju model nepozna.
    /// </summary>
    public static ScriptBuiltin? Find(string method)
    {
        ArgumentNullException.ThrowIfNull(method);
        return All.FirstOrDefault(builtin => string.Equals(builtin.Method, method, StringComparison.OrdinalIgnoreCase));
    }
}
