using System.Globalization;
using System.Text.RegularExpressions;

namespace ToolsCore.Iniss2.Grafikon.Script;

/// <summary>
/// Cita funkciu skriptu ako hlasenie upravitelne v editore. Funkcia je hlasenie, ak ju model uplne opise a jej
/// opatovny zapis da ten isty kod (porovnanie bez ohladu na medzery a prazdne riadky); inak ostava rucnym kodom a editor
/// ju nemeni.
/// </summary>
public static partial class ScriptReportReader
{
    /// <summary>
    /// Precita funkciu skriptu.
    /// </summary>
    /// <param name="script">skript</param>
    /// <param name="member">funkcia skriptu</param>
    public static ScriptReadResult Read(LogTextScript script, LogTextMember member)
    {
        ArgumentNullException.ThrowIfNull(script);
        ArgumentNullException.ThrowIfNull(member);

        var (report, reason, line) = Parse(script, member);
        if (report is null)
            return new ScriptReadResult(member, null, reason, line);

        var original = script.Lines.Skip(member.Start).Take(member.End - member.Start + 1);
        var written = ScriptReportWriter.Lines(report, member.Indent, member.EndComment);
        return VbText.Normalize(original).SequenceEqual(VbText.Normalize(written), StringComparer.Ordinal)
            ? new ScriptReadResult(member, report, ScriptManualReason.None, member.Start)
            : new ScriptReadResult(member, null, ScriptManualReason.Layout, member.Start);
    }

    private static (ScriptReport? Report, ScriptManualReason Reason, int Line) Parse(LogTextScript script, LogTextMember member)
    {
        if (!member.AttributesParsed || member.Attributes.Count != 1 || !ScriptReportKinds.TryParse(member.Attributes[0].Name, out var kind))
            return (null, ScriptManualReason.NotReport, member.Start);

        if (member.Keyword != "Sub" || member.Modifiers is not ["Public"] || member.Rest.Length > 0)
            return (null, ScriptManualReason.Signature, member.Signature);

        string? parameter = null;
        if (!string.IsNullOrEmpty(member.Parameters))
        {
            var match = ParameterPattern().Match(member.Parameters);
            if (!match.Success)
                return (null, ScriptManualReason.Signature, member.Signature);
            parameter = match.Groups["name"].Value;
        }

        var report = ReadAttribute(member.Attributes[0], member.Name, kind);
        if (report is null)
            return (null, ScriptManualReason.Attribute, member.Start);

        var body = ReadBody(script, member);
        return body.Blocks is null
            ? (null, ScriptManualReason.Statement, body.Line)
            : (report with
            {
                StationsParameter = parameter, OnlyLanguage = body.OnlyLanguage, Guards = body.Guards, Section = body.Section, Blocks = body.Blocks
            }, ScriptManualReason.None, member.Start);
    }

    // popis, poradove argumenty a obmedzenie na stanice; null = neobvykly zapis
    private static ScriptReport? ReadAttribute(LogTextAttribute attribute, string name, ScriptReportKind kind)
    {
        if (attribute.Description is not { } description)
            return null;

        var arguments = new List<string>();
        string? forStations = null;
        string? notStations = null;
        var named = false;
        foreach (var argument in attribute.Arguments.Skip(1))
        {
            var match = NamedArgumentPattern().Match(argument);
            if (!match.Success)
            {
                if (named || argument.Length == 0)
                    return null;
                arguments.Add(argument);
                continue;
            }

            named = true;
            var value = VbText.StringValue(match.Groups["value"].Value);
            switch (match.Groups["name"].Value)
            {
                case "ProStanice" when value is not null && forStations is null:
                    forStations = value;
                    break;
                case "MimoStanice" when value is not null && notStations is null:
                    notStations = value;
                    break;
                default:
                    return null;
            }
        }

        if ((forStations ?? notStations) is not null && !ScriptReportKinds.HasStationLimit(kind))
            return null;

        return new ScriptReport(name, kind)
        {
            Description = description,
            Arguments = arguments,
            ForStations = forStations,
            NotStations = notStations
        };
    }

    // telo funkcie: stráze (If … Then Return na zaciatku, prva jazykova je jazyk hlasenia), sekcia, kroky vo vnorenych
    // podmienkach If/Else; bloky null a riadok = prikaz, ktory model neopise
    private static (string? OnlyLanguage, List<string> Guards, bool Section, List<ScriptBlock>? Blocks, int Line) ReadBody(LogTextScript script,
        LogTextMember member)
    {
        string? onlyLanguage = null;
        var guards = new List<string>();
        var section = false;
        var sectionClosed = false;
        var started = false;
        var blocks = new List<ScriptBlock>();
        // otvorene podmienky a pocet blokov pri ich otvoreni (prazdna vetva sa zachova ako prazdny blok)
        var open = new List<(ScriptCondition Condition, int Blocks)>();
        List<ScriptStep>? current = null;

        for (var i = member.Signature + 1; i < member.End; i++)
        {
            var text = script.Lines[i].Trim();
            if (text.Length == 0)
                continue;

            var commentStart = VbText.CommentStart(text);
            var code = commentStart < 0 ? text : text[..commentStart].TrimEnd();
            var comment = commentStart < 0 ? null : text[(commentStart + 1)..];
            if (VbText.Continues(code) || sectionClosed)
                return Fail(i);

            if (code.Length == 0)
            {
                Add(new CommentStep(comment!));
                continue;
            }

            if (ReturnPattern().Match(code) is { Success: true } guard)
            {
                if (started || comment is not null)
                    return Fail(i);
                if (guards.Count == 0 && onlyLanguage is null && GuardPattern().Match(code) is { Success: true } language)
                    onlyLanguage = language.Groups["lang"].Value;
                else
                    guards.Add(guard.Groups["expr"].Value);
                continue;
            }

            if (SectionStartPattern().IsMatch(code))
            {
                if (started || section || comment is not null)
                    return Fail(i);
                section = true;
                started = true;
                continue;
            }

            if (SectionEndPattern().IsMatch(code))
            {
                if (!section || open.Count > 0 || comment is not null)
                    return Fail(i);
                sectionClosed = true;
                continue;
            }

            started = true;
            if (IfPattern().Match(code) is { Success: true } condition)
            {
                if (comment is not null)
                    return Fail(i);
                open.Add((new ScriptCondition(condition.Groups["expr"].Value), blocks.Count));
                current = null;
                continue;
            }

            if (code == "Else")
            {
                if (open.Count == 0 || open[^1].Condition.Else || comment is not null)
                    return Fail(i);
                KeepEmpty();
                open[^1] = (open[^1].Condition with { Else = true }, blocks.Count);
                current = null;
                continue;
            }

            if (code == "End If")
            {
                if (open.Count == 0 || comment is not null)
                    return Fail(i);
                KeepEmpty();
                open.RemoveAt(open.Count - 1);
                current = null;
                continue;
            }

            // viac krokov na riadku oddelenych dvojbodkou; komentar patri poslednemu
            var statements = VbText.SplitStatements(code);
            for (var s = 0; s < statements.Count; s++)
            {
                if (ReadStep(statements[s], IsFunction) is not { } step)
                    return Fail(i);
                Add(step with { Comment = s == statements.Count - 1 ? comment : null });
            }
        }

        if (open.Count > 0 || section != sectionClosed)
            return Fail(member.End);
        return (onlyLanguage, guards, section, blocks, member.Start);

        bool IsFunction(string name) => script.Find(name) is not null;

        (string?, List<string>, bool, List<ScriptBlock>?, int) Fail(int line) => (null, guards, false, null, line);

        // krok do bloku s aktualnymi podmienkami; novy blok po kazdom If, Else a End If
        void Add(ScriptStep step)
        {
            started = true;
            if (current is null)
            {
                current = [];
                blocks.Add(Block(current));
            }

            current.Add(step);
        }

        // vetva bez krokov (ani vnorenych) ostane ako prazdny blok, aby sa zapisala rovnako
        void KeepEmpty()
        {
            if (blocks.Count == open[^1].Blocks)
                blocks.Add(Block([]));
        }

        // blok s podmienkami otvorenych If: prva jazykova podmienka je jazyk bloku
        ScriptBlock Block(List<ScriptStep> steps)
        {
            var chain = open.Select(item => item.Condition).ToList();
            if (chain.Count > 0 && !chain[0].Else && LanguagePattern().Match("If " + chain[0].Expression + " Then") is { Success: true } language)
                return new ScriptBlock(language.Groups["lang"].Value, steps) { Conditions = chain[1..] };
            return new ScriptBlock(null, steps) { Conditions = chain };
        }
    }

    /// <summary>
    /// Krok z jedneho riadka kodu: zvuk, pauza, vstavana metoda alebo volanie funkcie s pripadnym komentarom na konci,
    /// samotny komentar je vypnuty krok; <see langword="null" />, ak riadok nie je jeden krok (napr. pri zapnuti vypnuteho
    /// riadka).
    /// </summary>
    /// <param name="line">riadok kodu bez odsadenia</param>
    /// <param name="isFunction">meno je funkcia skriptu; <see langword="null" /> = kazde meno mimo vstavanych metod</param>
    public static ScriptStep? ReadLine(string line, Func<string, bool>? isFunction = null)
    {
        ArgumentNullException.ThrowIfNull(line);
        var text = line.Trim();
        var commentStart = VbText.CommentStart(text);
        var code = commentStart < 0 ? text : text[..commentStart].TrimEnd();
        var comment = commentStart < 0 ? null : text[(commentStart + 1)..];
        if (code.Length == 0)
            return comment is null ? null : new CommentStep(comment);
        return ReadStep(code, isFunction ?? (_ => true)) is { } step ? step with { Comment = comment } : null;
    }

    // zvuk, pauza, vstavana metoda alebo volanie funkcie skriptu s jednoduchymi argumentmi; null = iny prikaz
    private static ScriptStep? ReadStep(string code, Func<string, bool> isFunction)
    {
        if (ReadSoundOrPause(code) is { } step)
            return step;

        if (CallPattern().Match(code) is not { Success: true } call)
            return null;
        var name = call.Groups["name"].Value;
        var open = call.Groups["open"].Index;
        if (VbText.Closing(code, open) != code.Length - 1
            || name.Equals("Zvuk", StringComparison.OrdinalIgnoreCase) || name.Equals("Pauza", StringComparison.OrdinalIgnoreCase))
            return null;

        var arguments = ScriptArguments.Split(code[(open + 1)..^1]);
        if (!ScriptArguments.AreSimple(arguments))
            return null;
        if (ScriptBuiltins.Find(name) is { } builtin)
            return builtin.Accepts(arguments) ? new BuiltinStep(builtin.Method, arguments) : null;
        return isFunction(name) ? new CallStep(name, arguments) : null;
    }

    // zvuk alebo pauza; null = iny prikaz
    private static ScriptStep? ReadSoundOrPause(string code)
    {
        if (SoundPattern().Match(code) is { Success: true } sound)
        {
            if (VbText.StringValue(sound.Groups["path"].Value) is not { } path)
                return null;
            if (!sound.Groups["before"].Success)
                return new SoundStep(path);
            return int.TryParse(sound.Groups["before"].Value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var before)
                   && int.TryParse(sound.Groups["after"].Value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var after)
                ? new SoundStep(path, before, after)
                : null;
        }

        if (PausePattern().Match(code) is { Success: true } pause)
            return int.TryParse(pause.Groups["ms"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var milliseconds)
                ? new PauseStep(milliseconds)
                : null;

        return null;
    }

    [GeneratedRegex(@"^ByVal\s+(?<name>[\p{L}\p{N}_]+)\s+As\s+Integer\(\)$", RegexOptions.CultureInvariant)]
    private static partial Regex ParameterPattern();

    [GeneratedRegex(@"^(?<name>[\p{L}\p{N}_]+)\s*:=\s*(?<value>.+)$", RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex NamedArgumentPattern();

    [GeneratedRegex(@"^If\s*(?:\(\s*Jazyk\s*<>\s*""(?<lang>[^""]+)""\s*\)|Jazyk\s*<>\s*""(?<lang>[^""]+)"")\s*Then\s+Return$", RegexOptions.CultureInvariant)]
    private static partial Regex GuardPattern();

    [GeneratedRegex(@"^If\s*(?:\(\s*Jazyk\s*=\s*""(?<lang>[^""]+)""\s*\)|Jazyk\s*=\s*""(?<lang>[^""]+)"")\s*Then$", RegexOptions.CultureInvariant)]
    private static partial Regex LanguagePattern();

    [GeneratedRegex(@"^Zvuk\s*\(\s*(?<path>""(?:[^""]|"""")*"")\s*(?:,\s*(?<before>-?\d+)\s*,\s*(?<after>-?\d+)\s*)?\)$", RegexOptions.CultureInvariant)]
    private static partial Regex SoundPattern();

    [GeneratedRegex(@"^Pauza\s*\(\s*(?<ms>\d+)\s*\)$", RegexOptions.CultureInvariant)]
    private static partial Regex PausePattern();

    [GeneratedRegex(@"^(?<name>[\p{L}_][\p{L}\p{N}_]*)\s*(?<open>\()", RegexOptions.CultureInvariant)]
    private static partial Regex CallPattern();

    [GeneratedRegex(@"^If(?=[\s(])\s*(?<expr>.+?)\s*Then\s+Return$", RegexOptions.CultureInvariant)]
    private static partial Regex ReturnPattern();

    [GeneratedRegex(@"^If(?=[\s(])\s*(?<expr>.+?)\s*Then$", RegexOptions.CultureInvariant)]
    private static partial Regex IfPattern();

    [GeneratedRegex(@"^ZacatekSekce\s*\(\s*\)$", RegexOptions.CultureInvariant)]
    private static partial Regex SectionStartPattern();

    [GeneratedRegex(@"^KonecSekce\s*\(\s*bRollBack\s*:=\s*True\s*\)$", RegexOptions.CultureInvariant)]
    private static partial Regex SectionEndPattern();
}
