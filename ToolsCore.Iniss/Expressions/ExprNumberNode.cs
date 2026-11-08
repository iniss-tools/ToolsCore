namespace ToolsCore.Iniss.Expressions;

/// <summary>
/// Cislo - literal, literal datumu/casu alebo pomenovana konstanta.
/// </summary>
public sealed record ExprNumberNode(int Start, int Length, int Value, ExprToken Token) : ExprNode(Start, Length)
{
    /// <inheritdoc/>
    public override IEnumerable<ExprNode> Children => [];

    /// <summary>Povod hodnoty.</summary>
    public ExprNumberSource Source => Token.NumberSource;
}
