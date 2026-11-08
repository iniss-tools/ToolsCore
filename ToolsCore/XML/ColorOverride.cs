namespace ToolsCore.XML;

/// <summary>
/// Zmena polozky schemy voci jej zakladu; <see langword="null" /> = hodnota zo zakladu.
/// </summary>
public sealed record ColorOverride(Color? Fore = null, Color? Back = null, bool? Bold = null)
{
    /// <summary>
    /// Ziadna zmena.
    /// </summary>
    public bool IsEmpty => Fore is null && Back is null && Bold is null;
}
