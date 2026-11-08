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
