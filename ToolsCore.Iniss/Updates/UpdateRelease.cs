using System.Text.Json.Serialization;

namespace ToolsCore.Iniss.Updates;

/// <summary>Vydanie programu v indexe aktualizacii.</summary>
public sealed record UpdateRelease
{
    /// <summary>Verzia vydania X.Y alebo X.Y.Z.</summary>
    [JsonPropertyName("version")]
    public string Version { get; init; } = "";

    /// <summary>Kedy vydanie vzniklo.</summary>
    [JsonPropertyName("published")]
    public DateTimeOffset? Published { get; init; }

    /// <summary>Stranka vydania na GitHube.</summary>
    [JsonPropertyName("page")]
    public string Page { get; init; } = "";

    /// <summary>Co je nove (Markdown z textu vydania).</summary>
    [JsonPropertyName("notes")]
    public string Notes { get; init; } = "";

    /// <summary>Instalator vydania.</summary>
    [JsonPropertyName("installer")]
    public UpdateInstaller? Installer { get; init; }
}
