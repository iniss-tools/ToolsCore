namespace ToolsCore.Iniss2.Grafikon.Script;

/// <summary>
/// Hlasenia skriptu LogText.vb ako udaje: citanie funkcii s oznacenim hlasenia, zapis hlasenia na miesto funkcie, nova
/// funkcia a zmazanie. Meni sa vzdy len rozsah jednej funkcie, ostatny text ostane po znakoch.
/// </summary>
public static class ScriptReports
{
    /// <summary>
    /// Funkcie skriptu s oznacenim hlasenia v poradi textu: hlasenie upravitelne v editore alebo dovod rucneho kodu.
    /// </summary>
    public static List<ScriptReadResult> Read(LogTextScript script)
    {
        ArgumentNullException.ThrowIfNull(script);
        return [.. script.Members
            .Where(member => member.Attributes.Any(attribute => ScriptReportKinds.TryParse(attribute.Name, out _)))
            .Select(member => ScriptReportReader.Read(script, member))];
    }

    /// <summary>
    /// Zapise hlasenie do textu skriptu. Funkcia <paramref name="replacing" /> sa nahradi na mieste - komentare nad nou
    /// ostanu, komentar za <c>End Sub</c> tiez (ak bol nazvom funkcie, dostane novy nazov). Bez nej je hlasenie nova
    /// funkcia za poslednou funkciou rovnakeho druhu, inak pred koncom triedy <c>LogText</c>.
    /// </summary>
    /// <param name="text">text skriptu</param>
    /// <param name="report">hlasenie</param>
    /// <param name="replacing">nazov funkcie, ktoru hlasenie nahradza (pri premenovani povodny nazov); <see langword="null" /> = nova funkcia</param>
    /// <returns>novy text skriptu</returns>
    /// <exception cref="ArgumentException">nazov hlasenia uz ma ina funkcia alebo nahradzana funkcia v skripte nie je</exception>
    /// <exception cref="FormatException">nova funkcia, skript vsak nema triedu <c>LogText</c> s koncom</exception>
    public static string Write(string text, ScriptReport report, string? replacing = null)
    {
        ArgumentNullException.ThrowIfNull(report);
        var script = LogTextScript.Parse(text);
        LogTextMember? member = null;
        if (replacing is not null)
            member = script.Find(replacing) ?? throw new ArgumentException($"Skript nema funkciu {replacing}.", nameof(replacing));
        if (script.Find(report.Name) is { } other && other != member)
            throw new ArgumentException($"Skript uz ma funkciu {other.Name}.", nameof(report));

        if (member is not null)
            return script.Replace(member.Start, member.End, ScriptReportWriter.Lines(report, member.Indent, EndComment(member, report.Name)));

        var anchor = script.Members.LastOrDefault(item => item.Attributes.Any(attribute =>
            ScriptReportKinds.TryParse(attribute.Name, out var kind) && kind == report.Kind));
        if (anchor is not null)
            return script.Insert(anchor.End + 1, ["", .. ScriptReportWriter.Lines(report, anchor.Indent, "' " + report.Name)]);

        if (script.ClassEnd is not { } classEnd)
            throw new FormatException("LogText.vb nema triedu LogText.");

        var indent = script.Members.Count > 0 ? script.Members[^1].Indent : ClassIndent(script) + ScriptReportWriter.IndentStep;
        var lines = ScriptReportWriter.Lines(report, indent, "' " + report.Name);
        return script.Insert(classEnd, script.Lines[classEnd - 1].Trim().Length > 0 ? ["", .. lines] : lines);
    }

    /// <summary>
    /// Text skriptu bez funkcie (od oznacenia po <c>End Sub</c>); prazdny riadok, ktory by ostal navyse, sa odstrani.
    /// Komentare nad funkciou ostanu. Funkcia, ktora v skripte nie je, nic nezmeni.
    /// </summary>
    /// <param name="text">text skriptu</param>
    /// <param name="name">nazov funkcie</param>
    public static string Remove(string text, string name)
    {
        var script = LogTextScript.Parse(text);
        if (script.Find(name) is not { } member)
            return text;

        var start = member.Start;
        var end = member.End;
        var before = start > 0 && script.Lines[start - 1].Trim().Length == 0;
        var after = end + 1 < script.Lines.Count && script.Lines[end + 1].Trim().Length == 0;
        if (before && after)
            end++;
        else if (before && end + 1 == script.ClassEnd)
            start--;
        return script.Remove(start, end);
    }

    // komentar za End Sub: nazov funkcie sa premenuje, iny komentar ostane
    private static string? EndComment(LogTextMember member, string name)
    {
        if (member.EndComment is not { } comment)
            return null;
        return string.Equals(comment.TrimStart('\'').Trim(), member.Name, StringComparison.Ordinal) ? "' " + name : comment;
    }

    private static string ClassIndent(LogTextScript script)
    {
        var line = script.ClassStart is { } start ? script.Lines[start] : "";
        return line[..(line.Length - line.TrimStart().Length)];
    }
}
