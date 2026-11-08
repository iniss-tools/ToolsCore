namespace ToolsCore.Iniss.Expressions;

/// <summary>
/// Retazec v uvodzovkach (len ako argument funkcie).
/// </summary>
public sealed record ExprStringNode(int Start, int Length, string Value) : ExprNode(Start, Length)
{
    /// <inheritdoc/>
    public override IEnumerable<ExprNode> Children => [];
}
