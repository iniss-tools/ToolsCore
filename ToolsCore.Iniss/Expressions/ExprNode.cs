namespace ToolsCore.Iniss.Expressions;

/// <summary>
/// Uzol syntaktickeho stromu vyrazu.
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
    /// Prejde uzol a vsetkych potomkov do hlbky.
    /// </summary>
    public IEnumerable<ExprNode> Descendants()
    {
        yield return this;
        foreach (var ch in Children)
        foreach (var d in ch.Descendants())
            yield return d;
    }
}
