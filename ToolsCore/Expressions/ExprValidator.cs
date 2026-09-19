namespace ToolsCore.Expressions;

/// <summary>
///     Nastavenia semantickej kontroly.
/// </summary>
public sealed class ExprValidationOptions
{
    /// <summary>Kontext prekladu.</summary>
    public ExprContext Context { get; init; } = ExprContext.Condition;

    /// <summary>Symboly grafikonu; <see langword="null"/> vypne kontroly proti datam.</summary>
    public IExprSymbolProvider? Symbols { get; init; }

    /// <summary>Ci ide o podmienku (TabTab, AutoCondition) - zapina kontrolu konstantnej podmienky.</summary>
    public bool IsCondition { get; init; } = true;

    /// <summary>Ci hlasit funkcie zavisle od kontextu (<c>VYLUKAZDE</c>, <c>ZPOZDENI</c>).</summary>
    public bool ReportContextDependent { get; init; } = true;
}

/// <summary>
///     Vysledok prekladu a kontroly.
/// </summary>
/// <param name="Parse">Vysledok prekladu.</param>
/// <param name="Diagnostics">Vsetky hlasenia - chyba prekladu alebo semanticke kontroly.</param>
public sealed record ExprValidationResult(ExprParseResult Parse, IReadOnlyList<ExprDiagnostic> Diagnostics)
{
    /// <summary>Koren stromu, ak sa vyraz prelozil.</summary>
    public ExprNode? Root => Parse.Root;

    /// <summary>Ci vyraz INISS prelozi.</summary>
    public bool Compiles => Parse.Success;

    /// <summary>Ci su medzi hlaseniami chyby.</summary>
    public bool HasErrors => Diagnostics.Any(d => d.IsError);

    /// <summary>Ci su medzi hlaseniami varovania.</summary>
    public bool HasWarnings => Diagnostics.Any(d => d.Severity == ExprSeverity.Warning);
}

/// <summary>
///     Prelozi vyraz a nad stromom vykona kontroly, ktore INISS nerobi, ale ktore odhalia
///     typicke omyly (preklepy v ID stanic, porovnanie masky s cislom, priorita spojok …).
/// </summary>
public static class ExprValidator
{
    /// <summary>
    ///     Prelozi a skontroluje vyraz.
    /// </summary>
    public static ExprValidationResult Validate(string text, ExprValidationOptions? options = null)
    {
        options ??= new ExprValidationOptions();
        var parse = ExprParser.Parse(text, options.Context, options.Symbols);
        if (parse.Root is null)
            return new ExprValidationResult(parse, parse.Diagnostics);

        var list = new List<ExprDiagnostic>();
        var v = new Visitor(options, list, parse);
        v.Visit(parse.Root);
        v.CheckRoot(parse.Root);
        list.Sort((a, b) => a.Start.CompareTo(b.Start));
        return new ExprValidationResult(parse, list);
    }

    private sealed class Visitor(ExprValidationOptions options, List<ExprDiagnostic> list, ExprParseResult parse)
    {
        private void Add(ExprSeverity severity, ExprDiagnosticCode code, string message, ExprNode at,
            string? suggestion = null, TextFix? fix = null) =>
            list.Add(new ExprDiagnostic(severity, code, message, at.Start, at.Length) { Suggestion = suggestion ?? fix?.Title, Fix = fix });

        /// <summary>Oprava: uzavriet usek od <paramref name="from"/> po <paramref name="to"/> do zatvoriek.</summary>
        private static TextFix Parenthesize(string title, ExprNode from, ExprNode to) =>
            new(title, [new TextEdit(from.Start, 0, "("), new TextEdit(to.End, 0, ")")]);

        public void Visit(ExprNode node)
        {
            switch (node)
            {
                case ExprNumberNode n:
                    VisitNumber(n);
                    break;
                case ExprFunctionNode f:
                    VisitFunction(f);
                    break;
                case ExprBinaryNode b:
                    VisitBinary(b);
                    break;
                case ExprUnaryNode u:
                    VisitUnary(u);
                    break;
            }

            foreach (var ch in node.Children)
                Visit(ch);
        }

        public void CheckRoot(ExprNode root)
        {
            if (!options.IsCondition) return;
            if (root is ExprNumberNode { Source: ExprNumberSource.Literal, Value: 1 }) return; // bezna zaloha "1"
            if (ExprEvaluator.TryFoldConstant(root, out var value))
                Add(ExprSeverity.Info, ExprDiagnosticCode.ConstantCondition,
                    value != 0 ? "Podmienka je vždy splnená" : "Podmienka nie je nikdy splnená", root,
                    value != 0 ? "Ako záložnú podmienku na konci zoznamu píšte 1" : "Položku odstráňte alebo doplňte podmienku");
        }

        private void VisitNumber(ExprNumberNode n)
        {
            var tok = n.Token;
            if (tok is { NumberSource: ExprNumberSource.Literal, Text.Length: > 1 } && tok.Text[0] == '0' && tok.Text[1] is not ('x' or 'X'))
            {
                var decimalText = tok.Text.TrimStart('0');
                if (decimalText.Length == 0) decimalText = "0";
                Add(ExprSeverity.Warning, ExprDiagnosticCode.OctalLiteral,
                    $"Číslo s vedúcou nulou sa číta osmičkovo: {tok.Text} = {tok.Value}", n,
                    fix: TextFix.Single($"Zapísať desiatkovo: {decimalText}", n.Start, n.Length, decimalText));
            }

            if (tok.TrainType is { } tt)
            {
                if (options.Symbols?.TrainTypeKeys is not null && !tt.FromTrTypes)
                    Add(ExprSeverity.Warning, ExprDiagnosticCode.TrainTypeNotInTrTypes,
                        $"Druh vlaku {tt.Key} nie je zavedený v TrTypes.txt (použije sa zabudovaný druh {ExprTrainTypes.NameOf(tt.Index)})", n,
                        $"Zaviesť druh {ExprTrainTypes.NameOf(tt.Index)} v TrTypes.txt alebo použiť druh, ktorý grafikon má");
                else if (!tt.Exact)
                {
                    var exact = ExactTypeName(tt);
                    var prefix = tok.Text[..(tok.Text.Length - tt.Key.Length)];
                    Add(ExprSeverity.Info, ExprDiagnosticCode.TrainTypeInexactMatch,
                        $"Druh vlaku {tt.Key} sa našiel len bez ohľadu na veľkosť písmen alebo diakritiku ({exact})", n,
                        fix: exact is null ? null : TextFix.Single($"Zapísať presne: {prefix}{exact}", n.Start, n.Length, prefix + exact));
                }
            }
        }

        /// <summary>Presny zapis kluca druhu vlaku (z TrTypes.txt alebo zabudovany nazov).</summary>
        private string? ExactTypeName(ExprTrainTypeMatch tt)
        {
            if (tt.FromTrTypes && options.Symbols?.TrainTypeKeys is { } keys)
                return keys.FirstOrDefault(k => k.Value == tt.Index && string.Equals(k.Key, tt.Key, StringComparison.OrdinalIgnoreCase)).Key
                       ?? keys.FirstOrDefault(k => k.Value == tt.Index).Key;
            return ExprTrainTypes.NameOf(tt.Index);
        }

        private void VisitFunction(ExprFunctionNode f)
        {
            var info = f.Function;
            if (options.ReportContextDependent && info.ContextDependent)
                Add(ExprSeverity.Info, ExprDiagnosticCode.ContextDependentFunction,
                    f.Canonical == ExprFunction.ZPOZDENI
                        ? "ZPOZDENI je meškanie príchodu na príchodovej tabuli, inak väčšie z oboch meškaní – jednoznačnejšie je ZPOZDENIPRIJ alebo ZPOZDENIODJ"
                        : "VYLUKAZDE je výluka príchodu na príchodovej tabuli, inak ktorákoľvek výluka – jednoznačnejšie je PRIZNAK(Prizn_VylP) alebo PRIZNAK(Prizn_VylO)",
                    f);

            var symbols = options.Symbols;
            if (symbols is null || f.Argument is null) return;

            switch (info.ArgMeaning)
            {
                case ExprArgMeaning.StationId when f.Argument is ExprNumberNode { Source: ExprNumberSource.Literal } id:
                    if (symbols.StationExists(id.Value) == false)
                        Add(ExprSeverity.Warning, ExprDiagnosticCode.UnknownStation,
                            $"Stanica {id.Value} nie je v grafikone", id, "Skontrolovať ID stanice (Stanice.txt, trasy vlakov)");
                    break;

                case ExprArgMeaning.TrackName when f.Argument is ExprStringNode track:
                    if (track.Value.Length > 0 && symbols.TrackExists(track.Value) == false)
                        Add(ExprSeverity.Warning, ExprDiagnosticCode.UnknownTrack,
                            $"Koľaj \"{track.Value}\" nie je v Pozice.txt", track, "Použiť názov koľaje z Pozice.txt");
                    break;

                case ExprArgMeaning.OperatorName when f.Argument is ExprStringNode op:
                    if (symbols.OperatorExists(op.Value) == false)
                        Add(ExprSeverity.Warning, ExprDiagnosticCode.UnknownOperator,
                            $"Dopravca \"{op.Value}\" nie je vo Vlastnik.txt", op, "Použiť názov dopravcu z Vlastnik.txt");
                    break;
            }
        }

        private void VisitBinary(ExprBinaryNode b)
        {
            if (b.IsComparison)
            {
                var mask = IsMaskCall(b.Left) && IsNonZeroLiteral(b.Right) ? b.Left
                    : IsMaskCall(b.Right) && IsNonZeroLiteral(b.Left) ? b.Right : null;
                if (mask is not null)
                {
                    var negated = b.Operator is ExprTokenKind.NotEqual;
                    var replacement = negated ? $"!{Text(mask)}" : Text(mask);
                    Add(ExprSeverity.Warning, ExprDiagnosticCode.MaskComparedToNumber,
                        "PRIZNAK(x) vracia masku príznakov, nie 0/1 – porovnávajte s 0 alebo použite bez porovnania", b,
                        fix: TextFix.Single($"Nahradiť porovnanie: {replacement}", b.Start, b.Length, replacement));
                }
            }

            // INISS viaze sprava: a op1 (b op2 c). Zvycajna priorita by dala (a op1 b) op2 c, ak op1 viaze silnejsie -
            // len vtedy sa vysledky lisia (pri rovnakej skupine je poradie jedno, pri slabsom op1 sa zhoduju).
            if (b is { IsConnective: true, Right: ExprBinaryNode { IsConnective: true } r } && Rank(b.Operator) > Rank(r.Operator))
            {
                Add(ExprSeverity.Warning, ExprDiagnosticCode.MixedLogicalOperators,
                    $"Spojky {ExprMessages.Operator(b.Operator)} a {ExprMessages.Operator(r.Operator)} majú v INISSe rovnakú prioritu a viažu sprava: "
                    + $"{Text(b.Left)} {ExprMessages.Operator(b.Operator)} ({Text(r.Left)} {ExprMessages.Operator(r.Operator)} {Text(r.Right)}) – doplňte zátvorky",
                    b, fix: Parenthesize($"Uzátvorkovať: ({Text(b.Left)} {ExprMessages.Operator(b.Operator)} {Text(r.Left)}) {ExprMessages.Operator(r.Operator)} …", b.Left, r.Left));
            }

            if (b.Right is ExprBinaryNode rs && IsOrderSensitive(b.Operator, rs.Operator))
            {
                Add(ExprSeverity.Warning, ExprDiagnosticCode.RightAssociativeArithmetic,
                    $"INISS počíta sprava: {Text(b.Left)} {ExprMessages.Operator(b.Operator)} ({Text(rs.Left)} {ExprMessages.Operator(rs.Operator)} {Text(rs.Right)}) – doplňte zátvorky",
                    b, fix: Parenthesize($"Uzátvorkovať zľava: ({Text(b.Left)} {ExprMessages.Operator(b.Operator)} {Text(rs.Left)}) {ExprMessages.Operator(rs.Operator)} …", b.Left, rs.Left));
            }

            if (b.Operator is ExprTokenKind.Slash or ExprTokenKind.Percent
                && ExprEvaluator.TryFoldConstant(b.Right, out var divisor) && divisor == 0)
                Add(ExprSeverity.Warning, ExprDiagnosticCode.DivisionByZero,
                    "Delenie nulou – INISS pri vyhodnotení spadne", b.Right, "Opraviť deliteľa");
        }

        private void VisitUnary(ExprUnaryNode u)
        {
            switch (u.Operator)
            {
                case ExprTokenKind.Not or ExprTokenKind.NotWord or ExprTokenKind.BitNot
                    when u.Operand is ExprBinaryNode { IsComparison: true } cmp:
                    Add(ExprSeverity.Info, ExprDiagnosticCode.NegationOfComparison,
                        $"Negácia platí na celé porovnanie: {ExprMessages.Operator(u.Operator)}({Text(cmp)})", u,
                        fix: Parenthesize($"Uzátvorkovať ľavú stranu: ({ExprMessages.Operator(u.Operator)}{Text(cmp.Left)}) …", u, cmp.Left));
                    break;

                case ExprTokenKind.Minus or ExprTokenKind.Plus
                    when u.Operand is ExprBinaryNode { Operator: ExprTokenKind.Plus or ExprTokenKind.Minus } sum:
                    Add(ExprSeverity.Warning, ExprDiagnosticCode.SignOfSum,
                        $"Znamienko platí na celý súčet: {ExprMessages.Operator(u.Operator)}({Text(sum)})", u,
                        fix: Parenthesize($"Uzátvorkovať prvý člen: ({ExprMessages.Operator(u.Operator)}{Text(sum.Left)}) …", u, sum.Left));
                    break;
            }
        }

        /// <summary>
        ///     Ci pri <c>a op1 (b op2 c)</c> zalezi na tom, ze INISS viaze sprava (lava asociativita by dala iny vysledok).
        /// </summary>
        private static bool IsOrderSensitive(ExprTokenKind outer, ExprTokenKind inner) => outer switch
        {
            ExprTokenKind.Minus => inner is ExprTokenKind.Plus or ExprTokenKind.Minus,
            ExprTokenKind.Slash or ExprTokenKind.Percent => inner is ExprTokenKind.Star or ExprTokenKind.Slash or ExprTokenKind.Percent,
            ExprTokenKind.Star => inner is ExprTokenKind.Slash or ExprTokenKind.Percent,
            _ => false
        };

        private static bool IsMaskCall(ExprNode n) =>
            n is ExprFunctionNode { Canonical: ExprFunction.PRIZNAK, Argument: not null };

        private static bool IsNonZeroLiteral(ExprNode n) =>
            n is ExprNumberNode { Source: ExprNumberSource.Literal, Value: not 0 };

        /// <summary>Zvycajna priorita spojok (C): &amp; &gt; ^ &gt; | &gt; &amp;&amp; &gt; ||; vyssie cislo = viaze silnejsie.</summary>
        private static int Rank(ExprTokenKind op) => op switch
        {
            ExprTokenKind.BitAnd => 5,
            ExprTokenKind.BitXor => 4,
            ExprTokenKind.BitOr => 3,
            ExprTokenKind.And or ExprTokenKind.AndWord => 2,
            ExprTokenKind.Or or ExprTokenKind.OrWord => 1,
            _ => 0
        };

        private string Text(ExprNode n) => parse.Text.Substring(n.Start, n.Length);
    }
}
