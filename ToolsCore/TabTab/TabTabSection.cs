namespace ToolsCore.TabTab;

/// <summary>
///     Druh logickeho riadka sekcie TabTab (klasifikacia INISSu).
/// </summary>
public enum TabTabLineKind
{
    /// <summary>Prazdny riadok.</summary>
    Empty,

    /// <summary>Komentar <c>; …</c>.</summary>
    Comment,

    /// <summary>Hlavicka sekcie <c>[Nazov]</c> (v texte sekcie by nemala byt - GVDEditor sekcie drzi zvlast).</summary>
    SectionHeader,

    /// <summary><c>[</c> bez <c>]</c> - INISS riadok ignoruje.</summary>
    BadSectionHeader,

    /// <summary>Pravidlo <c>lava = prava</c>.</summary>
    Rule,

    /// <summary>Riadok bez <c>=</c> - volby sekcie (<c>IgnoreCase</c>, <c>ViewValues</c>).</summary>
    Options
}

/// <summary>
///     Udalost na pravej strane pravidla.
/// </summary>
public enum TabTabEventKind
{
    /// <summary>Obycajne pravidlo - prava strana je text INISSu.</summary>
    None,

    /// <summary><c>#SWITCH</c> - dvojice podmienka, text.</summary>
    Switch,

    /// <summary><c>#MERGE</c> - trojice podmienka, oddelovac, text.</summary>
    Merge,

    /// <summary><c>#MERGE2</c> - trojice podmienka, oddelovac, text.</summary>
    Merge2,

    /// <summary><c>#VYLUKA</c>.</summary>
    Vyluka,

    /// <summary><c>#ODKLON</c>.</summary>
    Odklon,

    /// <summary><c>#POZODJ_&lt;kolaj&gt;</c>.</summary>
    PozOdj,

    /// <summary>Ine meno zacinajuce <c>#</c> - INISS hlasi <c>unknown magic item</c>.</summary>
    Unknown
}

/// <summary>
///     Usek textu sekcie (fyzicke pozicie v texte, ako ho drzi editor).
/// </summary>
/// <param name="Start">Index prveho znaku.</param>
/// <param name="Length">Dlzka.</param>
public readonly record struct TabTabSpan(int Start, int Length)
{
    /// <summary>Index za poslednym znakom.</summary>
    public int End => Start + Length;

    /// <summary>Prazdny usek na danej pozicii.</summary>
    public static TabTabSpan At(int pos) => new(pos, 0);
}

/// <summary>
///     Polozka lavej strany pravidla s udalostou (<c>#SWITCH</c>, <c>#MERGE</c>, <c>#MERGE2</c>).
/// </summary>
public sealed class TabTabItem
{
    /// <summary>Poradie polozky v zozname (od 0).</summary>
    public int Index { get; init; }

    /// <summary>Text polozky tak, ako je v subore (bez okolitych medzier).</summary>
    public string Text { get; init; } = "";

    /// <summary>Usek polozky v texte sekcie.</summary>
    public TabTabSpan Span { get; init; }

    /// <summary>Ci je polozka podmienkou (vyrazom).</summary>
    public bool IsCondition { get; init; }

    /// <summary>Ci je polozka oddelovacom (<c>#MERGE</c>, <c>#MERGE2</c>).</summary>
    public bool IsSeparator { get; init; }

    /// <summary>Text polozky po dekodovani (<see cref="TabTabText.Decode"/>) - len pri textoch a oddelovacoch.</summary>
    public TabTabText? Decoded { get; init; }
}

/// <summary>
///     Jeden logicky riadok sekcie (po spojeni pokracovani <c>\</c>).
/// </summary>
public sealed class TabTabLine
{
    /// <summary>Druh riadka.</summary>
    public TabTabLineKind Kind { get; init; }

    /// <summary>Cislo prveho fyzickeho riadka (od 0).</summary>
    public int LineIndex { get; init; }

    /// <summary>Usek celeho logickeho riadka v texte (od prveho po posledny fyzicky riadok).</summary>
    public TabTabSpan Span { get; init; }

    /// <summary>Lava strana pravidla (text pre tabulu / zoznam poloziek), bez orezania.</summary>
    public string Left { get; init; } = "";

    /// <summary>Usek lavej strany.</summary>
    public TabTabSpan LeftSpan { get; init; }

    /// <summary>Prava strana pravidla (text INISSu / udalost), orezana sprava.</summary>
    public string Right { get; init; } = "";

    /// <summary>Usek pravej strany (orezanej).</summary>
    public TabTabSpan RightSpan { get; init; }

    /// <summary>Udalost pravej strany.</summary>
    public TabTabEventKind Event { get; init; }

    /// <summary>Kolaj pri <see cref="TabTabEventKind.PozOdj"/>.</summary>
    public string? PozOdjTrack { get; init; }

    /// <summary>Polozky lavej strany pri <c>#SWITCH</c>/<c>#MERGE</c>/<c>#MERGE2</c>.</summary>
    public IReadOnlyList<TabTabItem> Items { get; init; } = [];

    /// <summary>Pri <see cref="TabTabLineKind.Options"/>: volby (polozky oddelene ciarkou).</summary>
    public IReadOnlyList<string> Options { get; init; } = [];

    /// <summary>Text komentara alebo nazov sekcie pri prislusnych druhoch.</summary>
    public string Text { get; init; } = "";

    /// <summary>Ci je riadok pravidlom s udalostou.</summary>
    public bool IsEventRule => Kind == TabTabLineKind.Rule && Event != TabTabEventKind.None;
}

/// <summary>
///     Text polozky/pravidla po dekodovani INISSom - bez uvodzoviek, s vyriesenymi
///     <c>\x</c> a s pismom z koncoveho <c>{n}</c>.
/// </summary>
/// <param name="Text">Dekodovany text.</param>
/// <param name="Font">Cislo pisma z koncoveho <c>{n}</c>; <see langword="null"/>, ak nebolo, -1 pri <c>{@}</c> (predvolene pismo).</param>
public readonly record struct TabTabText(string Text, int? Font)
{
    /// <summary>Hodnota <see cref="Font"/> pre <c>{@}</c>.</summary>
    public const int DefaultFont = -1;

    /// <summary>
    ///     Dekoduje text ako INISS: preskoci vedúce medzery, <c>\x</c> je doslovne <c>x</c>, <c>"</c> zapina
    ///     a vypina uvodzovky (<c>""</c> vnutri je <c>"</c>), mimo uvodzoviek sa koncove medzery zahodia
    ///     a <c>{n}</c> alebo <c>{@}</c> uplne na konci je cislo pisma, nie text.
    /// </summary>
    public static TabTabText Decode(string raw)
    {
        var sb = new StringBuilder(raw.Length);
        var quoted = false;
        var keepLen = 0; // dlzka bez koncovych medzier mimo uvodzoviek
        int? font = null;

        var i = 0;
        while (i < raw.Length && char.IsWhiteSpace(raw[i])) i++;

        for (; i < raw.Length; i++)
        {
            var c = raw[i];
            if (c == '\\')
            {
                i++;
                if (i >= raw.Length) break;
                c = raw[i];
            }
            else if (c == '"')
            {
                if (quoted && i + 1 < raw.Length && raw[i + 1] == '"')
                {
                    i++; // "" vnutri uvodzoviek = "
                }
                else
                {
                    quoted = !quoted;
                    continue;
                }
            }
            else if (!quoted && c == '{' && TryFontSuffix(raw, i, out var f))
            {
                font = f;
                break;
            }

            sb.Append(c);
            if (quoted || !char.IsWhiteSpace(c))
                keepLen = sb.Length;
        }

        sb.Length = keepLen;
        return new TabTabText(sb.ToString(), font);
    }

    /// <summary>
    ///     Ci od pozicie <paramref name="i"/> (na <c>{</c>) nasleduje <c>{n}</c> alebo <c>{@}</c> a za nim uz len medzery.
    /// </summary>
    private static bool TryFontSuffix(string s, int i, out int font)
    {
        font = 0;
        var j = i + 1;
        while (j < s.Length && char.IsWhiteSpace(s[j])) j++;
        if (j >= s.Length) return false;

        if (s[j] == '@')
        {
            font = DefaultFont;
            j++;
        }
        else
        {
            var start = j;
            while (j < s.Length && (char.IsAsciiLetterOrDigit(s[j]) || (j == start && s[j] is '+' or '-'))) j++;
            if (j == start) return false;
            if (!Expressions.ExprLexer.TryStrtol(s[start..j].TrimStart('+'), out font))
            {
                // strtol by precital aspon cast; INISS berie hodnotu a pokracuje - zjednodusene: neplatne cislo = nie je pismo
                return false;
            }
        }

        while (j < s.Length && char.IsWhiteSpace(s[j])) j++;
        if (j >= s.Length || s[j] != '}') return false;
        j++;
        while (j < s.Length && char.IsWhiteSpace(s[j])) j++;
        return j >= s.Length;
    }

    /// <summary>
    ///     Druh nedostatku v zapise textu.
    /// </summary>
    public enum IssueKind
    {
        /// <summary>Neparny pocet uvodzoviek - text od poslednej uvodzovky sa berie doslovne (aj <c>{n}</c>).</summary>
        UnbalancedQuotes,

        /// <summary><c>{…}</c> mimo uvodzoviek, ktore nie je platny zapis pisma na konci - INISS ho berie ako text.</summary>
        BadFontCode,

        /// <summary><c>{n}</c> je vnutri uvodzoviek - je to text, nie pismo.</summary>
        FontInsideQuotes
    }

    /// <summary>Nedostatok v zapise textu: druh a usek v <paramref name="raw"/> (relativne k jeho zaciatku).</summary>
    public readonly record struct Issue(IssueKind Kind, int Start, int Length);

    /// <summary>
    ///     Najde v surovom zapise textu (lava strana pravidla, polozka) veci, ktore INISS sice precita,
    ///     ale skoro urcite inak, nez autor chcel.
    /// </summary>
    public static List<Issue> Inspect(string raw)
    {
        var issues = new List<Issue>();
        var quoted = false;
        var lastQuote = -1;

        for (var i = 0; i < raw.Length; i++)
        {
            var c = raw[i];
            if (c == '\\')
            {
                i++;
                continue;
            }

            if (c == '"')
            {
                if (quoted && i + 1 < raw.Length && raw[i + 1] == '"')
                {
                    i++;
                    continue;
                }
                quoted = !quoted;
                lastQuote = i;
                continue;
            }

            if (c != '{') continue;

            if (quoted)
            {
                // {cislo} v uvodzovkach - autor asi chcel pismo, ale je to text
                var close = raw.IndexOf('}', i + 1);
                if (close > i && close - i - 1 > 0 && TryFontSuffix(raw[..(close + 1)], i, out _))
                    issues.Add(new Issue(IssueKind.FontInsideQuotes, i, close - i + 1));
                continue;
            }

            if (TryFontSuffix(raw, i, out _))
                break; // platne pismo na konci

            var end = raw.IndexOf('}', i + 1);
            issues.Add(new Issue(IssueKind.BadFontCode, i, end < 0 ? raw.Length - i : end - i + 1));
            if (end < 0) break;
            i = end;
        }

        if (quoted)
        {
            // pri nesparovanych uvodzovkach je zvysok len nasledok - hlasi sa iba pricina
            issues.RemoveAll(i => i.Kind == IssueKind.FontInsideQuotes);
            issues.Insert(0, new Issue(IssueKind.UnbalancedQuotes, lastQuote, raw.Length - lastQuote));
        }

        return issues;
    }
}

/// <summary>
///     Rozobrany text jednej sekcie TabTab.
/// </summary>
public sealed class TabTabSection
{
    /// <summary>Text, z ktoreho sekcia vznikla.</summary>
    public string Text { get; init; } = "";

    /// <summary>Logicke riadky v poradi.</summary>
    public IReadOnlyList<TabTabLine> Lines { get; init; } = [];

    /// <summary>Ci ma sekcia volbu <c>IgnoreCase</c> (prava strana sa porovnava bez ohladu na velkost pismen).</summary>
    public bool IgnoreCase { get; init; }

    /// <summary>Ci ma sekcia volbu <c>ViewValues</c>.</summary>
    public bool ViewValues { get; init; }

    /// <summary>Pravidla (riadky druhu <see cref="TabTabLineKind.Rule"/>).</summary>
    public IEnumerable<TabTabLine> Rules => Lines.Where(l => l.Kind == TabTabLineKind.Rule);

    /// <summary>
    ///     Rozoberie text sekcie tak, ako ho cita INISS - spojenie riadkov s <c>\</c>,
    ///     komentare, posledne neescapovane <c>=</c> ako oddelovac, <c>;</c> ako koniec riadka.
    /// </summary>
    public static TabTabSection Parse(string text) => TabTabSectionParser.Parse(text ?? "");
}
