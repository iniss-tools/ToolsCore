using System.Text;

namespace ToolsCore.Iniss2.Grafikon.Script;

/// <summary>
/// Pomocne operacie nad riadkami kodu VB.NET: retazce, komentare, pokracovanie riadka a porovnanie zapisu bez ohladu
/// na medzery.
/// </summary>
internal static class VbText
{
    /// <summary>
    /// Index apostrofu, ktorym zacina komentar mimo retazca; -1 = riadok nema komentar. <c>REM</c> sa neuvazuje.
    /// </summary>
    public static int CommentStart(string line)
    {
        var inString = false;
        for (var i = 0; i < line.Length; i++)
        {
            if (line[i] == '"')
                inString = !inString;
            else if (!inString && line[i] == '\'')
                return i;
        }

        return -1;
    }

    /// <summary>
    /// Kod riadka bez komentara, obsah retazcov nahradeny znakom <c>x</c> (uvodzovky ostanu) - na hladanie klucovych
    /// slov, ktore v retazci nie su kodom.
    /// </summary>
    public static string Code(string line)
    {
        var code = new StringBuilder(line.Length);
        var inString = false;
        foreach (var c in line)
        {
            if (c == '"')
            {
                inString = !inString;
                code.Append(c);
                continue;
            }

            if (!inString && c == '\'')
                break;
            code.Append(inString ? 'x' : c);
        }

        return code.ToString().TrimEnd();
    }

    /// <summary>
    /// Riadok bez komentara (retazce ostanu), bez koncovych medzier.
    /// </summary>
    public static string WithoutComment(string line)
    {
        var start = CommentStart(line);
        return (start < 0 ? line : line[..start]).TrimEnd();
    }

    /// <summary>
    /// Kod konci pokracovanim riadka (medzera a podciarkovnik).
    /// </summary>
    public static bool Continues(string code) => code == "_" || code.EndsWith(" _", StringComparison.Ordinal) || code.EndsWith("\t_", StringComparison.Ordinal);

    /// <summary>
    /// Index zatvorky, ktora uzatvara zatvorku na <paramref name="open" /> (retazce sa preskocia); -1 = chyba.
    /// </summary>
    public static int Closing(string code, int open)
    {
        var depth = 0;
        var inString = false;
        for (var i = open; i < code.Length; i++)
        {
            var c = code[i];
            if (c == '"')
                inString = !inString;
            else if (inString)
                continue;
            else if (c == '(')
                depth++;
            else if (c == ')' && --depth == 0)
                return i;
        }

        return -1;
    }

    /// <summary>
    /// Rozdeli text podla oddelovaca mimo retazcov a zatvoriek; casti su orezane.
    /// </summary>
    public static List<string> SplitTopLevel(string text, char separator)
    {
        var parts = new List<string>();
        var depth = 0;
        var inString = false;
        var start = 0;
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (c == '"')
                inString = !inString;
            else if (inString)
                continue;
            else if (c is '(' or '{')
                depth++;
            else if (c is ')' or '}')
                depth--;
            else if (c == separator && depth == 0)
            {
                parts.Add(text[start..i].Trim());
                start = i + 1;
            }
        }

        parts.Add(text[start..].Trim());
        return parts;
    }

    /// <summary>
    /// Prikazy riadka kodu oddelene dvojbodkou mimo retazcov (pomenovany argument <c>:=</c> nie je oddelovac); casti
    /// su orezane, prazdne sa vynechaju.
    /// </summary>
    public static List<string> SplitStatements(string code)
    {
        var parts = new List<string>();
        var inString = false;
        var start = 0;
        for (var i = 0; i < code.Length; i++)
        {
            var c = code[i];
            if (c == '"')
                inString = !inString;
            else if (!inString && c == ':' && (i + 1 >= code.Length || code[i + 1] != '='))
            {
                parts.Add(code[start..i].Trim());
                start = i + 1;
            }
        }

        parts.Add(code[start..].Trim());
        return [.. parts.Where(part => part.Length > 0)];
    }

    /// <summary>
    /// Hodnota retazcoveho literalu VB (<c>"..."</c>, zdvojene uvodzovky); <see langword="null" />, ak text nie je jeden
    /// literal.
    /// </summary>
    public static string? StringValue(string text)
    {
        var literal = text.Trim();
        if (literal.Length < 2 || literal[0] != '"' || literal[^1] != '"')
            return null;

        var value = new StringBuilder();
        for (var i = 1; i < literal.Length - 1; i++)
        {
            if (literal[i] != '"')
            {
                value.Append(literal[i]);
                continue;
            }

            if (i + 1 >= literal.Length - 1 || literal[i + 1] != '"')
                return null;
            value.Append('"');
            i++;
        }

        return value.ToString();
    }

    /// <summary>
    /// Retazcovy literal VB pre text.
    /// </summary>
    public static string Literal(string value) => "\"" + value.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";

    /// <summary>
    /// Zapis riadkov na porovnanie: logicke riadky (pokracovanie spojene, oznacenia spojene s podpisom), bez prazdnych
    /// riadkov, medzier navyse, medzier pri zatvorkach a ciarkach a bez vonkajsich zatvoriek podmienky <c>If</c>.
    /// Retazce ostanu presne.
    /// </summary>
    public static List<string> Normalize(IEnumerable<string> lines)
    {
        var logical = new List<string>();
        var pending = "";
        foreach (var line in lines)
        {
            var code = WithoutComment(line);
            if (Continues(code.TrimStart()))
            {
                pending += code.TrimEnd()[..^1] + " ";
                continue;
            }

            // viac prikazov na riadku = viac riadkov; komentar ostane pri poslednom
            var full = pending + line;
            pending = "";
            var commentAt = CommentStart(full);
            var statements = SplitStatements(commentAt < 0 ? full : full[..commentAt]);
            if (statements.Count > 1)
            {
                statements[^1] += commentAt < 0 ? "" : " " + full[commentAt..];
                logical.AddRange(statements.Select(Canonical));
                continue;
            }

            var canonical = Canonical(full);
            if (canonical.Length > 0)
                logical.Add(canonical);
        }

        if (pending.Length > 0)
            logical.Add(Canonical(pending));

        // samostatne oznacenia (bez pokracovania riadka) patria k nasledujucemu riadku
        var result = new List<string>();
        var attributes = "";
        foreach (var line in logical)
        {
            if (line.StartsWith('<') && line.EndsWith('>'))
            {
                attributes += line + " ";
                continue;
            }

            result.Add(attributes + line);
            attributes = "";
        }

        if (attributes.Length > 0)
            result.Add(attributes.TrimEnd());
        return result;
    }

    // jeden logicky riadok v porovnavanom tvare
    private static string Canonical(string line)
    {
        var text = line.Trim();
        var code = new StringBuilder(text.Length);
        var inString = false;
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (inString)
            {
                code.Append(c);
                if (c == '"')
                    inString = false;
                continue;
            }

            if (c == '"')
            {
                inString = true;
                code.Append(c);
                continue;
            }

            if (c == '\'')
            {
                var comment = string.Join(' ', text[(i + 1)..].Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
                var before = WithoutIfParentheses(code.ToString().TrimEnd());
                return before.Length == 0 ? "'" + comment : before + " '" + comment;
            }

            if (char.IsWhiteSpace(c))
            {
                if (code.Length > 0 && code[^1] != ' ')
                    code.Append(' ');
                continue;
            }

            if (c is '(' or ')' or ',')
            {
                if (code.Length > 0 && code[^1] == ' ')
                    code.Length--;
                code.Append(c);
                while (i + 1 < text.Length && char.IsWhiteSpace(text[i + 1]))
                    i++;
                continue;
            }

            code.Append(c);
        }

        return WithoutIfParentheses(code.ToString().TrimEnd());
    }

    // "If(podmienka)Then zvysok" -> "If podmienka Then zvysok", ak zatvorky obaluju celu podmienku
    private static string WithoutIfParentheses(string code)
    {
        if (!code.StartsWith("If(", StringComparison.Ordinal))
            return code;

        var close = Closing(code, 2);
        if (close < 0 || !code.AsSpan(close + 1).StartsWith("Then", StringComparison.Ordinal))
            return code;

        var rest = code[(close + 1 + "Then".Length)..];
        if (rest.Length > 0 && rest[0] != ' ')
            return code;
        return "If " + code[3..close] + " Then" + rest;
    }
}
