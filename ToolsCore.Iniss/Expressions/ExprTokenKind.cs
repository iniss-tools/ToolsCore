using System.Diagnostics.CodeAnalysis;

namespace ToolsCore.Iniss.Expressions;

/// <summary>
/// Druhy tokenov jazyka vyrazov. Ciselne hodnoty 1-26 su indexy v tabulke prekladaca INISSu,
/// <see cref="Number"/>, <see cref="String"/> a <see cref="End"/> su jeho vnutorne kody 0x58-0x5a.
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
