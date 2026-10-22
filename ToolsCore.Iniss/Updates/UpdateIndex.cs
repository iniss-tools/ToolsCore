using System.Text.Json;
using System.Text.Json.Serialization;

namespace ToolsCore.Iniss.Updates;

/// <summary>
/// Index aktualizacii <c>update.json</c> - workflow vydania (ToolsCore/Installer/Publish-Release.ps1) ho prilozi ku
/// kazdemu vydaniu programu na GitHube a program ho cita z posledneho vydania svojho repozitara. Kluc je nazov programu,
/// subor vydania ma len jeden.
/// </summary>
public sealed record UpdateIndex
{
    /// <summary>Verzia tvaru indexu.</summary>
    [JsonPropertyName("schema")]
    public int Schema { get; init; }

    /// <summary>Posledne vydanie kazdeho programu podla nazvu programu (nazov zostavy).</summary>
    [JsonPropertyName("programs")]
    public IReadOnlyDictionary<string, UpdateRelease> Programs { get; init; } = new Dictionary<string, UpdateRelease>();

    /// <summary>Nacita index z JSON.</summary>
    /// <exception cref="JsonException">JSON nie je index aktualizacii</exception>
    public static UpdateIndex Parse(string json) =>
        JsonSerializer.Deserialize<UpdateIndex>(json) ?? throw new JsonException("The update index is empty.");
}

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
