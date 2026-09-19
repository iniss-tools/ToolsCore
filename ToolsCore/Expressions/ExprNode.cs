namespace ToolsCore.Expressions;

/// <summary>
///     Uzol syntaktickeho stromu vyrazu.
/// </summary>
/// <param name="Start">Index prveho znaku uzla v texte.</param>
/// <param name="Length">Dlzka uzla v znakoch.</param>
public abstract record ExprNode(int Start, int Length)
{
    /// <summary>Index za poslednym znakom uzla.</summary>
    public int End => Start + Length;

    /// <summary>Priami potomkovia uzla.</summary>
    public abstract IEnumerable<ExprNode> Children { get; }

    /// <summary>
    ///     Prejde uzol a vsetkych potomkov do hlbky.
    /// </summary>
    public IEnumerable<ExprNode> Descendants()
    {
        yield return this;
        foreach (var ch in Children)
        foreach (var d in ch.Descendants())
            yield return d;
    }
}

/// <summary>
///     Cislo - literal, literal datumu/casu alebo pomenovana konstanta.
/// </summary>
public sealed record ExprNumberNode(int Start, int Length, int Value, ExprToken Token) : ExprNode(Start, Length)
{
    /// <inheritdoc/>
    public override IEnumerable<ExprNode> Children => [];

    /// <summary>Povod hodnoty.</summary>
    public ExprNumberSource Source => Token.NumberSource;
}

/// <summary>
///     Retazec v uvodzovkach (len ako argument funkcie).
/// </summary>
public sealed record ExprStringNode(int Start, int Length, string Value) : ExprNode(Start, Length)
{
    /// <inheritdoc/>
    public override IEnumerable<ExprNode> Children => [];
}

/// <summary>
///     Unarny operator: <c>~</c>, <c>!</c>/<c>NOT</c>, znamienko <c>+</c>/<c>-</c>, <c>ODD</c>.
/// </summary>
public sealed record ExprUnaryNode(int Start, int Length, ExprTokenKind Operator, ExprNode Operand) : ExprNode(Start, Length)
{
    /// <inheritdoc/>
    public override IEnumerable<ExprNode> Children => [Operand];
}

/// <summary>
///     Binarny operator.
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

/// <summary>
///     Podmieneny operator <c>p ? a : b</c>.
/// </summary>
public sealed record ExprConditionalNode(int Start, int Length, ExprNode Condition, ExprNode WhenTrue, ExprNode WhenFalse)
    : ExprNode(Start, Length)
{
    /// <inheritdoc/>
    public override IEnumerable<ExprNode> Children => [Condition, WhenTrue, WhenFalse];
}

/// <summary>
///     Volanie funkcie; <see cref="Argument"/> je <see langword="null"/> pri funkcii bez argumentu.
/// </summary>
public sealed record ExprFunctionNode(int Start, int Length, ExprFunctionInfo Function, ExprNode? Argument, ExprToken Token)
    : ExprNode(Start, Length)
{
    /// <inheritdoc/>
    public override IEnumerable<ExprNode> Children => Argument is null ? [] : [Argument];

    /// <summary>Kanonicka funkcia (anglicky alias prevedeny na cesku podobu).</summary>
    public ExprFunction Canonical => Function.Canonical;
}

/// <summary>
///     Vyraz v zatvorkach - v strome sa drzi kvoli presnym poziciam a formatovaniu.
/// </summary>
public sealed record ExprParenNode(int Start, int Length, ExprNode Inner) : ExprNode(Start, Length)
{
    /// <inheritdoc/>
    public override IEnumerable<ExprNode> Children => [Inner];
}
