using System.Text.RegularExpressions;

namespace ToolsCore.Iniss2.Grafikon.Script;

/// <summary>
/// Skript hlaseni (LogText.vb) rozdeleny na funkcie triedy <c>LogText</c>. Nie je to uplny parser VB.NET: hlada podpisy
/// funkcii, oznacenia nad nimi a ich koniec; neobvykly zapis (napr. podpis bez konca) funkciu vynecha. Uprava vracia novy
/// text, riadky mimo upravy ostanu po znakoch a konce riadkov podla suboru.
/// </summary>
public sealed partial class LogTextScript
{
    private readonly List<string> _lines;

    private LogTextScript(string newLine, List<string> lines, List<LogTextMember> members, int? classStart, int? classEnd)
    {
        NewLine = newLine;
        _lines = lines;
        Members = members;
        ClassStart = classStart;
        ClassEnd = classEnd;
    }

    /// <summary>Koniec riadka podla textu: <c>\r\n</c>, ak ho text obsahuje, inak <c>\n</c>.</summary>
    public string NewLine { get; }

    /// <summary>Riadky textu bez koncov riadkov.</summary>
    public IReadOnlyList<string> Lines => _lines;

    /// <summary>Funkcie triedy <c>LogText</c> v poradi textu (bez funkcii vnorenych typov).</summary>
    public IReadOnlyList<LogTextMember> Members { get; }

    /// <summary>Riadok <c>Class LogText</c>; <see langword="null" />, ak ho text nema (funkcie sa potom hladaju v celom texte).</summary>
    public int? ClassStart { get; }

    /// <summary>Riadok <c>End Class</c> triedy <c>LogText</c>; <see langword="null" />, ak trieda nie je ukoncena.</summary>
    public int? ClassEnd { get; }

    /// <summary>Text skriptu.</summary>
    public string Text => string.Join(NewLine, _lines);

    /// <summary>
    /// Rozdeli text skriptu na funkcie.
    /// </summary>
    /// <param name="text">text LogText.vb</param>
    public static LogTextScript Parse(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var newLine = text.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        var lines = text.Split(newLine).ToList();
        var (classStart, classEnd) = FindClass(lines);
        var from = classStart is { } start ? start + 1 : 0;
        return new LogTextScript(newLine, lines, FindMembers(lines, from, classEnd ?? lines.Count), classStart, classEnd);
    }

    /// <summary>
    /// Funkcia podla nazvu bez ohladu na velkost pismen (ako VB.NET); <see langword="null" />, ak nie je.
    /// </summary>
    public LogTextMember? Find(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        return Members.FirstOrDefault(member => string.Equals(member.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Text, v ktorom su riadky <paramref name="start" /> az <paramref name="end" /> (vratane) nahradene inymi.
    /// </summary>
    public string Replace(int start, int end, IReadOnlyList<string> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);
        CheckRange(start, end);
        var result = new List<string>(_lines);
        result.RemoveRange(start, end - start + 1);
        result.InsertRange(start, lines);
        return string.Join(NewLine, result);
    }

    /// <summary>
    /// Text s riadkami vlozenymi pred riadok <paramref name="index" /> (pocet riadkov = na koniec).
    /// </summary>
    public string Insert(int index, IReadOnlyList<string> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, _lines.Count);
        var result = new List<string>(_lines);
        result.InsertRange(index, lines);
        return string.Join(NewLine, result);
    }

    /// <summary>
    /// Text bez riadkov <paramref name="start" /> az <paramref name="end" /> (vratane).
    /// </summary>
    public string Remove(int start, int end) => Replace(start, end, []);

    private void CheckRange(int start, int end)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(start);
        ArgumentOutOfRangeException.ThrowIfLessThan(end, start);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(end, _lines.Count);
    }

    // trieda LogText a jej koniec (vnorene typy sa preskocia, komentare a retazce sa ignoruju)
    private static (int? Start, int? End) FindClass(List<string> lines)
    {
        var start = lines.FindIndex(line => LogTextClassPattern().IsMatch(VbText.Code(line).Trim()));
        if (start < 0)
            return (null, null);

        var depth = 1;
        for (var i = start + 1; i < lines.Count; i++)
        {
            var code = VbText.Code(lines[i]).Trim();
            if (TypeEndPattern().IsMatch(code) && --depth == 0)
                return (start, i);
            if (TypeStartPattern().IsMatch(code))
                depth++;
        }

        return (start, null);
    }

    private static List<LogTextMember> FindMembers(List<string> lines, int from, int to)
    {
        var members = new List<LogTextMember>();
        var attributes = new List<LogTextAttribute>();
        var attributesParsed = true;
        var headerStart = -1;
        var depth = 0;

        for (var i = from; i < to; i++)
        {
            var first = i;
            var (raw, code) = LogicalLine(lines, ref i, to);
            if (code.Length == 0)
                continue;

            if (depth > 0)
            {
                if (TypeEndPattern().IsMatch(code))
                    depth--;
                else if (TypeStartPattern().IsMatch(code))
                    depth++;
                continue;
            }

            if (TypeStartPattern().IsMatch(code))
            {
                depth++;
                Reset();
                continue;
            }

            var consumed = 0;
            if (code.StartsWith('<'))
            {
                if (ParseAttributes(raw, code, out consumed) is { } parsed)
                    attributes.AddRange(parsed);
                else
                    attributesParsed = false;
                if (headerStart < 0)
                    headerStart = first;
            }

            var signature = SignaturePattern().Match(code, consumed);
            if (!signature.Success)
            {
                // samotne oznacenia patria nasledujucej funkcii, iny kod (pole, #Region) ich zahodi
                if (code[consumed..].Trim().Length > 0)
                    Reset();
                continue;
            }

            var modifiers = signature.Groups["mods"].Value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (modifiers.Any(modifier => modifier.Equals("MustOverride", StringComparison.OrdinalIgnoreCase)
                                          || modifier.Equals("Partial", StringComparison.OrdinalIgnoreCase)))
            {
                Reset();
                continue;
            }

            var end = FindEnd(lines, i + 1, to);
            if (end < 0)
            {
                Reset();
                continue;
            }

            var (parameters, rest) = SplitParameters(raw, code, signature.Groups["after"].Index);
            var start = headerStart >= 0 ? headerStart : first;
            var commentStart = VbText.CommentStart(lines[end]);
            members.Add(new LogTextMember(signature.Groups["name"].Value)
            {
                Keyword = signature.Groups["kw"].Value,
                Modifiers = modifiers,
                Parameters = parameters,
                Rest = rest,
                Attributes = attributes,
                AttributesParsed = attributesParsed,
                Start = start,
                Signature = i,
                End = end,
                EndComment = commentStart < 0 ? null : lines[end][commentStart..].TrimEnd(),
                Indent = lines[start][..(lines[start].Length - lines[start].TrimStart().Length)]
            });
            Reset();
            i = end;
        }

        return members;

        void Reset()
        {
            attributes = [];
            attributesParsed = true;
            headerStart = -1;
        }
    }

    // logicky riadok od indexu i (pokracovanie spoji dalsie riadky, i ukaze na posledny): text bez komentara a kod
    // s prazdnymi retazcami, oba rovnako dlhe
    private static (string Raw, string Code) LogicalLine(List<string> lines, ref int i, int to)
    {
        var raw = VbText.WithoutComment(lines[i]).Trim();
        var code = VbText.Code(lines[i]).Trim();
        while (VbText.Continues(code) && i + 1 < to)
        {
            i++;
            raw = raw[..^1].TrimEnd() + " " + VbText.WithoutComment(lines[i]).Trim();
            code = code[..^1].TrimEnd() + " " + VbText.Code(lines[i]).Trim();
        }

        return raw.Length == code.Length ? (raw, code) : (code, code);
    }

    // oznacenia <...> na zaciatku riadka; null = neobvykly zapis
    private static List<LogTextAttribute>? ParseAttributes(string raw, string code, out int consumed)
    {
        var result = new List<LogTextAttribute>();
        var ok = true;
        var position = 0;
        while (position < code.Length && code[position] == '<')
        {
            var end = AttributeGroupEnd(code, position);
            if (end < 0)
            {
                consumed = code.Length;
                return null;
            }

            foreach (var item in VbText.SplitTopLevel(raw[(position + 1)..end], ','))
            {
                if (ParseAttribute(item) is { } attribute)
                    result.Add(attribute);
                else
                    ok = false;
            }

            position = end + 1;
            while (position < code.Length && char.IsWhiteSpace(code[position]))
                position++;
        }

        consumed = position;
        return ok ? result : null;
    }

    // koniec skupiny oznaceni '>' mimo zatvoriek (retazce su v kode prazdne)
    private static int AttributeGroupEnd(string code, int start)
    {
        var depth = 0;
        for (var i = start + 1; i < code.Length; i++)
        {
            switch (code[i])
            {
                case '(':
                    depth++;
                    break;
                case ')':
                    depth--;
                    break;
                case '>' when depth == 0:
                    return i;
            }
        }

        return -1;
    }

    private static LogTextAttribute? ParseAttribute(string text)
    {
        var match = AttributePattern().Match(text);
        if (!match.Success)
            return null;

        var name = match.Groups["name"].Value;
        var arguments = match.Groups["args"];
        if (!arguments.Success)
            return new LogTextAttribute(name, []);

        var value = arguments.Value;
        if (VbText.Closing(value, 0) != value.Length - 1)
            return null;

        var inner = value[1..^1];
        return new LogTextAttribute(name, inner.Trim().Length == 0 ? [] : VbText.SplitTopLevel(inner, ','));
    }

    // parametre medzi zatvorkami za nazvom a zvysok podpisu
    private static (string? Parameters, string Tail) SplitParameters(string raw, string code, int after)
    {
        var position = after;
        while (position < code.Length && char.IsWhiteSpace(code[position]))
            position++;
        if (position >= code.Length || code[position] != '(')
            return (null, raw[position..].Trim());

        var close = VbText.Closing(code, position);
        return close < 0 ? (raw[(position + 1)..].Trim(), "") : (raw[(position + 1)..close].Trim(), raw[(close + 1)..].Trim());
    }

    // riadok End Sub/End Function funkcie; viacriadkove lambdy (Sub(...) bez tela na riadku) maju vlastny koniec.
    // -1 = funkcia nema koniec pred dalsou funkciou alebo koncom triedy.
    private static int FindEnd(List<string> lines, int from, int to)
    {
        var lambdas = 0;
        for (var i = from; i < to; i++)
        {
            var code = VbText.Code(lines[i]).Trim();
            if (EndPattern().IsMatch(code))
            {
                if (lambdas == 0)
                    return i;
                lambdas--;
                continue;
            }

            if (SignaturePattern().IsMatch(code) || TypeStartPattern().IsMatch(code) || TypeEndPattern().IsMatch(code))
                return -1;
            lambdas += OpenedLambdas(code);
        }

        return -1;
    }

    // pocet viacriadkovych lambd, ktore riadok otvara: Sub(...) alebo Function(...) [As typ] na konci riadka
    private static int OpenedLambdas(string code)
    {
        var count = 0;
        foreach (Match match in LambdaPattern().Matches(code))
        {
            var open = match.Index + match.Length - 1;
            var close = VbText.Closing(code, open);
            if (close < 0)
                continue;
            var rest = code[(close + 1)..].Trim();
            if (rest.Length == 0 || LambdaTypePattern().IsMatch(rest))
                count++;
        }

        return count;
    }

    [GeneratedRegex(@"^(?:(?:Public|Partial|Friend|NotInheritable)\s+)*Class\s+LogText\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex LogTextClassPattern();

    [GeneratedRegex(@"^(?:(?:Public|Private|Friend|Protected|Partial|Shared|Shadows|NotInheritable|MustInherit)\s+)*(?:Class|Structure|Interface|Module)\s+\w", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex TypeStartPattern();

    [GeneratedRegex(@"^End\s+(?:Class|Structure|Interface|Module)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex TypeEndPattern();

    [GeneratedRegex(@"\G(?<mods>(?:(?:Public|Private|Protected|Friend|Shared|Overrides|Overridable|Overloads|NotOverridable|MustOverride|Shadows|Partial|Async|Iterator)\s+)*)(?<kw>Sub|Function)\s+(?<name>[\p{L}\p{N}_]+)(?<after>.*)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex SignaturePattern();

    [GeneratedRegex(@"^End\s+(?:Sub|Function)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex EndPattern();

    [GeneratedRegex(@"^(?:(?:Assembly|Module)\s*:\s*)?(?<name>[\p{L}\p{N}_.]+)\s*(?<args>\(.*\))?$", RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex AttributePattern();

    [GeneratedRegex(@"(?<![\p{L}\p{N}_])(?:Sub|Function)\s*\(", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex LambdaPattern();

    [GeneratedRegex(@"^As\s+[\p{L}\p{N}_.()]+$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex LambdaTypePattern();
}
