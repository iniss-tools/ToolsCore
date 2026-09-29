using System.Globalization;
using ToolsCore.Expressions;
using ToolsCore.Properties;

namespace ToolsCore.TabTab;

/// <summary>
/// Kod hlasenia kontroly sekcie TabTab. Kody <c>Iniss*</c> zodpovedaju chybam, ktore INISS zapisuje do logu.
/// </summary>
public enum TabTabDiagnosticCode
{
    /// <summary><c>unknown magic item</c> - nezname meno udalosti za <c>=</c>.</summary>
    InissUnknownMagicItem,

    /// <summary><c>the items count is uneven</c> - <c>#SWITCH</c> s neparnym poctom poloziek.</summary>
    InissItemsCountUneven,

    /// <summary><c>the items count is not multiple of 3</c>.</summary>
    InissItemsCountNotMultipleOf3,

    /// <summary>Chyba prekladu podmienky (text chyby je text INISSu).</summary>
    InissConditionError,

    /// <summary><c>the position … was not found</c> - kolaj z <c>#POZODJ_</c> nie je v Pozice.txt.</summary>
    InissUnknownTrack,

    /// <summary>Varovanie z kontroly vyrazu (kod je v <see cref="TabTabDiagnostic.ExprCode"/>).</summary>
    Condition,

    /// <summary><c>%meno%</c> odkazuje na stlpec, ktory tabula nema.</summary>
    UnknownColumn,

    /// <summary>Pravidlo s udalostou bez poloziek.</summary>
    EmptyEventList,

    /// <summary>Polozka za podmienkou, ktora je vzdy splnena - nikdy sa nepouzije.</summary>
    UnreachableItem,

    /// <summary>Hlavicka <c>[…]</c> vnutri textu sekcie - pri ulozeni by vznikla nova sekcia.</summary>
    SectionHeaderInside,

    /// <summary><c>[</c> bez <c>]</c> - INISS riadok ignoruje.</summary>
    BadSectionHeader,

    /// <summary>Riadok bez <c>=</c> s neznamou volbou - INISS ho ignoruje.</summary>
    UnknownOption,

    /// <summary>Prava strana pravidla je prazdna.</summary>
    EmptyRight,

    /// <summary>Udalost zapisana inou velkostou pismen (<c>#switch</c>) - INISS ju nepozna.</summary>
    EventCase,

    /// <summary>Neparny pocet uvodzoviek v texte.</summary>
    UnbalancedQuotes,

    /// <summary><c>{…}</c>, ktore nie je platny zapis pisma na konci textu.</summary>
    BadFontCode,

    /// <summary><c>{n}</c> vnutri uvodzoviek - je textom, nie pismom.</summary>
    FontInsideQuotes,

    /// <summary>Za <c>\</c> na konci riadka nieco nasleduje (komentar, medzery) - INISS pokracovanie nerozpozna.</summary>
    BrokenContinuation,

    /// <summary>Komentarovy riadok vnutri viacriadkoveho pravidla - INISS nim pravidlo ukonci.</summary>
    CommentInsideRule
}

/// <summary>
/// Hlasenie kontroly sekcie TabTab; pozicie su v texte sekcie.
/// </summary>
/// <param name="Severity">Zavaznost.</param>
/// <param name="Code">Kod.</param>
/// <param name="Message">Text.</param>
/// <param name="Start">Index prveho znaku.</param>
/// <param name="Length">Dlzka (0 = bodove).</param>
/// <param name="LineIndex">Cislo fyzickeho riadka (od 0), na ktorom hlasenie zacina.</param>
public sealed record TabTabDiagnostic(ExprSeverity Severity, TabTabDiagnosticCode Code, string Message, int Start, int Length, int LineIndex)
{
    /// <summary>Kod z kontroly vyrazu pri <see cref="TabTabDiagnosticCode.Condition"/> a <see cref="TabTabDiagnosticCode.InissConditionError"/>.</summary>
    public ExprDiagnosticCode? ExprCode { get; init; }

    /// <summary>Odporucane riesenie (text pre stlpec „Riešenie“); <see langword="null"/>, ak nie je.</summary>
    public string? Suggestion { get; init; }

    /// <summary>Automaticka oprava textu sekcie, ak sa da navrhnut; pozicie su v texte sekcie.</summary>
    public TextFix? Fix { get; init; }

    /// <summary>Kod ako text pre zobrazenie (pri hlaseni z vyrazu jeho kod).</summary>
    public string CodeName => ExprCode?.ToString() ?? Code.ToString();

    /// <summary>Index za poslednym znakom.</summary>
    public int End => Start + Length;

    /// <summary>Ci ide o chybu (INISS pravidlo odmietne alebo zapise do logu).</summary>
    public bool IsError => Severity == ExprSeverity.Error;

    /// <inheritdoc/>
    public override string ToString() => $"{Severity} r.{LineIndex + 1} [{Start}+{Length}] {Message}";
}

/// <summary>
/// Nastavenia kontroly sekcie.
/// </summary>
public sealed class TabTabValidationOptions
{
    /// <summary>Symboly grafikonu pre vyrazy a kolaje; <see langword="null"/> vypne kontroly proti datam.</summary>
    public IExprSymbolProvider? Symbols { get; init; }

    /// <summary>
    /// Mena stlpcov katalogovych tabul, ktore sekciu pouzivaju (pre <c>%meno%</c>);
    /// <see langword="null"/> kontrolu vypne.
    /// </summary>
    public IReadOnlyCollection<string>? ColumnNames { get; init; }

    /// <summary>Ci hlasit funkcie zavisle od kontextu (<c>VYLUKAZDE</c>, <c>ZPOZDENI</c>).</summary>
    public bool ReportContextDependent { get; init; }
}

/// <summary>
/// Vysledok kontroly sekcie.
/// </summary>
/// <param name="Section">Rozobrana sekcia.</param>
/// <param name="Diagnostics">Hlasenia zoradene podla pozicie.</param>
public sealed record TabTabValidationResult(TabTabSection Section, IReadOnlyList<TabTabDiagnostic> Diagnostics)
{
    /// <summary>Pocet chyb.</summary>
    public int ErrorCount => Diagnostics.Count(d => d.Severity == ExprSeverity.Error);

    /// <summary>Pocet varovani.</summary>
    public int WarningCount => Diagnostics.Count(d => d.Severity == ExprSeverity.Warning);
}

/// <summary>
/// Kontrola sekcie TabTab: to, co hlasi INISS pri nacitani, plus kontroly GVDEditora.
/// </summary>
public static class TabTabValidator
{
    /// <summary>
    /// Rozoberie a skontroluje text sekcie.
    /// </summary>
    public static TabTabValidationResult Validate(string text, TabTabValidationOptions? options = null)
    {
        options ??= new TabTabValidationOptions();
        var section = TabTabSection.Parse(text);
        var list = new List<TabTabDiagnostic>();
        _text = section.Text;

        var brokenLines = CheckContinuations(section.Text, list);
        brokenLines.UnionWith(CheckCommentsInsideRules(section.Text, list));

        foreach (var line in section.Lines)
        {
            // riadok s pokazenym pokracovanim uz ma svoje hlasenie - "volba sekcie" by bola len nasledok
            if (line.Kind == TabTabLineKind.Options && brokenLines.Contains(line.LineIndex))
                continue;

            switch (line.Kind)
            {
                case TabTabLineKind.SectionHeader:
                    list.Add(New(ExprSeverity.Warning, TabTabDiagnosticCode.SectionHeaderInside,
                        string.Format(CultureInfo.CurrentCulture, Resources.Ttv_HeaderInSection, line.Text), line.Span, line,
                        suggestion: Resources.Ttv_HeaderInSection_Fix));
                    break;

                case TabTabLineKind.BadSectionHeader:
                    list.Add(New(ExprSeverity.Warning, TabTabDiagnosticCode.BadSectionHeader,
                        Resources.Ttv_BracketUnclosed, line.Span, line, suggestion: Resources.Ttv_BracketUnclosed_Fix));
                    break;

                case TabTabLineKind.Options:
                    foreach (var o in line.Options)
                        if (!o.Equals(TabTabSectionParser.OptionIgnoreCase, StringComparison.OrdinalIgnoreCase)
                            && !o.Equals(TabTabSectionParser.OptionViewValues, StringComparison.OrdinalIgnoreCase))
                            list.Add(New(ExprSeverity.Warning, TabTabDiagnosticCode.UnknownOption,
                                string.Format(CultureInfo.CurrentCulture, Resources.Ttv_LineWithoutEquals, o), line.LeftSpan, line,
                                suggestion: Resources.Ttv_LineWithoutEquals_Fix));
                    break;

                case TabTabLineKind.Rule:
                    CheckRule(line, options, list);
                    break;
            }
        }

        _text = null;
        list.Sort((a, b) => a.Start != b.Start ? a.Start.CompareTo(b.Start) : b.Severity.CompareTo(a.Severity));
        return new TabTabValidationResult(section, list);
    }

    [ThreadStatic] private static string? _text;

    /// <summary>
    /// Najde fyzicke riadky, kde za <c>\</c> nasleduje este nieco (medzery, komentar). INISS spaja riadky
    /// len vtedy, ked je <c>\</c> uplne posledny znak - inak riadok spracuje samostatne a zvysok pravidla
    /// na dalsich riadkoch sa rozpadne.
    /// </summary>
    private static HashSet<int> CheckContinuations(string text, List<TabTabDiagnostic> list)
    {
        var broken = new HashSet<int>();
        var newline = text.Contains("\r\n") ? "\r\n" : "\n";
        var lineIndex = 0;
        var pos = 0;
        var ruleStartPos = 0; // zaciatok logickeho riadka (pravidla), do ktoreho aktualny fyzicky riadok patri
        var inside = false;
        while (pos < text.Length)
        {
            var nl = text.IndexOf('\n', pos);
            var end = nl < 0 ? text.Length : nl;
            var contentEnd = end;
            if (contentEnd > pos && text[contentEnd - 1] == '\r') contentEnd--;

            if (!inside) ruleStartPos = pos;

            var slash = FindDanglingBackslash(text, pos, contentEnd);
            if (slash >= 0)
            {
                var tail = text[(slash + 1)..contentEnd];
                var comment = tail.TrimStart(' ', '\t');
                var isComment = comment.StartsWith(';');
                // komentar patri pred prvy riadok pravidla - vnutri viacriadkoveho pravidla by INISS pravidlo ukoncil
                var fix = isComment
                    ? new TextFix(Resources.Ttv_MoveCommentBeforeRule, [
                        new TextEdit(slash + 1, contentEnd - slash - 1, ""),
                        new TextEdit(ruleStartPos, 0, comment + newline)
                    ])
                    : TextFix.Single(Resources.Ttv_RemoveSpacesAfterSlash, slash + 1, contentEnd - slash - 1, "");
                list.Add(new TabTabDiagnostic(ExprSeverity.Warning, TabTabDiagnosticCode.BrokenContinuation,
                    (isComment ? Resources.Ttv_CommentAfterSlash : Resources.Ttv_SpacesAfterSlash)
                    + Resources.Ttv_SlashNotLast,
                    slash, contentEnd - slash, lineIndex) { Suggestion = fix.Title, Fix = fix });
                broken.Add(lineIndex);
                inside = true; // autor pokracovanie chcel - dalsie riadky su stale to iste pravidlo
            }
            else
            {
                var first = pos;
                while (first < contentEnd && text[first] is ' ' or '\t') first++;
                var isCommentLine = first < contentEnd && text[first] == ';';
                var endsWithBackslash = contentEnd > pos && text[contentEnd - 1] == '\\';
                inside = !isCommentLine && endsWithBackslash;
            }

            if (nl < 0) break;
            pos = nl + 1;
            lineIndex++;
        }
        return broken;
    }

    /// <summary>
    /// Index <c>\</c>, ktore malo byt pokracovanim, ale nie je poslednym znakom riadka (za nim su len
    /// medzery alebo medzery a komentar <c>;…</c>); -1, ak taky nie je. Komentarove riadky sa preskocia.
    /// </summary>
    private static int FindDanglingBackslash(string text, int start, int end)
    {
        var first = start;
        while (first < end && text[first] is ' ' or '\t') first++;
        if (first >= end || text[first] == ';') return -1;

        // koniec obsahu bez komentara ;… (neescapovany, mimo uvodzoviek)
        var quoted = false;
        var contentEnd = end;
        for (var i = first; i < end; i++)
        {
            var c = text[i];
            if (c == '\\') { i++; continue; }
            if (c == '"') quoted = !quoted;
            else if (c == ';' && !quoted) { contentEnd = i; break; }
        }

        var last = contentEnd - 1;
        while (last >= first && text[last] is ' ' or '\t') last--;
        if (last < first || text[last] != '\\') return -1;
        if (last == end - 1) return -1; // '\' je naozaj posledny znak - v poriadku

        // escapovane \\ (parny pocet) nie je pokracovanie
        var run = 0;
        for (var i = last; i >= first && text[i] == '\\'; i--) run++;
        return run % 2 == 1 ? last : -1;
    }

    /// <summary>
    /// Najde komentarove riadky vnutri viacriadkoveho pravidla. INISS taky riadok nikdy nespoji -
    /// pravidlo nim ukonci (a zvysok na dalsich riadkoch sa rozpadne). Vracia indexy prvych
    /// riadkov postihnutych pravidiel.
    /// </summary>
    private static HashSet<int> CheckCommentsInsideRules(string text, List<TabTabDiagnostic> list)
    {
        var broken = new HashSet<int>();
        var newline = text.Contains("\r\n") ? "\r\n" : "\n";
        var lineIndex = 0;
        var pos = 0;
        var ruleStartPos = -1;
        var ruleStartLine = -1;
        var inside = false;

        while (pos < text.Length)
        {
            var nl = text.IndexOf('\n', pos);
            var end = nl < 0 ? text.Length : nl;
            var contentEnd = end;
            if (contentEnd > pos && text[contentEnd - 1] == '\r') contentEnd--;

            var first = pos;
            while (first < contentEnd && text[first] is ' ' or '\t') first++;
            var isComment = first < contentEnd && text[first] == ';';
            var endsWithBackslash = contentEnd > pos && text[contentEnd - 1] == '\\';

            if (inside && isComment)
            {
                var comment = text[first..contentEnd];
                var lineEnd = nl < 0 ? end : nl + 1; // vratane konca riadka
                var fix = new TextFix(Resources.Ttv_MoveCommentBeforeRuleStart, [
                    new TextEdit(pos, lineEnd - pos, ""),
                    new TextEdit(ruleStartPos, 0, comment + newline)
                ]);
                list.Add(new TabTabDiagnostic(ExprSeverity.Warning, TabTabDiagnosticCode.CommentInsideRule,
                    Resources.Ttv_CommentInsideRule,
                    first, contentEnd - first, lineIndex) { Suggestion = fix.Title, Fix = fix });
                broken.Add(ruleStartLine);
                inside = false;
            }
            else if (!inside && !isComment && endsWithBackslash)
            {
                inside = true;
                ruleStartPos = pos;
                ruleStartLine = lineIndex;
            }
            else if (inside && !endsWithBackslash)
            {
                inside = false;
            }

            if (nl < 0) break;
            pos = nl + 1;
            lineIndex++;
        }
        return broken;
    }

    /// <summary>Cislo fyzickeho riadka (od 0) pre poziciu v texte sekcie.</summary>
    private static int LineOf(int pos)
    {
        var t = _text ?? "";
        var line = 0;
        for (var i = 0; i < pos && i < t.Length; i++)
            if (t[i] == '\n') line++;
        return line;
    }

    private static TabTabDiagnostic New(ExprSeverity sev, TabTabDiagnosticCode code, string msg, TabTabSpan span, TabTabLine line,
        ExprDiagnosticCode? exprCode = null, string? suggestion = null, TextFix? fix = null) =>
        new(sev, code, msg, span.Start, span.Length, Math.Max(line.LineIndex, LineOf(span.Start)))
        {
            ExprCode = exprCode, Suggestion = suggestion ?? fix?.Title, Fix = fix
        };

    private static void CheckRule(TabTabLine line, TabTabValidationOptions options, List<TabTabDiagnostic> list)
    {
        if (line.Right.Length == 0)
        {
            list.Add(New(ExprSeverity.Warning, TabTabDiagnosticCode.EmptyRight,
                Resources.Ttv_EmptyRight, line.RightSpan, line,
                suggestion: Resources.Ttv_EmptyRight_Fix));
            return;
        }

        switch (line.Event)
        {
            case TabTabEventKind.None:
                CheckTextSyntax(line.Left, line.LeftSpan, line, list);
                CheckTextSyntax(line.Right, line.RightSpan, line, list);
                CheckColumnRefs(line.Left, line.LeftSpan, line, options, list);
                return;

            case TabTabEventKind.Unknown:
            {
                var upper = line.Right.ToUpperInvariant();
                var known = upper is "#VYLUKA" or "#ODKLON" or "#SWITCH" or "#MERGE" or "#MERGE2" || upper.StartsWith("#POZODJ_", StringComparison.Ordinal);
                list.Add(known
                    ? New(ExprSeverity.Error, TabTabDiagnosticCode.EventCase,
                        string.Format(CultureInfo.CurrentCulture, Resources.Ttv_MagicItemCase, line.Right, upper), line.RightSpan, line,
                        fix: TextFix.Single(string.Format(CultureInfo.CurrentCulture, Resources.Ttv_RewriteTo, upper), line.RightSpan.Start, line.RightSpan.Length, upper))
                    : New(ExprSeverity.Error, TabTabDiagnosticCode.InissUnknownMagicItem,
                        $"unknown magic item {line.Right}", line.RightSpan, line,
                        suggestion: Resources.Ttv_MagicItem_Fix));
                return;
            }

            case TabTabEventKind.Vyluka or TabTabEventKind.Odklon:
                CheckTextSyntax(line.Left, line.LeftSpan, line, list);
                CheckColumnRefs(line.Left, line.LeftSpan, line, options, list);
                return;

            case TabTabEventKind.PozOdj:
            {
                var track = line.PozOdjTrack ?? "";
                if (options.Symbols?.TrackExists(track) == false)
                    list.Add(New(ExprSeverity.Error, TabTabDiagnosticCode.InissUnknownTrack,
                        string.Format(CultureInfo.CurrentCulture, Resources.Ttv_PositionNotFound, track), line.RightSpan, line,
                        suggestion: Resources.Ttv_UseTrackName));
                CheckColumnRefs(line.Left, line.LeftSpan, line, options, list);
                return;
            }
        }

        // #SWITCH / #MERGE / #MERGE2
        var items = line.Items;
        var step = line.Event == TabTabEventKind.Switch ? 2 : 3;

        if (items is [{ Text.Length: 0 }])
        {
            list.Add(New(ExprSeverity.Warning, TabTabDiagnosticCode.EmptyEventList,
                string.Format(CultureInfo.CurrentCulture, Resources.Ttv_NoItems, line.Right), line.RightSpan, line, suggestion: Resources.Ttv_NoItems_Fix));
            return;
        }

        if (items.Count % step != 0)
        {
            list.Add(step == 2
                ? New(ExprSeverity.Error, TabTabDiagnosticCode.InissItemsCountUneven,
                    string.Format(CultureInfo.CurrentCulture, Resources.Ttv_ItemsUneven, line.Right, items.Count), line.LeftSpan, line,
                    suggestion: Resources.Ttv_ItemsUneven_Fix)
                : New(ExprSeverity.Error, TabTabDiagnosticCode.InissItemsCountNotMultipleOf3,
                    string.Format(CultureInfo.CurrentCulture, Resources.Ttv_ItemsNotTriple, line.Right, items.Count), line.LeftSpan, line,
                    suggestion: Resources.Ttv_ItemsNotTriple_Fix));
        }

        var alwaysTrueSeen = false;
        foreach (var item in items)
        {
            if (item.IsCondition)
            {
                if (alwaysTrueSeen && line.Event == TabTabEventKind.Switch)
                {
                    list.Add(New(ExprSeverity.Warning, TabTabDiagnosticCode.UnreachableItem,
                        Resources.Ttv_ItemAfterAlwaysTrue, item.Span, line,
                        suggestion: Resources.Ttv_ItemAfterAlwaysTrue_Fix));
                }

                CheckCondition(item, line, options, list, out var alwaysTrue);
                alwaysTrueSeen |= alwaysTrue;
            }
            else
            {
                CheckTextSyntax(item.Text, item.Span, line, list);
                if (!item.IsSeparator)
                    CheckColumnRefs(item.Text, item.Span, line, options, list);
            }
        }
    }

    private static void CheckCondition(TabTabItem item, TabTabLine line, TabTabValidationOptions options,
        List<TabTabDiagnostic> list, out bool alwaysTrue)
    {
        alwaysTrue = false;
        var r = ExprValidator.Validate(item.Text, new ExprValidationOptions
        {
            Context = ExprContext.Condition,
            Symbols = options.Symbols,
            IsCondition = true,
            ReportContextDependent = options.ReportContextDependent
        });

        foreach (var d in r.Diagnostics)
        {
            var span = new TabTabSpan(item.Span.Start + d.Start, d.Length);
            var fix = d.Fix?.Shift(item.Span.Start);
            if (d.IsError)
            {
                // INISS: "Err: …TABTAB, section X, #SWITCH/<poradie>: <chyba>"
                list.Add(New(ExprSeverity.Error, TabTabDiagnosticCode.InissConditionError,
                    $"{line.Right}/{item.Index + 1}: {d.Message}", d.Length == 0 ? TabTabSpan.At(span.Start) : span, line, d.Code,
                    d.Suggestion ?? ErrorSuggestion(d.Code), fix));
            }
            else
            {
                list.Add(New(d.Severity, TabTabDiagnosticCode.Condition, d.Message, span, line, d.Code, d.Suggestion, fix));
            }
        }

        if (r.Root is not null && ExprEvaluator.TryFoldConstant(r.Root, out var v) && v != 0)
            alwaysTrue = true;
    }

    /// <summary>
    /// Skontroluje zapis textu (uvodzovky, pismo <c>{n}</c>).
    /// </summary>
    private static void CheckTextSyntax(string raw, TabTabSpan span, TabTabLine line, List<TabTabDiagnostic> list)
    {
        foreach (var issue in TabTabText.Inspect(raw))
        {
            var at = new TabTabSpan(span.Start + issue.Start, issue.Length);
            switch (issue.Kind)
            {
                case TabTabText.IssueKind.UnbalancedQuotes:
                    list.Add(New(ExprSeverity.Warning, TabTabDiagnosticCode.UnbalancedQuotes,
                        Resources.Ttv_OddQuotes, at, line,
                        suggestion: Resources.Ttv_OddQuotes_Fix));
                    break;
                case TabTabText.IssueKind.BadFontCode:
                    list.Add(New(ExprSeverity.Warning, TabTabDiagnosticCode.BadFontCode,
                        string.Format(CultureInfo.CurrentCulture, Resources.Ttv_NotFont, raw.Substring(issue.Start, issue.Length)), at, line,
                        suggestion: Resources.Ttv_NotFont_Fix));
                    break;
                case TabTabText.IssueKind.FontInsideQuotes:
                    list.Add(New(ExprSeverity.Warning, TabTabDiagnosticCode.FontInsideQuotes,
                        string.Format(CultureInfo.CurrentCulture, Resources.Ttv_FontInQuotes, raw.Substring(issue.Start, issue.Length)), at, line,
                        suggestion: Resources.Ttv_FontInQuotes_Fix));
                    break;
            }
        }
    }

    /// <summary>Odporucanie k chybe prekladaca INISSu.</summary>
    private static string? ErrorSuggestion(ExprDiagnosticCode code) => code switch
    {
        ExprDiagnosticCode.InissUnknownSymbol => Resources.Ttv_Fix_UnknownSymbol,
        ExprDiagnosticCode.InissExpected => Resources.Ttv_Fix_Expected,
        ExprDiagnosticCode.InissUnexpected => Resources.Ttv_Fix_Unexpected,
        ExprDiagnosticCode.InissUnterminatedString => Resources.Ttv_Fix_UnterminatedString,
        ExprDiagnosticCode.InissNumberExpected => Resources.Ttv_Fix_NumberExpected,
        ExprDiagnosticCode.InissBadDateFormat => Resources.Ttv_Fix_BadDate,
        ExprDiagnosticCode.InissBadTimeFormat => Resources.Ttv_Fix_BadTime,
        _ => null
    };

    /// <summary>
    /// Skontroluje odkazy <c>%meno%</c> na stlpce tabule.
    /// </summary>
    private static void CheckColumnRefs(string text, TabTabSpan span, TabTabLine line, TabTabValidationOptions options, List<TabTabDiagnostic> list)
    {
        if (options.ColumnNames is null) return;

        // INISS berie %meno% len ako cely text polozky (po dekodovani); tu staci cely orezany text
        var decoded = TabTabText.Decode(text).Text;
        if (decoded.Length <= 2 || decoded[0] != '%' || decoded[^1] != '%') return;

        var name = decoded[1..^1];
        if (options.ColumnNames.Any(c => string.Equals(c, name, StringComparison.OrdinalIgnoreCase))) return;

        list.Add(New(ExprSeverity.Warning, TabTabDiagnosticCode.UnknownColumn,
            string.Format(CultureInfo.CurrentCulture, Resources.Ttv_ColumnUnknown, name), span, line,
            suggestion: Resources.Ttv_ColumnUnknown_Fix));
    }
}
