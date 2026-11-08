using ToolsCore.Iniss.Expressions;

namespace ToolsCore.Iniss.TabTab;

/// <summary>
/// Hlasenie kontroly sekcie TabTab; pozicie su v texte sekcie.
/// </summary>
/// <param name="Severity">Zavaznost.</param>
/// <param name="Code">Kod.</param>
/// <param name="Message">Text.</param>
/// <param name="Start">Index prveho znaku.</param>
/// <param name="Length">Dlzka (0 = bodove).</param>
/// <param name="LineIndex">Cislo fyzickeho riadka (od 0), na ktorom hlasenie zacina.</param>
public sealed record TabTabDiagnostic(ExprSeverity Severity, TabTabDiagnosticCode Code, string Message, int Start, int Length, int LineIndex)
{
    /// <summary>Kod z kontroly vyrazu pri <see cref="TabTabDiagnosticCode.Condition"/> a <see cref="TabTabDiagnosticCode.InissConditionError"/>.</summary>
    public ExprDiagnosticCode? ExprCode { get; init; }

    /// <summary>Odporucane riesenie (text pre stlpec „Riešenie“); <see langword="null"/>, ak nie je.</summary>
    public string? Suggestion { get; init; }

    /// <summary>Automaticka oprava textu sekcie, ak sa da navrhnut; pozicie su v texte sekcie.</summary>
    public TextFix? Fix { get; init; }

    /// <summary>Kod ako text pre zobrazenie (pri hlaseni z vyrazu jeho kod).</summary>
    public string CodeName => ExprCode?.ToString() ?? Code.ToString();

    /// <summary>Index za poslednym znakom.</summary>
    public int End => Start + Length;

    /// <summary>Ci ide o chybu (INISS pravidlo odmietne alebo zapise do logu).</summary>
    public bool IsError => Severity == ExprSeverity.Error;

    /// <inheritdoc/>
    public override string ToString() => $"{Severity} r.{LineIndex + 1} [{Start}+{Length}] {Message}";
}
