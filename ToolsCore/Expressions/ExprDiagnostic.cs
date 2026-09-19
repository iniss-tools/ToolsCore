namespace ToolsCore.Expressions;

/// <summary>
///     Zavaznost hlasenia.
/// </summary>
public enum ExprSeverity
{
    /// <summary>Informacia - vyraz je v poriadku, ale stoji za pozornost.</summary>
    Info,

    /// <summary>Varovanie - INISS vyraz prelozi, ale pravdepodobne nerobi to, co autor chcel.</summary>
    Warning,

    /// <summary>Chyba - INISS vyraz neprelozi (hlasenie zodpoveda jeho textu).</summary>
    Error
}

/// <summary>
///     Kod hlasenia. Kody <c>Iniss*</c> zodpovedaju chybam prekladaca INISSu (RCIniss.dll 8001-8012),
///     ostatne su kontroly GVDEditora nad ramec INISSu.
/// </summary>
public enum ExprDiagnosticCode
{
    // --- chyby prekladaca INISSu ---
    InissUnterminatedString,
    InissNumberExpected,
    InissUnknownSymbol,
    InissExpected,
    InissUnexpected,
    InissBadDateFormat,
    InissBadTimeFormat,

    // --- semanticke kontroly GVDEditora ---
    /// <summary><c>Typ_X</c> sa nasiel len medzi zabudovanymi nazvami, nie v TrTypes.txt.</summary>
    TrainTypeNotInTrTypes,

    /// <summary><c>Typ_X</c> sa nasiel len bez ohladu na velkost pismen alebo diakritiku.</summary>
    TrainTypeInexactMatch,

    /// <summary>ID stanice v argumente neexistuje v grafikone.</summary>
    UnknownStation,

    /// <summary>Kolaj v argumente neexistuje v Pozice.txt.</summary>
    UnknownTrack,

    /// <summary>Dopravca v argumente neexistuje vo Vlastnik.txt.</summary>
    UnknownOperator,

    /// <summary>Porovnanie masky (<c>PRIZNAK(x) == 1</c>) - funkcia vracia masku, nie 0/1.</summary>
    MaskComparedToNumber,

    /// <summary>Delenie konstantnou nulou - INISS pri vyhodnoteni spadne.</summary>
    DivisionByZero,

    /// <summary>Spojky roznej priority bez zatvoriek (<c>a &amp;&amp; b || c</c>) - INISS viaze sprava.</summary>
    MixedLogicalOperators,

    /// <summary>Retazenie <c>-</c> alebo <c>/</c> bez zatvoriek - INISS viaze sprava.</summary>
    RightAssociativeArithmetic,

    /// <summary>Negacia pred porovnanim (<c>!a == b</c>) - plati na cele porovnanie.</summary>
    NegationOfComparison,

    /// <summary>Znamienko pred suctom (<c>-a + b</c>) - plati na cely sucet.</summary>
    SignOfSum,

    /// <summary>Cislo s veducou nulou - INISS ho cita osmickovo.</summary>
    OctalLiteral,

    /// <summary>Funkcia zavisla od kontextu (<c>VYLUKAZDE</c>, <c>ZPOZDENI</c>).</summary>
    ContextDependentFunction,

    /// <summary>Podmienka je konstanta (okrem <c>1</c>).</summary>
    ConstantCondition
}

/// <summary>
///     Hlasenie o vyraze.
/// </summary>
/// <param name="Severity">Zavaznost.</param>
/// <param name="Code">Kod.</param>
/// <param name="Message">Text (slovensky; pri chybach INISSu jeho vlastny text).</param>
/// <param name="Start">Index prveho znaku miesta v texte vyrazu.</param>
/// <param name="Length">Dlzka miesta v znakoch (0 = bodove hlasenie, napr. koniec textu).</param>
public sealed record ExprDiagnostic(ExprSeverity Severity, ExprDiagnosticCode Code, string Message, int Start, int Length)
{
    /// <summary>Index za poslednym znakom miesta.</summary>
    public int End => Start + Length;

    /// <summary>Odporucane riesenie (text pre stlpec „Riešenie“); <see langword="null"/>, ak nie je.</summary>
    public string? Suggestion { get; init; }

    /// <summary>Automaticka oprava textu, ak sa da navrhnut; <see langword="null"/>, ak nie.</summary>
    public TextFix? Fix { get; init; }

    /// <summary>Ci ide o chybu prekladu.</summary>
    public bool IsError => Severity == ExprSeverity.Error;

    /// <inheritdoc/>
    public override string ToString() => $"{Severity} [{Start}+{Length}] {Message}";
}

/// <summary>
///     Jedna uprava textu: nahradenie useku <paramref name="Start"/>..<paramref name="Start"/>+<paramref name="Length"/> textom <paramref name="NewText"/>.
/// </summary>
public sealed record TextEdit(int Start, int Length, string NewText)
{
    /// <summary>Posunie upravu o <paramref name="offset"/> znakov.</summary>
    public TextEdit Shift(int offset) => this with { Start = Start + offset };
}

/// <summary>
///     Navrhovana oprava - nazov a zoznam uprav (neprekryvajucich sa, v lubovolnom poradi).
/// </summary>
public sealed record TextFix(string Title, IReadOnlyList<TextEdit> Edits)
{
    /// <summary>Oprava s jedinou upravou.</summary>
    public static TextFix Single(string title, int start, int length, string newText) => new(title, [new TextEdit(start, length, newText)]);

    /// <summary>Posunie vsetky upravy o <paramref name="offset"/> znakov.</summary>
    public TextFix Shift(int offset) => new(Title, Edits.Select(e => e.Shift(offset)).ToList());

    /// <summary>
    ///     Pouzije upravy na text (od konca, aby sa pozicie neposuvali).
    /// </summary>
    public string Apply(string text)
    {
        var sb = new StringBuilder(text);
        foreach (var e in Edits.OrderByDescending(e => e.Start))
        {
            sb.Remove(e.Start, e.Length);
            sb.Insert(e.Start, e.NewText);
        }
        return sb.ToString();
    }
}

/// <summary>
///     Texty hlaseni. Chyby prekladaca sa zhoduju s textami INISSu, aby sa v editore dalo hladat to,
///     co vypisal log.
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
    ///     Meno tokenu v hlaseniach INISSu.
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
    ///     Zapis operatora podla druhu tokenu.
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
