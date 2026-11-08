namespace ToolsCore.Iniss.Expressions;

/// <summary>
/// Chyba pri vyhodnoteni vyrazu (v INISSe by znamenala pad programu).
/// </summary>
public sealed class ExprEvaluationException(string message, ExprNode node) : Exception(message)
{
    /// <summary>Uzol, pri ktorom chyba nastala.</summary>
    public ExprNode Node { get; } = node;
}
