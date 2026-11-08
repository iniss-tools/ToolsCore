namespace ToolsCore.Iniss.Expressions;

/// <summary>
/// Texty hlaseni. Chyby prekladaca sa zhoduju s textami INISSu, aby sa v editore dalo hladat to,
/// co vypisal log.
/// </summary>
public static class ExprMessages
{
    /// <summary>RCIniss 8001.</summary>
    public const string UnterminatedString = "Neukončený reťazec";

    /// <summary>RCIniss 8002.</summary>
    public const string NumberExpected = "Očakával som číslo";

    /// <summary>RCIniss 8004 (<c>Neznámy symbol %s</c>).</summary>
    public static string UnknownSymbol(string symbol) => $"Neznámy symbol {symbol}";

    /// <summary>RCIniss 8005 (<c>Očakávam %s</c>).</summary>
    public static string Expected(string tokenName) => $"Očakávam {tokenName}";

    /// <summary>RCIniss 8006 (<c>Neočakávaný symbol %s</c>).</summary>
    public static string Unexpected(string tokenName) => $"Neočakávaný symbol {tokenName}";

    /// <summary>RCIniss 8011.</summary>
    public const string BadDateFormat = "Neznámy formát dátumu";

    /// <summary>RCIniss 8012.</summary>
    public const string BadTimeFormat = "Neznámy formát času";

    /// <summary>RCIniss 8008.</summary>
    public const string TokenConstant = "konštanta";

    /// <summary>RCIniss 8009.</summary>
    public const string TokenEnd = "koniec";

    /// <summary>RCIniss 8010.</summary>
    public const string TokenString = "reťazec v úvodzovkách";

    /// <summary>
    /// Meno tokenu v hlaseniach INISSu.
    /// </summary>
    public static string TokenName(ExprTokenKind kind, string text) => kind switch
    {
        ExprTokenKind.Number => TokenConstant,
        ExprTokenKind.String => TokenString,
        ExprTokenKind.End => TokenEnd,
        ExprTokenKind.Function => text,
        _ => Operator(kind)
    };

    /// <summary>
    /// Zapis operatora podla druhu tokenu.
    /// </summary>
    public static string Operator(ExprTokenKind kind) => kind switch
    {
        ExprTokenKind.LParen => "(",
        ExprTokenKind.RParen => ")",
        ExprTokenKind.BitAnd => "&",
        ExprTokenKind.BitOr => "|",
        ExprTokenKind.BitXor => "^",
        ExprTokenKind.BitNot => "~",
        ExprTokenKind.And => "&&",
        ExprTokenKind.Or => "||",
        ExprTokenKind.Not => "!",
        ExprTokenKind.AndWord => "AND",
        ExprTokenKind.OrWord => "OR",
        ExprTokenKind.NotWord => "NOT",
        ExprTokenKind.Equal => "==",
        ExprTokenKind.NotEqual => "!=",
        ExprTokenKind.LessOrEqual => "<=",
        ExprTokenKind.Less => "<",
        ExprTokenKind.GreaterOrEqual => ">=",
        ExprTokenKind.Greater => ">",
        ExprTokenKind.Plus => "+",
        ExprTokenKind.Minus => "-",
        ExprTokenKind.Star => "*",
        ExprTokenKind.Slash => "/",
        ExprTokenKind.Percent => "%",
        ExprTokenKind.Odd => "ODD",
        ExprTokenKind.Question => "?",
        ExprTokenKind.Colon => ":",
        _ => kind.ToString()
    };
}
