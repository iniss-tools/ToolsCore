namespace ToolsCore.Iniss.Expressions;

/// <summary>
/// Vyraz v zatvorkach - v strome sa drzi kvoli presnym poziciam a formatovaniu.
/// </summary>
public sealed record ExprParenNode(int Start, int Length, ExprNode Inner) : ExprNode(Start, Length)
{
    /// <inheritdoc/>
    public override IEnumerable<ExprNode> Children => [Inner];
}
