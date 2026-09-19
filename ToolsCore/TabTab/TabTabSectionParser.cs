namespace ToolsCore.TabTab;

/// <summary>
///     Rozklad textu sekcie TabTab na logicke riadky, pravidla a polozky - so zachovanim fyzickych pozicii
///     pre editor.
/// </summary>
public static class TabTabSectionParser
{
    /// <summary>Volba sekcie: porovnavanie pravej strany bez ohladu na velkost pismen.</summary>
    public const string OptionIgnoreCase = "IgnoreCase";

    /// <summary>Volba sekcie: ViewValues (vyznam v INISSe nezisteny).</summary>
    public const string OptionViewValues = "ViewValues";

    private const string EvVyluka = "#VYLUKA";
    private const string EvOdklon = "#ODKLON";
    private const string EvPozOdj = "#POZODJ_";
    private const string EvSwitch = "#SWITCH";
    private const string EvMerge = "#MERGE";
    private const string EvMerge2 = "#MERGE2";

    /// <summary>
    ///     Rozoberie text sekcie.
    /// </summary>
    public static TabTabSection Parse(string text)
    {
        var lines = new List<TabTabLine>();
        var ignoreCase = false;
        var viewValues = false;

        foreach (var logical in LogicalLines(text))
        {
            var line = Classify(logical);
            lines.Add(line);
            if (line.Kind == TabTabLineKind.Options)
            {
                foreach (var o in line.Options)
                {
                    if (o.Equals(OptionIgnoreCase, StringComparison.OrdinalIgnoreCase)) ignoreCase = true;
                    else if (o.Equals(OptionViewValues, StringComparison.OrdinalIgnoreCase)) viewValues = true;
                }
            }
        }

        return new TabTabSection { Text = text, Lines = lines, IgnoreCase = ignoreCase, ViewValues = viewValues };
    }

    /// <summary>
    ///     Logicky riadok: spojeny text a mapa logicky index → fyzicky index.
    /// </summary>
    private sealed class Logical
    {
        public int LineIndex;
        public int Start;
        public int End;
        public readonly StringBuilder Text = new();
        public readonly List<int> Map = [];

        public int Phys(int logicalIndex) => logicalIndex < Map.Count ? Map[logicalIndex] : End;

        public TabTabSpan Span(int logStart, int logLength) =>
            logLength <= 0 ? TabTabSpan.At(Phys(logStart)) : new TabTabSpan(Phys(logStart), Phys(logStart + logLength - 1) + 1 - Phys(logStart));
    }

    /// <summary>
    ///     Rozdeli text na logicke riadky ako INISS - riadok konciaci <c>\</c> pokracuje
    ///     dalsim riadkom (bez oddelovaca); komentar sa nikdy nespaja.
    /// </summary>
    private static IEnumerable<Logical> LogicalLines(string text)
    {
        // fyzicke riadky (bez CR/LF); text konciaci koncom riadka nema za nim prazdny riadok navyse
        var physical = new List<(int Start, int End)>();
        var pos = 0;
        while (pos < text.Length)
        {
            var nl = text.IndexOf('\n', pos);
            var end = nl < 0 ? text.Length : nl;
            var contentEnd = end;
            if (contentEnd > pos && text[contentEnd - 1] == '\r') contentEnd--;
            physical.Add((pos, contentEnd));
            if (nl < 0) break;
            pos = nl + 1;
        }

        Logical? cur = null;
        for (var li = 0; li < physical.Count; li++)
        {
            var (segStart, segEnd) = physical[li];
            var isNew = cur is null;
            cur ??= new Logical { LineIndex = li, Start = segStart };

            var continues = false;
            if (!(isNew && IsCommentLine(text, segStart, segEnd)) && segEnd > segStart && text[segEnd - 1] == '\\')
            {
                segEnd--;
                continues = true;
            }

            for (var i = segStart; i < segEnd; i++)
            {
                cur.Text.Append(text[i]);
                cur.Map.Add(i);
            }
            cur.End = segEnd;

            if (!continues || li == physical.Count - 1)
            {
                yield return cur;
                cur = null;
            }
        }
    }

    private static bool IsCommentLine(string text, int start, int end)
    {
        var i = start;
        while (i < end && text[i] is ' ' or '\t') i++;
        return i < end && text[i] == ';';
    }

    /// <summary>
    ///     Klasifikacia logickeho riadka ako INISS.
    /// </summary>
    private static TabTabLine Classify(Logical lg)
    {
        var s = lg.Text.ToString();
        var span = new TabTabSpan(lg.Start, lg.End - lg.Start);

        var i = 0;
        while (i < s.Length && s[i] is ' ' or '\t') i++;

        if (i >= s.Length)
            return new TabTabLine { Kind = TabTabLineKind.Empty, LineIndex = lg.LineIndex, Span = span };

        if (s[i] == ';')
            return new TabTabLine { Kind = TabTabLineKind.Comment, LineIndex = lg.LineIndex, Span = span, Text = s[(i + 1)..] };

        if (s[i] == '[')
        {
            var close = s.IndexOf(']', i + 1);
            if (close < 0)
                return new TabTabLine { Kind = TabTabLineKind.BadSectionHeader, LineIndex = lg.LineIndex, Span = span };
            return new TabTabLine
            {
                Kind = TabTabLineKind.SectionHeader, LineIndex = lg.LineIndex, Span = span,
                Text = s[(i + 1)..close], LeftSpan = lg.Span(i + 1, close - i - 1)
            };
        }

        // hladanie posledneho neescapovaneho '='; ';' konci riadok
        var contentStart = i;
        var eq = -1;
        var endPos = s.Length;
        var escape = false;
        for (var j = contentStart; j < s.Length; j++)
        {
            var c = s[j];
            if (escape)
            {
                escape = false;
                continue;
            }
            if (c == '\\')
            {
                escape = true;
                continue;
            }
            if (c == '=')
            {
                eq = j;
                continue;
            }
            if (c == ';')
            {
                endPos = j;
                break;
            }
        }

        if (eq < 0)
        {
            var optText = s[contentStart..endPos].TrimEnd();
            var options = optText.Split(',').Select(o => o.Trim()).Where(o => o.Length > 0).ToList();
            return new TabTabLine
            {
                Kind = TabTabLineKind.Options, LineIndex = lg.LineIndex, Span = span,
                Text = optText, LeftSpan = lg.Span(contentStart, optText.Length), Options = options
            };
        }

        var left = s[contentStart..eq];
        var rightRaw = s[(eq + 1)..endPos];
        var rightTrimmed = rightRaw.TrimEnd();
        var rightLead = rightTrimmed.Length - rightTrimmed.TrimStart().Length;
        var right = rightTrimmed.TrimStart();
        var rightStart = eq + 1 + rightLead;

        var (ev, track) = ClassifyEvent(right);
        var items = ev is TabTabEventKind.Switch or TabTabEventKind.Merge or TabTabEventKind.Merge2
            ? SplitItems(left, contentStart, lg, ev)
            : [];

        return new TabTabLine
        {
            Kind = TabTabLineKind.Rule, LineIndex = lg.LineIndex, Span = span,
            Left = left, LeftSpan = lg.Span(contentStart, left.Length),
            Right = right, RightSpan = lg.Span(rightStart, right.Length),
            Event = ev, PozOdjTrack = track, Items = items
        };
    }

    /// <summary>
    ///     Rozpozna udalost na pravej strane - INISS porovnava presne (s ohladom na velkost pismen).
    /// </summary>
    private static (TabTabEventKind, string?) ClassifyEvent(string right)
    {
        if (right.Length == 0 || right[0] != '#') return (TabTabEventKind.None, null);
        if (right == EvVyluka) return (TabTabEventKind.Vyluka, null);
        if (right == EvOdklon) return (TabTabEventKind.Odklon, null);
        if (right.StartsWith(EvPozOdj, StringComparison.Ordinal)) return (TabTabEventKind.PozOdj, right[EvPozOdj.Length..]);
        if (right == EvSwitch) return (TabTabEventKind.Switch, null);
        if (right == EvMerge) return (TabTabEventKind.Merge, null);
        if (right == EvMerge2) return (TabTabEventKind.Merge2, null);
        return (TabTabEventKind.Unknown, null);
    }

    /// <summary>
    ///     Rozdeli lavu stranu na polozky: ciarka mimo uvodzoviek oddeluje, <c>\</c> chrani nasledujuci znak.
    ///     Polozky sa orezu o okolite medzery.
    /// </summary>
    private static List<TabTabItem> SplitItems(string left, int leftLogStart, Logical lg, TabTabEventKind ev)
    {
        var items = new List<TabTabItem>();
        var step = ev == TabTabEventKind.Switch ? 2 : 3;
        var quoted = false;
        var escape = false;
        var itemStart = 0;

        for (var j = 0; j <= left.Length; j++)
        {
            var atEnd = j == left.Length;
            var c = atEnd ? ',' : left[j];
            if (!atEnd)
            {
                if (escape)
                {
                    escape = false;
                    continue;
                }
                if (c == '\\')
                {
                    escape = true;
                    continue;
                }
                if (c == '"')
                {
                    quoted = !quoted;
                    continue;
                }
                if (c != ',' || quoted) continue;
            }

            // polozka left[itemStart..j]
            var raw = left[itemStart..j];
            var lead = raw.Length - raw.TrimStart().Length;
            var trimmed = raw.Trim();
            var index = items.Count;
            var pos = index % step;
            var isCond = pos == 0;
            var isSep = step == 3 && pos == 1;
            items.Add(new TabTabItem
            {
                Index = index,
                Text = trimmed,
                Span = lg.Span(leftLogStart + itemStart + lead, trimmed.Length),
                IsCondition = isCond,
                IsSeparator = isSep,
                Decoded = isCond ? null : TabTabText.Decode(trimmed)
            });
            itemStart = j + 1;
        }

        return items;
    }
}
