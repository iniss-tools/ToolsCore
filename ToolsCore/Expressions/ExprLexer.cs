namespace ToolsCore.Expressions;

/// <summary>
///     Lexikalny analyzator jazyka vyrazov - verna kopia spravania INISSu.
/// </summary>
public sealed class ExprLexer
{
    /// <summary>Datum, od ktoreho sa pocitaju OLE datumy.</summary>
    public static readonly DateOnly OleEpoch = new(1899, 12, 30);

    private static readonly Dictionary<string, ExprTokenKind> Keywords = new(StringComparer.OrdinalIgnoreCase)
    {
        ["("] = ExprTokenKind.LParen, [")"] = ExprTokenKind.RParen,
        ["&"] = ExprTokenKind.BitAnd, ["|"] = ExprTokenKind.BitOr, ["^"] = ExprTokenKind.BitXor, ["~"] = ExprTokenKind.BitNot,
        ["&&"] = ExprTokenKind.And, ["||"] = ExprTokenKind.Or, ["!"] = ExprTokenKind.Not,
        ["AND"] = ExprTokenKind.AndWord, ["OR"] = ExprTokenKind.OrWord, ["NOT"] = ExprTokenKind.NotWord,
        ["=="] = ExprTokenKind.Equal, ["!="] = ExprTokenKind.NotEqual, ["<="] = ExprTokenKind.LessOrEqual,
        ["<"] = ExprTokenKind.Less, [">="] = ExprTokenKind.GreaterOrEqual, [">"] = ExprTokenKind.Greater,
        ["+"] = ExprTokenKind.Plus, ["-"] = ExprTokenKind.Minus, ["*"] = ExprTokenKind.Star,
        ["/"] = ExprTokenKind.Slash, ["%"] = ExprTokenKind.Percent, ["ODD"] = ExprTokenKind.Odd,
        ["?"] = ExprTokenKind.Question, [":"] = ExprTokenKind.Colon
    };

    private readonly string _text;
    private readonly ExprContext _context;
    private readonly IExprSymbolProvider? _symbols;
    private int _pos;

    /// <summary>
    ///     Vytvori lexer nad textom vyrazu.
    /// </summary>
    /// <param name="text">Text vyrazu (jeden riadok).</param>
    /// <param name="context">Kontext prekladu.</param>
    /// <param name="symbols">Symboly grafikonu (kluce TrTypes.txt) alebo <see langword="null"/>.</param>
    public ExprLexer(string? text, ExprContext context = ExprContext.Condition, IExprSymbolProvider? symbols = null)
    {
        _text = text ?? "";
        _context = context;
        _symbols = symbols;
    }

    /// <summary>
    ///     Chyba, na ktorej lexer skoncil; <see langword="null"/>, kym k nej nedoslo.
    /// </summary>
    public ExprDiagnostic? Error { get; private set; }

    /// <summary>
    ///     Aktualna pozicia v texte.
    /// </summary>
    public int Position => _pos;

    private char Cur => _pos < _text.Length ? _text[_pos] : '\0';

    private char Peek(int offset = 1) => _pos + offset < _text.Length ? _text[_pos + offset] : '\0';

    /// <summary>
    ///     Rozlozi cely text na tokeny (vratane koncoveho <see cref="ExprTokenKind.End"/>).
    ///     Pri chybe sa zoznam konci tokenom <see cref="ExprTokenKind.None"/> a <see cref="Error"/> je nastavene.
    /// </summary>
    public List<ExprToken> Tokenize()
    {
        var list = new List<ExprToken>();
        while (true)
        {
            var t = Next();
            list.Add(t);
            if (t.Kind is ExprTokenKind.End or ExprTokenKind.None) break;
        }
        return list;
    }

    /// <summary>
    ///     Precita dalsi token.
    /// </summary>
    public ExprToken Next()
    {
        if (Error is not null)
            return new ExprToken(ExprTokenKind.None, _pos, 0, "");

        while (_pos < _text.Length && char.IsWhiteSpace(_text[_pos])) _pos++;

        var start = _pos;
        var c = Cur;

        if (c == '\0')
            return new ExprToken(ExprTokenKind.End, start, 0, "");

        if (char.IsLetter(c) || c == '_')
        {
            while (char.IsLetterOrDigit(Cur) || Cur == '_') _pos++;
            return Identifier(start);
        }

        if (c == '"')
        {
            var s = ReadDelimited('"', start);
            return s is null
                ? Fail(start)
                : new ExprToken(ExprTokenKind.String, start, _pos - start, s);
        }

        if (c == '#')
        {
            var s = ReadDelimited('#', start);
            if (s is null) return Fail(start);
            return s.Contains(':') ? TimeLiteral(s, start) : DateLiteral(s, start);
        }

        if (char.IsAsciiDigit(c))
            return NumberLiteral(start);

        _pos++;
        switch (c)
        {
            case '/' or '%' or '(' or ')' or '*' or '+' or '-' or ':' or '?' or '^' or '~':
                return Operator(start);
            case '&' or '|':
                if (Cur == c) _pos++;
                return Operator(start);
            case '!' or '<' or '=' or '>':
                if (Cur == '=') _pos++;
                return Operator(start);
            default:
                // INISS: "Neznámy symbol <znak>"
                return Fail(start, ExprDiagnosticCode.InissUnknownSymbol, ExprMessages.UnknownSymbol(c.ToString()));
        }
    }

    private ExprToken Operator(int start)
    {
        var text = _text[start.._pos];
        if (Keywords.TryGetValue(text, out var kind))
            return new ExprToken(kind, start, text.Length, text);
        // samotne "=" - v tabulke nie je, konstantou nie je
        return Fail(start, ExprDiagnosticCode.InissUnknownSymbol, ExprMessages.UnknownSymbol(text));
    }

    private ExprToken Identifier(int start)
    {
        var text = _text[start.._pos];
        if (Keywords.TryGetValue(text, out var kind))
            return new ExprToken(kind, start, text.Length, text);

        var fn = ExprFunctions.Find(text);
        if (fn is not null)
            return new ExprToken(ExprTokenKind.Function, start, text.Length, text) { Function = fn };

        var c = ExprConstants.Resolve(text, _context, _symbols);
        if (c is not null)
            return new ExprToken(ExprTokenKind.Number, start, text.Length, text)
            {
                Value = c.Value.Value,
                NumberSource = c.Value.TrainType is null ? ExprNumberSource.Constant : ExprNumberSource.TrainType,
                Constant = c.Value.Constant,
                TrainType = c.Value.TrainType
            };

        return Fail(start, ExprDiagnosticCode.InissUnknownSymbol, ExprMessages.UnknownSymbol(text));
    }

    /// <summary>
    ///     Precita text medzi dvoma vyskytmi <paramref name="delim"/>; bez koncoveho vrati <see langword="null"/>
    ///     a nastavi chybu. Koniec riadka retazec ukonci ako chybu.
    /// </summary>
    private string? ReadDelimited(char delim, int start)
    {
        _pos++; // otvaraci znak
        var sb = new StringBuilder();
        while (true)
        {
            var c = Cur;
            if (c == delim)
            {
                _pos++;
                return sb.ToString();
            }
            if (c is '\0' or '\n' or '\r')
            {
                Error = new ExprDiagnostic(ExprSeverity.Error, ExprDiagnosticCode.InissUnterminatedString,
                    ExprMessages.UnterminatedString, start, _pos - start);
                return null;
            }
            sb.Append(c);
            _pos++;
        }
    }

    private ExprToken NumberLiteral(int start)
    {
        // INISS zbiera cislice a pismena A-Z (po toupper) a cely text da strtol(…, 0)
        while (char.IsAsciiDigit(Cur) || char.IsAsciiLetter(Cur)) _pos++;
        var text = _text[start.._pos];

        if (!TryStrtol(text, out var value))
            return Fail(start, ExprDiagnosticCode.InissNumberExpected, ExprMessages.NumberExpected);

        return new ExprToken(ExprTokenKind.Number, start, text.Length, text)
        {
            Value = value,
            NumberSource = ExprNumberSource.Literal
        };
    }

    /// <summary>
    ///     <c>strtol(text, &amp;end, 0)</c> s podmienkou, ze sa spracuje cely text a hodnota sa zmesti do 32 bitov.
    /// </summary>
    public static bool TryStrtol(string text, out int value)
    {
        value = 0;
        if (text.Length == 0) return false;

        long result = 0;
        int i = 0, radix = 10;
        if (text[0] == '0' && text.Length > 1)
        {
            if (text[1] is 'x' or 'X')
            {
                radix = 16;
                i = 2;
                if (i >= text.Length) return false; // "0x" - strtol precita len "0", zvysok ostane
            }
            else
            {
                radix = 8;
                i = 1;
            }
        }

        var any = false;
        for (; i < text.Length; i++)
        {
            var d = DigitValue(text[i]);
            if (d < 0 || d >= radix) return false;
            result = result * radix + d;
            if (result > int.MaxValue) return false; // ERANGE
            any = true;
        }

        if (!any) return false;
        value = (int)result;
        return true;
    }

    private static int DigitValue(char c) => c switch
    {
        >= '0' and <= '9' => c - '0',
        >= 'a' and <= 'z' => c - 'a' + 10,
        >= 'A' and <= 'Z' => c - 'A' + 10,
        _ => -1
    };

    private ExprToken DateLiteral(string s, int start)
    {
        var parts = s.Split('.');
        if (parts.Length == 3
            && TryInt(parts[0], out var day) && TryInt(parts[1], out var month) && TryInt(parts[2], out var year)
            && year is >= 100 and <= 9999 && month is >= 1 and <= 12 && day >= 1 && day <= DateTime.DaysInMonth(year, month))
        {
            var value = new DateOnly(year, month, day).DayNumber - OleEpoch.DayNumber;
            return new ExprToken(ExprTokenKind.Number, start, _pos - start, s)
            {
                Value = value,
                NumberSource = ExprNumberSource.DateLiteral
            };
        }

        return Fail(start, ExprDiagnosticCode.InissBadDateFormat, ExprMessages.BadDateFormat);
    }

    private ExprToken TimeLiteral(string s, int start)
    {
        var parts = s.Split(':');
        if (parts.Length is 2 or 3
            && TryInt(parts[0], out var h) && TryInt(parts[1], out var m)
            && (parts.Length == 2 ? TrySeconds0(out var sec) : TryInt(parts[2], out sec)))
        {
            return new ExprToken(ExprTokenKind.Number, start, _pos - start, s)
            {
                Value = (h * 60 + m) * 60 + sec,
                NumberSource = ExprNumberSource.TimeLiteral
            };
        }

        return Fail(start, ExprDiagnosticCode.InissBadTimeFormat, ExprMessages.BadTimeFormat);

        static bool TrySeconds0(out int sec)
        {
            sec = 0;
            return true;
        }
    }

    private static bool TryInt(string s, out int value) =>
        int.TryParse(s.Trim(), System.Globalization.NumberStyles.AllowLeadingSign,
            System.Globalization.CultureInfo.InvariantCulture, out value);

    private ExprToken Fail(int start) => new(ExprTokenKind.None, start, _pos - start, "");

    private ExprToken Fail(int start, ExprDiagnosticCode code, string message)
    {
        Error = new ExprDiagnostic(ExprSeverity.Error, code, message, start, _pos - start);
        return new ExprToken(ExprTokenKind.None, start, _pos - start, "");
    }
}
