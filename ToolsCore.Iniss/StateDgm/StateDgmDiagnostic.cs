using ToolsCore.Iniss.Expressions;

namespace ToolsCore.Iniss.StateDgm;

/// <summary>
/// Hlasenie kontroly diagramu.
/// </summary>
public sealed record StateDgmDiagnostic(ExprSeverity Severity, StateDgmDiagnosticCode Code, string Message, StateDgmLocation Location)
{
    /// <summary>Kluc/nazov prvku pre zobrazenie (napr. <c>Výchozí vlak › Vypiš › #GoToPřijíždí</c>).</summary>
    public string Path { get; init; } = "";

    /// <summary>Hlasenie z validatora vyrazov (pozicia vo vyraze), ak ide o vyraz.</summary>
    public ExprDiagnostic? Expr { get; init; }

    /// <summary>Kluc, v ktorom je vyraz (IndCat, AutoCondition, AutoMode…).</summary>
    public string? ExprKey { get; init; }

    /// <summary>Je to chyba (INISS diagram nenacita alebo sa spravi zjavne zle).</summary>
    public bool IsError => Severity == ExprSeverity.Error;

    /// <inheritdoc />
    public override string ToString() => $"{Severity} {Code}: {Path}: {Message}";
}
