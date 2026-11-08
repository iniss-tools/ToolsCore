namespace ToolsCore.Iniss.Expressions;

/// <summary>
/// Unarny operator: <c>~</c>, <c>!</c>/<c>NOT</c>, znamienko <c>+</c>/<c>-</c>, <c>ODD</c>.
/// </summary>
public sealed record ExprUnaryNode(int Start, int Length, ExprTokenKind Operator, ExprNode Operand) : ExprNode(Start, Length)
{
    /// <inheritdoc/>
    public override IEnumerable<ExprNode> Children => [Operand];
}
