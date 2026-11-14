namespace ToolsCore.Iniss2.Grafikon.Script;

/// <summary>
/// Krok hlasenia: jeden riadok tela funkcie.
/// </summary>
public abstract record ScriptStep
{
    /// <summary>Komentar na konci riadka bez apostrofu; <see langword="null" /> = bez komentara.</summary>
    public string? Comment { get; init; }
}
