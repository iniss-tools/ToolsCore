namespace ToolsCore.Iniss.Expressions;

/// <summary>
/// Binarny operator.
/// </summary>
public sealed record ExprBinaryNode(int Start, int Length, ExprTokenKind Operator, ExprNode Left, ExprNode Right, int OperatorStart)
    : ExprNode(Start, Length)
{
    /// <inheritdoc/>
    public override IEnumerable<ExprNode> Children => [Left, Right];

    /// <summary>Ci je operator logicka alebo bitova spojka (jedna uroven priority v INISSe).</summary>
    public bool IsConnective => Operator is ExprTokenKind.BitAnd or ExprTokenKind.BitOr or ExprTokenKind.BitXor
        or ExprTokenKind.And or ExprTokenKind.Or or ExprTokenKind.AndWord or ExprTokenKind.OrWord;

    /// <summary>Ci je operator porovnanie.</summary>
    public bool IsComparison => Operator is >= ExprTokenKind.Equal and <= ExprTokenKind.Greater;
}
