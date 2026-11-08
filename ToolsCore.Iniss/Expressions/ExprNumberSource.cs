namespace ToolsCore.Iniss.Expressions;

/// <summary>
/// Ako vznikol token <see cref="ExprTokenKind.Number"/>.
/// </summary>
public enum ExprNumberSource
{
    Literal,
    DateLiteral,
    TimeLiteral,
    Constant,
    TrainType
}
