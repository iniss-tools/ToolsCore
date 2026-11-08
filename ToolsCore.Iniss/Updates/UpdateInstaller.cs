using System.Text.Json.Serialization;

namespace ToolsCore.Iniss.Updates;

/// <summary>Instalator vydania - subor priamo z vydania na GitHube.</summary>
public sealed record UpdateInstaller
{
    /// <summary>Adresa na stiahnutie.</summary>
    [JsonPropertyName("url")]
    public string Url { get; init; } = "";

    /// <summary>SHA-256 suboru malymi sestnastkovymi cislicami.</summary>
    [JsonPropertyName("sha256")]
    public string Sha256 { get; init; } = "";

    /// <summary>Velkost suboru v bajtoch.</summary>
    [JsonPropertyName("size")]
    public long Size { get; init; }
}
