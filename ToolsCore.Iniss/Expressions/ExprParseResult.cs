namespace ToolsCore.Iniss.Expressions;

/// <summary>
/// Vysledok prekladu vyrazu.
/// </summary>
/// <param name="Text">Prekladany text.</param>
/// <param name="Root">Koren stromu; <see langword="null"/> pri chybe prekladu.</param>
/// <param name="Tokens">Tokeny v poradi (pri chybe po chybny token).</param>
/// <param name="Diagnostics">Hlasenia - pri chybe prekladu prave jedna chyba (INISS hlasi len prvu).</param>
public sealed record ExprParseResult(string Text, ExprNode? Root, IReadOnlyList<ExprToken> Tokens, IReadOnlyList<ExprDiagnostic> Diagnostics)
{
    /// <summary>Ci sa vyraz prelozil.</summary>
    public bool Success => Root is not null;

    /// <summary>Chyba prekladu, ak nastala.</summary>
    public ExprDiagnostic? Error => Diagnostics.FirstOrDefault(d => d.IsError);
}
