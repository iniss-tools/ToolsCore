namespace ToolsCore.Iniss.Expressions;

/// <summary>
/// Hlasenie o vyraze.
/// </summary>
/// <param name="Severity">Zavaznost.</param>
/// <param name="Code">Kod.</param>
/// <param name="Message">Text (slovensky; pri chybach INISSu jeho vlastny text).</param>
/// <param name="Start">Index prveho znaku miesta v texte vyrazu.</param>
/// <param name="Length">Dlzka miesta v znakoch (0 = bodove hlasenie, napr. koniec textu).</param>
public sealed record ExprDiagnostic(ExprSeverity Severity, ExprDiagnosticCode Code, string Message, int Start, int Length)
{
    /// <summary>Index za poslednym znakom miesta.</summary>
    public int End => Start + Length;

    /// <summary>Odporucane riesenie (text pre stlpec „Riešenie“); <see langword="null"/>, ak nie je.</summary>
    public string? Suggestion { get; init; }

    /// <summary>Automaticka oprava textu, ak sa da navrhnut; <see langword="null"/>, ak nie.</summary>
    public TextFix? Fix { get; init; }

    /// <summary>Ci ide o chybu prekladu.</summary>
    public bool IsError => Severity == ExprSeverity.Error;

    /// <inheritdoc/>
    public override string ToString() => $"{Severity} [{Start}+{Length}] {Message}";
}
