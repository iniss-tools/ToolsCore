using ToolsCore.Iniss.Expressions;

namespace ToolsCore.Iniss.TabTab;

/// <summary>
/// Vysledok kontroly sekcie.
/// </summary>
/// <param name="Section">Rozobrana sekcia.</param>
/// <param name="Diagnostics">Hlasenia zoradene podla pozicie.</param>
public sealed record TabTabValidationResult(TabTabSection Section, IReadOnlyList<TabTabDiagnostic> Diagnostics)
{
    /// <summary>Pocet chyb.</summary>
    public int ErrorCount => Diagnostics.Count(d => d.Severity == ExprSeverity.Error);

    /// <summary>Pocet varovani.</summary>
    public int WarningCount => Diagnostics.Count(d => d.Severity == ExprSeverity.Warning);
}
