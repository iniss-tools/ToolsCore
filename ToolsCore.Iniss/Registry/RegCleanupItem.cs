namespace ToolsCore.Iniss.Registry;

/// <summary>
/// Polozka na vycistenie konfiguracie - jedna hodnota (alebo cela sekcia) na jednom mieste registra.
/// </summary>
/// <param name="Kind">druh</param>
/// <param name="Location">miesto registra</param>
/// <param name="Section">sekcia</param>
/// <param name="Name">hodnota; null pri celej sekcii</param>
/// <param name="Raw">sucasna hodnota</param>
public sealed record RegCleanupItem(RegCleanupKind Kind, RegLocation Location, string Section, string? Name, RegRawValue? Raw)
{
    /// <summary>Nova hodnota (prevedeny typ, premenovana hodnota); null = hodnota sa zmaze.</summary>
    public RegRawValue? Replacement { get; init; }

    /// <summary>Novy nazov pri <see cref="RegCleanupKind.LegacyName" />.</summary>
    public string? NewName { get; init; }

    /// <summary>Hodnota v HKLM pri <see cref="RegCleanupKind.VirtualStoreCopy" />.</summary>
    public RegRawValue? Reference { get; init; }

    /// <summary>Odporucane vycistit (predvolene oznacene).</summary>
    public bool Recommended { get; init; }
}
