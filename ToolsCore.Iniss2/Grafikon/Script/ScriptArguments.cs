using System.Text.RegularExpressions;

namespace ToolsCore.Iniss2.Grafikon.Script;

/// <summary>
/// Argumenty volania v kroku hlasenia. Model pozna len jednoduche argumenty: text, cele cislo, <c>True</c>, <c>False</c>,
/// <c>Nothing</c>, <c>AddressOf funkcia</c> a cestu k hodnote (<c>VlakS.PozicePrijSkut</c>, <c>VlakS.Druh(True)</c>,
/// <c>aiStanice</c>); vyraz s operatorom alebo <c>If(…)</c> zostava rucnym kodom.
/// </summary>
public static partial class ScriptArguments
{
    // slova, ktore nie su menom hodnoty (vyrazy a prevody VB.NET)
    private static readonly HashSet<string> Keywords = new(
    [
        "If", "Not", "And", "Or", "AndAlso", "OrElse", "Xor", "Mod", "New", "Is", "IsNot", "Like", "TypeOf", "GetType", "CType", "DirectCast",
        "TryCast", "CInt", "CStr", "CBool", "CDbl", "CLng", "CDate", "CSng", "CDec", "CByte", "CShort", "CObj", "Function", "Sub", "AddressOf"
    ], StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Argumenty volania z textu medzi zatvorkami (oddelene ciarkou mimo retazcov a zatvoriek); prazdny text = ziadne.
    /// </summary>
    public static List<string> Split(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        return text.Trim().Length == 0 ? [] : VbText.SplitTopLevel(text, ',');
    }

    /// <summary>
    /// Pomenovany argument <c>nazov:=hodnota</c>; <see langword="null" />, ak je argument poradovy.
    /// </summary>
    public static (string Name, string Value)? Named(string argument)
    {
        ArgumentNullException.ThrowIfNull(argument);
        var match = NamedPattern().Match(argument.Trim());
        return match.Success ? (match.Groups["name"].Value, match.Groups["value"].Value.Trim()) : null;
    }

    /// <summary>
    /// Vsetky argumenty su jednoduche (pomenovany argument ma jednoduchu hodnotu).
    /// </summary>
    public static bool AreSimple(IEnumerable<string> arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        return arguments.All(argument => IsSimple(Named(argument)?.Value ?? argument));
    }

    /// <summary>
    /// Vyraz je jednoduchy argument (pozri popis triedy).
    /// </summary>
    public static bool IsSimple(string expression)
    {
        ArgumentNullException.ThrowIfNull(expression);
        var text = expression.Trim();
        var position = 0;
        return Value(text, ref position) && Skip(text, ref position) == text.Length;
    }

    private static int Skip(string text, ref int position)
    {
        while (position < text.Length && char.IsWhiteSpace(text[position]))
            position++;
        return position;
    }

    private static bool Value(string text, ref int position)
    {
        Skip(text, ref position);
        if (position >= text.Length)
            return false;

        if (text[position] == '"')
        {
            var close = position + 1;
            while (close < text.Length)
            {
                if (text[close] == '"' && (close + 1 >= text.Length || text[close + 1] != '"'))
                    break;
                close += text[close] == '"' ? 2 : 1;
            }

            if (close >= text.Length)
                return false;
            position = close + 1;
            return true;
        }

        if (NumberPattern().Match(text, position) is { Success: true } number)
        {
            position += number.Length;
            return true;
        }

        if (Identifier(text, ref position) is not { } first)
            return false;
        if (string.Equals(first, "AddressOf", StringComparison.OrdinalIgnoreCase))
            return Identifier(text, ref position) is { } target && !Keywords.Contains(target);
        if (Keywords.Contains(first))
            return false;
        if (first is "True" or "False" or "Nothing")
            return true;

        // cesta: meno, clenovia cez bodku, za kazdym nepovinne argumenty v zatvorkach
        while (true)
        {
            var save = position;
            Skip(text, ref position);
            if (position < text.Length && text[position] == '(')
            {
                var close = VbText.Closing(text, position);
                if (close < 0)
                    return false;
                var inner = text[(position + 1)..close];
                if (inner.Trim().Length > 0 && !VbText.SplitTopLevel(inner, ',').All(IsSimple))
                    return false;
                position = close + 1;
                continue;
            }

            if (position < text.Length && text[position] == '.')
            {
                position++;
                if (Identifier(text, ref position) is not { } member || Keywords.Contains(member))
                    return false;
                continue;
            }

            position = save;
            return true;
        }
    }

    private static string? Identifier(string text, ref int position)
    {
        Skip(text, ref position);
        var match = IdentifierPattern().Match(text, position);
        if (!match.Success)
            return null;
        position += match.Length;
        return match.Value;
    }

    [GeneratedRegex(@"^(?<name>[\p{L}_][\p{L}\p{N}_]*)\s*:=\s*(?<value>.+)$", RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex NamedPattern();

    [GeneratedRegex(@"\G-?\d+(?![\p{L}\p{N}_.])", RegexOptions.CultureInvariant)]
    private static partial Regex NumberPattern();

    [GeneratedRegex(@"\G[\p{L}_][\p{L}\p{N}_]*", RegexOptions.CultureInvariant)]
    private static partial Regex IdentifierPattern();
}
