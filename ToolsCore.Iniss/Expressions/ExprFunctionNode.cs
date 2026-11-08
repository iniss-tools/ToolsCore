namespace ToolsCore.Iniss.Expressions;

/// <summary>
/// Volanie funkcie; <see cref="Argument"/> je <see langword="null"/> pri funkcii bez argumentu.
/// </summary>
public sealed record ExprFunctionNode(int Start, int Length, ExprFunctionInfo Function, ExprNode? Argument, ExprToken Token)
    : ExprNode(Start, Length)
{
    /// <inheritdoc/>
    public override IEnumerable<ExprNode> Children => Argument is null ? [] : [Argument];

    /// <summary>Kanonicka funkcia (anglicky alias prevedeny na cesku podobu).</summary>
    public ExprFunction Canonical => Function.Canonical;
}
