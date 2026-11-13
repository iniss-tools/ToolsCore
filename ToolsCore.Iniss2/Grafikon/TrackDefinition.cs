namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Kolaj alebo nastupiste stanice.
/// </summary>
public sealed record TrackDefinition
{
    /// <summary>Kluc - podla neho sa na kolaj odkazuje vlak aj sprava GTN.</summary>
    public string Key { get; init; } = "";

    /// <summary>Nazov pre obsluhu.</summary>
    public string Name { get; init; } = "";

    /// <summary>Nazov sektora - obsluha ho vidi za nazvom kolaje (<c>s</c>).</summary>
    public string Sector { get; init; } = "";

    /// <summary>Text sektora na tabuliach (<c>tabsektor</c>).</summary>
    public string SectorText { get; init; } = "";

    /// <summary>Zvuk sektora (<c>zvuksektor</c>).</summary>
    public string SectorSound { get; init; } = "";

    /// <summary>Text kolaje na tabuliach.</summary>
    public string TrackText { get; init; } = "";

    /// <summary>Text nastupista na tabuliach.</summary>
    public string PlatformText { get; init; } = "";

    /// <summary>Text nastupista a kolaje na tabuliach.</summary>
    public string PlatformTrackText { get; init; } = "";

    /// <summary>Nahradny text kolaje na tabuliach.</summary>
    public string AltTrackText { get; init; } = "";

    /// <summary>Zvuk kolaje.</summary>
    public string TrackSound { get; init; } = "";

    /// <summary>Zvuk nastupista.</summary>
    public string PlatformSound { get; init; } = "";

    /// <summary>Priznaky.</summary>
    public TrackFlags Flags { get; init; }

    /// <summary>
    /// Skutocna kolaj, na ktoru moze vlak prist.
    /// </summary>
    public bool IsRealTrack => (Flags & (TrackFlags.Separator | TrackFlags.Common)) == 0;
}
