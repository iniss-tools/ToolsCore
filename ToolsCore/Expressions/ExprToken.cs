using System.Diagnostics.CodeAnalysis;

namespace ToolsCore.Expressions;

/// <summary>
///     Druhy tokenov jazyka vyrazov. Ciselne hodnoty 1-26 su indexy v tabulke prekladaca INISSu,
///     <see cref="Number"/>, <see cref="String"/> a <see cref="End"/> su jeho vnutorne kody 0x58-0x5a.
/// </summary>
public enum ExprTokenKind
{
    None = 0,
    LParen = 1,
    RParen = 2,
    BitAnd = 3,
    BitOr = 4,
    BitXor = 5,
    BitNot = 6,
    And = 7,
    Or = 8,
    Not = 9,
    AndWord = 10,
    OrWord = 11,
    NotWord = 12,
    Equal = 13,
    NotEqual = 14,
    LessOrEqual = 15,
    Less = 16,
    GreaterOrEqual = 17,
    Greater = 18,
    Plus = 19,
    Minus = 20,
    Star = 21,
    Slash = 22,
    Percent = 23,
    Odd = 24,
    Question = 25,
    Colon = 26,

    /// <summary>Volanie funkcie - <see cref="ExprToken.Function"/> hovori ktorej.</summary>
    Function = 0x40,

    /// <summary>Cislo, literal datumu/casu alebo pomenovana konstanta - hodnota v <see cref="ExprToken.Value"/>.</summary>
    Number = 0x58,

    /// <summary>Retazec v uvodzovkach - text v <see cref="ExprToken.Text"/>.</summary>
    [SuppressMessage("Naming", "CA1720:Identifier contains type name")] 
    String = 0x59,

    /// <summary>Koniec vyrazu.</summary>
    End = 0x5a
}

/// <summary>
///     Ako vznikol token <see cref="ExprTokenKind.Number"/>.
/// </summary>
public enum ExprNumberSource
{
    Literal,
    DateLiteral,
    TimeLiteral,
    Constant,
    TrainType
}

/// <summary>
///     Token vyrazu.
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
    ///     Meno tokenu tak, ako ho pise INISS v hlaseniach (<c>konštanta</c>, <c>koniec</c>, text operatora …).
    /// </summary>
    public string DisplayName => ExprMessages.TokenName(Kind, Text);
}
