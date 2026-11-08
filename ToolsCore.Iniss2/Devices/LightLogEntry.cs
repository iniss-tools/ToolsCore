namespace ToolsCore.Iniss2.Devices;

/// <summary>
/// Zaznam logu svetiel servera INISS2 (<c>LOG\LIGHT&lt;datum&gt;.xml</c>, prvok <c>L</c>).
/// </summary>
/// <param name="At">cas zapisu (cas servera s posunom)</param>
/// <param name="Station">nazov stanice (<c>s</c>) - server pise nazov, cislo len pri stanici bez nazvu</param>
/// <param name="Name">nazov svetla (<c>n</c>)</param>
/// <param name="Event">udalost (<c>u</c>): <see cref="LightLogFile.ChangeForTrain" />, <see cref="LightLogFile.SetMode" />, <see cref="LightLogFile.ChangeState" /></param>
public sealed record LightLogEntry(DateTimeOffset At, string Station, string Name, string Event)
{
    /// <summary>Rucna zmena obsluhou (<c>m</c>).</summary>
    public bool Manual { get; init; }

    /// <summary>Rezim (<c>r</c>): <c>OFF</c>, <c>AUTO</c>, <c>FORCELIGHT</c>; prazdny = nezadany.</summary>
    public string Mode { get; init; } = "";

    /// <summary>Vlak potrebuje svetlo (<c>t</c>).</summary>
    public bool TrainNeedsLight { get; init; }

    /// <summary>Priznaky stavu (<c>st</c>) bez rezimu, napr. <c>LIGHTON</c>, <c>TWILIGHTON</c>, <c>NOTCONNECTED</c>.</summary>
    public IReadOnlyList<string> State { get; init; } = [];

    /// <summary>Kedy sa svetlo najskor rozsvieti pre dalsi vlak (<c>on</c>); null = nezadane.</summary>
    public DateTimeOffset? TurnOn { get; init; }

    /// <summary>Kedy svetlo zhasne po vlaku, pre ktory svieti (<c>off</c>); null = nezadane.</summary>
    public DateTimeOffset? TurnOff { get; init; }

    /// <summary>Cislo vlaku, pre ktory svetlo svieti (<c>dcv</c>); prazdne = ziadny.</summary>
    public string Train { get; init; } = "";
}
