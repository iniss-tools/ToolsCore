namespace ToolsCore.Iniss.Expressions;

/// <summary>
/// Vysledok prekladu a kontroly.
/// </summary>
/// <param name="Parse">Vysledok prekladu.</param>
/// <param name="Diagnostics">Vsetky hlasenia - chyba prekladu alebo semanticke kontroly.</param>
public sealed record ExprValidationResult(ExprParseResult Parse, IReadOnlyList<ExprDiagnostic> Diagnostics)
{
    /// <summary>Koren stromu, ak sa vyraz prelozil.</summary>
    public ExprNode? Root => Parse.Root;

    /// <summary>Ci vyraz INISS prelozi.</summary>
    public bool Compiles => Parse.Success;

    /// <summary>Ci su medzi hlaseniami chyby.</summary>
    public bool HasErrors => Diagnostics.Any(d => d.IsError);

    /// <summary>Ci su medzi hlaseniami varovania.</summary>
    public bool HasWarnings => Diagnostics.Any(d => d.Severity == ExprSeverity.Warning);
}
