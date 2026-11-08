namespace ToolsCore.Iniss.TabTab;

/// <summary>
/// Text polozky/pravidla po dekodovani INISSom - bez uvodzoviek, s vyriesenymi
/// <c>\x</c> a s pismom z koncoveho <c>{n}</c>.
/// </summary>
/// <param name="Text">Dekodovany text.</param>
/// <param name="Font">Cislo pisma z koncoveho <c>{n}</c>; <see langword="null"/>, ak nebolo, -1 pri <c>{@}</c> (predvolene pismo).</param>
public readonly record struct TabTabText(string Text, int? Font)
{
    /// <summary>Hodnota <see cref="Font"/> pre <c>{@}</c>.</summary>
    public const int DefaultFont = -1;

    /// <summary>
    /// Dekoduje text ako INISS: preskoci vedúce medzery, <c>\x</c> je doslovne <c>x</c>, <c>"</c> zapina
    /// a vypina uvodzovky (<c>""</c> vnutri je <c>"</c>), mimo uvodzoviek sa koncove medzery zahodia
    /// a <c>{n}</c> alebo <c>{@}</c> uplne na konci je cislo pisma, nie text.
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
    /// Ci od pozicie <paramref name="i"/> (na <c>{</c>) nasleduje <c>{n}</c> alebo <c>{@}</c> a za nim uz len medzery.
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
    /// Druh nedostatku v zapise textu.
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
    /// Najde v surovom zapise textu (lava strana pravidla, polozka) veci, ktore INISS sice precita,
    /// ale skoro urcite inak, nez autor chcel.
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
