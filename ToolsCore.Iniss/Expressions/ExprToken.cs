namespace ToolsCore.Iniss.Expressions;

/// <summary>
/// Token vyrazu.
/// </summary>
/// <param name="Kind">Druh.</param>
/// <param name="Start">Index prveho znaku v texte.</param>
/// <param name="Length">Dlzka v znakoch.</param>
/// <param name="Text">Povodny text tokenu (pri retazci obsah bez uvodzoviek).</param>
public sealed record ExprToken(ExprTokenKind Kind, int Start, int Length, string Text)
{
    /// <summary>Hodnota cisla alebo konstanty.</summary>
    public int Value { get; init; }

    /// <summary>Povod ciselnej hodnoty.</summary>
    public ExprNumberSource NumberSource { get; init; }

    /// <summary>Funkcia pri <see cref="ExprTokenKind.Function"/>.</summary>
    public ExprFunctionInfo? Function { get; init; }

    /// <summary>Pomenovana konstanta, z ktorej hodnota vznikla.</summary>
    public ExprConstantInfo? Constant { get; init; }

    /// <summary>Druh vlaku, z ktoreho hodnota vznikla (<c>Typ_…</c>).</summary>
    public ExprTrainTypeMatch? TrainType { get; init; }

    /// <summary>Index za poslednym znakom tokenu.</summary>
    public int End => Start + Length;

    /// <summary>
    /// Meno tokenu tak, ako ho pise INISS v hlaseniach (<c>konštanta</c>, <c>koniec</c>, text operatora …).
    /// </summary>
    public string DisplayName => ExprMessages.TokenName(Kind, Text);
}
