namespace ToolsCore.Expressions;

/// <summary>
///     Vysledok prekladu vyrazu.
/// </summary>
/// <param name="Text">Prekladany text.</param>
/// <param name="Root">Koren stromu; <see langword="null"/> pri chybe prekladu.</param>
/// <param name="Tokens">Tokeny v poradi (pri chybe po chybny token).</param>
/// <param name="Diagnostics">Hlasenia - pri chybe prekladu prave jedna chyba (INISS hlasi len prvu).</param>
public sealed record ExprParseResult(string Text, ExprNode? Root, IReadOnlyList<ExprToken> Tokens, IReadOnlyList<ExprDiagnostic> Diagnostics)
{
    /// <summary>Ci sa vyraz prelozil.</summary>
    public bool Success => Root is not null;

    /// <summary>Chyba prekladu, ak nastala.</summary>
    public ExprDiagnostic? Error => Diagnostics.FirstOrDefault(d => d.IsError);
}

/// <summary>
///     Rekurzivny zostupny prekladac jazyka vyrazov - verna kopia gramatiky INISSu.
/// </summary>
public sealed class ExprParser
{
    private readonly ExprLexer _lexer;
    private readonly List<ExprToken> _tokens = [];
    private ExprToken _cur = null!;

    private ExprParser(string text, ExprContext context, IExprSymbolProvider? symbols)
    {
        _lexer = new ExprLexer(text, context, symbols);
    }

    /// <summary>
    ///     Prelozi vyraz.
    /// </summary>
    /// <param name="text">Text vyrazu.</param>
    /// <param name="context">Kontext prekladu (urcuje prijimane konstanty).</param>
    /// <param name="symbols">Symboly grafikonu alebo <see langword="null"/>.</param>
    public static ExprParseResult Parse(string? text, ExprContext context = ExprContext.Condition, IExprSymbolProvider? symbols = null)
    {
        text ??= "";
        var p = new ExprParser(text, context, symbols);
        try
        {
            p.Advance();
            if (!IsExpressionStart(p._cur.Kind))
                throw p.Unexpected();

            var root = p.ParseExpression();
            p.Expect(ExprTokenKind.End);
            return new ExprParseResult(text, root, p._tokens, []);
        }
        catch (ParseAbort e)
        {
            return new ExprParseResult(text, null, p._tokens, [e.Diagnostic]);
        }
    }

    private static bool IsExpressionStart(ExprTokenKind k) =>
        k is ExprTokenKind.LParen or ExprTokenKind.Number or ExprTokenKind.Function or ExprTokenKind.Odd
            or ExprTokenKind.Plus or ExprTokenKind.Minus
            or ExprTokenKind.BitNot or ExprTokenKind.Not or ExprTokenKind.NotWord;

    // ---------------------------------------------------------------- tokeny

    private ExprToken Advance()
    {
        var t = _lexer.Next();
        if (_lexer.Error is not null)
        {
            _tokens.Add(t);
            throw new ParseAbort(_lexer.Error);
        }
        _tokens.Add(t);
        _cur = t;
        return t;
    }

    private void Expect(ExprTokenKind kind)
    {
        if (_cur.Kind != kind)
            throw new ParseAbort(new ExprDiagnostic(ExprSeverity.Error, ExprDiagnosticCode.InissExpected,
                ExprMessages.Expected(ExprMessages.TokenName(kind, "")), _cur.Start, _cur.Length));
        if (kind != ExprTokenKind.End)
            Advance();
    }

    private ParseAbort Unexpected() =>
        new(new ExprDiagnostic(ExprSeverity.Error, ExprDiagnosticCode.InissUnexpected,
            ExprMessages.Unexpected(_cur.DisplayName), _cur.Start, _cur.Length));

    // ---------------------------------------------------------------- gramatika

    /// <summary>vyraz := logicky [ '?' vyraz ':' vyraz ]</summary>
    private ExprNode ParseExpression()
    {
        var cond = ParseLogical();
        if (_cur.Kind != ExprTokenKind.Question)
            return cond;

        Advance();
        var whenTrue = ParseExpression();
        Expect(ExprTokenKind.Colon);
        var whenFalse = ParseExpression();
        return new ExprConditionalNode(cond.Start, whenFalse.End - cond.Start, cond, whenTrue, whenFalse);
    }

    /// <summary>logicky := negacia [ spojka logicky ]  (jedna uroven, prava asociativita)</summary>
    private ExprNode ParseLogical()
    {
        var left = ParseNegation();
        var op = _cur.Kind;
        if (op is not (ExprTokenKind.BitAnd or ExprTokenKind.BitOr or ExprTokenKind.BitXor
            or ExprTokenKind.And or ExprTokenKind.Or or ExprTokenKind.AndWord or ExprTokenKind.OrWord))
            return left;

        var opStart = _cur.Start;
        Advance();
        var right = ParseLogical();
        return new ExprBinaryNode(left.Start, right.End - left.Start, op, left, right, opStart);
    }

    /// <summary>negacia := [ '~' | '!' | 'NOT' ] porovnanie</summary>
    private ExprNode ParseNegation()
    {
        var op = _cur.Kind;
        if (op is not (ExprTokenKind.BitNot or ExprTokenKind.Not or ExprTokenKind.NotWord))
            return ParseComparison();

        var start = _cur.Start;
        Advance();
        var operand = ParseComparison();
        return new ExprUnaryNode(start, operand.End - start, op, operand);
    }

    /// <summary>porovnanie := znamienko [ relacia znamienko ]</summary>
    private ExprNode ParseComparison()
    {
        var left = ParseSign();
        var op = _cur.Kind;
        if (op is not (>= ExprTokenKind.Equal and <= ExprTokenKind.Greater))
            return left;

        var opStart = _cur.Start;
        Advance();
        var right = ParseSign();
        return new ExprBinaryNode(left.Start, right.End - left.Start, op, left, right, opStart);
    }

    /// <summary>znamienko := [ '+' | '-' ] sucet</summary>
    private ExprNode ParseSign()
    {
        var op = _cur.Kind;
        if (op is not (ExprTokenKind.Plus or ExprTokenKind.Minus))
            return ParseSum();

        var start = _cur.Start;
        Advance();
        var operand = ParseSum();
        return new ExprUnaryNode(start, operand.End - start, op, operand);
    }

    /// <summary>sucet := sucin [ ( '+' | '-' ) sucet ]  (prava asociativita)</summary>
    private ExprNode ParseSum()
    {
        var left = ParseProduct();
        var op = _cur.Kind;
        if (op is not (ExprTokenKind.Plus or ExprTokenKind.Minus))
            return left;

        var opStart = _cur.Start;
        Advance();
        var right = ParseSum();
        return new ExprBinaryNode(left.Start, right.End - left.Start, op, left, right, opStart);
    }

    /// <summary>sucin := prvok [ ( '*' | '/' | '%' ) sucin ]  (prava asociativita)</summary>
    private ExprNode ParseProduct()
    {
        var left = ParsePrimary();
        var op = _cur.Kind;
        if (op is not (ExprTokenKind.Star or ExprTokenKind.Slash or ExprTokenKind.Percent))
            return left;

        var opStart = _cur.Start;
        Advance();
        var right = ParseProduct();
        return new ExprBinaryNode(left.Start, right.End - left.Start, op, left, right, opStart);
    }

    /// <summary>prvok := '(' vyraz ')' | cislo | 'ODD' '(' vyraz ')' | funkcia</summary>
    private ExprNode ParsePrimary()
    {
        var tok = _cur;
        switch (tok.Kind)
        {
            case ExprTokenKind.LParen:
            {
                Advance();
                var inner = ParseExpression();
                var close = _cur;
                Expect(ExprTokenKind.RParen);
                return new ExprParenNode(tok.Start, close.End - tok.Start, inner);
            }

            case ExprTokenKind.Number:
                Advance();
                return new ExprNumberNode(tok.Start, tok.Length, tok.Value, tok);

            case ExprTokenKind.Odd:
            {
                Advance();
                Expect(ExprTokenKind.LParen);
                var inner = ParseExpression();
                var close = _cur;
                Expect(ExprTokenKind.RParen);
                return new ExprUnaryNode(tok.Start, close.End - tok.Start, ExprTokenKind.Odd, inner);
            }

            case ExprTokenKind.Function:
                return ParseFunction(tok);

            default:
                throw Unexpected();
        }
    }

    private ExprNode ParseFunction(ExprToken tok)
    {
        var fn = tok.Function!;
        Advance();

        switch (fn.ArgKind)
        {
            case ExprArgKind.None:
                return new ExprFunctionNode(tok.Start, tok.Length, fn, null, tok);

            case ExprArgKind.OptionalNumber:
            case ExprArgKind.OptionalNumberOrString:
            {
                if (_cur.Kind != ExprTokenKind.LParen)
                    return new ExprFunctionNode(tok.Start, tok.Length, fn, null, tok);
                Advance();
                ExprNode arg;
                if (fn.ArgKind == ExprArgKind.OptionalNumberOrString && _cur.Kind == ExprTokenKind.String)
                {
                    arg = new ExprStringNode(_cur.Start, _cur.Length, _cur.Text);
                    Advance();
                }
                else
                {
                    arg = ParseExpression();
                }
                var close = _cur;
                Expect(ExprTokenKind.RParen);
                return new ExprFunctionNode(tok.Start, close.End - tok.Start, fn, arg, tok);
            }

            case ExprArgKind.RequiredNumber:
            {
                Expect(ExprTokenKind.LParen);
                var arg = ParseExpression();
                var close = _cur;
                Expect(ExprTokenKind.RParen);
                return new ExprFunctionNode(tok.Start, close.End - tok.Start, fn, arg, tok);
            }

            case ExprArgKind.RequiredString:
            {
                Expect(ExprTokenKind.LParen);
                if (_cur.Kind != ExprTokenKind.String)
                    throw new ParseAbort(new ExprDiagnostic(ExprSeverity.Error, ExprDiagnosticCode.InissExpected,
                        ExprMessages.Expected(ExprMessages.TokenString), _cur.Start, _cur.Length));
                var arg = new ExprStringNode(_cur.Start, _cur.Length, _cur.Text);
                Advance();
                var close = _cur;
                Expect(ExprTokenKind.RParen);
                return new ExprFunctionNode(tok.Start, close.End - tok.Start, fn, arg, tok);
            }

            default:
                throw new InvalidOperationException(fn.ArgKind.ToString());
        }
    }

    private sealed class ParseAbort(ExprDiagnostic diagnostic) : Exception(diagnostic.Message)
    {
        public ExprDiagnostic Diagnostic { get; } = diagnostic;
    }
}
