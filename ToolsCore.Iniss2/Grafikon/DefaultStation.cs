namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Stanica grafikonu, ktora sa predvolene povie v hlaseni do smeru.
/// </summary>
/// <param name="Station">cislo stanice</param>
/// <param name="Name">nazov pre orientaciu v subore; server ho nepouziva</param>
public sealed record DefaultStation(int Station, string Name)
{
    /// <summary>Stanica sa povie v kratkom hlaseni.</summary>
    public bool Short { get; init; } = true;

    /// <summary>Stanica sa povie v dlhom hlaseni.</summary>
    public bool Long { get; init; } = true;
}
