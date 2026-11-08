namespace ToolsCore.Iniss.Expressions;

/// <summary>
/// Podmieneny operator <c>p ? a : b</c>.
/// </summary>
public sealed record ExprConditionalNode(int Start, int Length, ExprNode Condition, ExprNode WhenTrue, ExprNode WhenFalse)
    : ExprNode(Start, Length)
{
    /// <inheritdoc/>
    public override IEnumerable<ExprNode> Children => [Condition, WhenTrue, WhenFalse];
}
