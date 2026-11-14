using System.Globalization;

namespace ToolsCore.Iniss2.Grafikon.Script;

/// <summary>
/// Kod funkcie hlasenia v zapise ako v korpuse a v exporte GVDEditora: oznacenie s pokracovanim riadka <c>_</c>,
/// <c>Public Sub</c>, odsadenie styrmi medzerami, jazykovy blok <c>If (Jazyk = "CZ") Then</c>. Len syntax Visual Basic 10
/// (server preklada kompilatorom v4.0).
/// </summary>
public static class ScriptReportWriter
{
    /// <summary>Odsadenie jednej urovne.</summary>
    public const string IndentStep = "    ";

    /// <summary>
    /// Riadky funkcie bez koncov riadkov.
    /// </summary>
    /// <param name="report">hlasenie</param>
    /// <param name="indent">odsadenie oznacenia a podpisu</param>
    /// <param name="endComment">komentar za <c>End Sub</c> vratane apostrofu; <see langword="null" /> = bez komentara</param>
    public static List<string> Lines(ScriptReport report, string indent = IndentStep, string? endComment = null)
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentNullException.ThrowIfNull(indent);
        var body = indent + IndentStep;
        var parameter = report.StationsParameter is { } name ? "ByVal " + name + " As Integer()" : "";
        var lines = new List<string>
        {
            indent + "<" + ScriptReportKinds.AttributeName(report.Kind) + "(" + string.Join(", ", AttributeArguments(report)) + ")> _",
            indent + "Public Sub " + report.Name + "(" + parameter + ")"
        };

        if (report.OnlyLanguage is { } language)
            lines.Add(body + "If (Jazyk <> " + VbText.Literal(language) + ") Then Return");
        lines.AddRange(report.Guards.Select(guard => body + "If " + guard + " Then Return"));
        if (report.Section)
            lines.Add(body + "ZacatekSekce()");

        Blocks(report.Blocks, body, lines);

        if (report.Section)
            lines.Add(body + "KonecSekce(bRollBack:=True)");
        lines.Add(indent + "End Sub" + (endComment is null ? "" : " " + endComment));
        return lines;
    }

    /// <summary>Podmienka jazykoveho bloku, napr. <c>(Jazyk = "CZ")</c>.</summary>
    public static string LanguageCondition(string language) => "(Jazyk = " + VbText.Literal(language) + ")";

    // bloky s vnorenymi podmienkami: spolocny zaciatok podmienok susednych blokov je jedno If, ta ista podmienka
    // s Else hned za nou je jej vetva Else, dva bloky s rovnakymi podmienkami su dve vnutorne If za sebou
    private static void Blocks(IReadOnlyList<ScriptBlock> blocks, string body, List<string> lines)
    {
        var open = new List<ScriptCondition>();
        foreach (var block in blocks)
        {
            var target = block.Chain.ToList();
            var common = 0;
            while (common < open.Count && common < target.Count && open[common] == target[common])
                common++;
            if (common == open.Count && common == target.Count && common > 0)
                common--;

            var switchToElse = common < open.Count && common < target.Count && !open[common].Else && target[common].Else
                               && open[common].Expression == target[common].Expression;
            Close(open, switchToElse ? common + 1 : common, body, lines);
            var start = common;
            if (switchToElse)
            {
                lines.Add(body + Indent(common) + "Else");
                open[common] = target[common];
                start = common + 1;
            }

            for (var depth = start; depth < target.Count; depth++)
            {
                lines.Add(body + Indent(depth) + "If " + target[depth].Expression + " Then");
                if (target[depth].Else)
                    lines.Add(body + Indent(depth) + "Else");
                open.Add(target[depth]);
            }

            lines.AddRange(block.Steps.Select(step => body + Indent(target.Count) + StepText(step)));
        }

        Close(open, 0, body, lines);
    }

    private static void Close(List<ScriptCondition> open, int depth, string body, List<string> lines)
    {
        for (var level = open.Count - 1; level >= depth; level--)
        {
            lines.Add(body + Indent(level) + "End If");
            open.RemoveAt(level);
        }
    }

    private static string Indent(int depth) => string.Concat(Enumerable.Repeat(IndentStep, depth));

    /// <summary>
    /// Riadok kroku bez odsadenia.
    /// </summary>
    public static string StepText(ScriptStep step)
    {
        ArgumentNullException.ThrowIfNull(step);
        var text = step switch
        {
            SoundStep { PauseBefore: 0, PauseAfter: 0 } sound => "Zvuk(" + VbText.Literal(sound.Path) + ")",
            SoundStep sound => "Zvuk(" + VbText.Literal(sound.Path) + ", " + Number(sound.PauseBefore) + ", " + Number(sound.PauseAfter) + ")",
            PauseStep pause => "Pauza(" + Number(pause.Milliseconds) + ")",
            BuiltinStep builtin => builtin.Method + "(" + string.Join(", ", builtin.Arguments) + ")",
            CallStep call => call.Function + "(" + string.Join(", ", call.Arguments) + ")",
            CommentStep comment => "'" + comment.Text,
            _ => throw new ArgumentOutOfRangeException(nameof(step), step.GetType().Name, "Neznamy krok hlasenia.")
        };
        return step is CommentStep || step.Comment is null ? text : text + " '" + step.Comment;
    }

    // popis, poradove argumenty a obmedzenie na stanice
    private static IEnumerable<string> AttributeArguments(ScriptReport report)
    {
        yield return VbText.Literal(report.Description);
        foreach (var argument in report.Arguments)
            yield return argument;
        if (report.ForStations is { } forStations)
            yield return "ProStanice:=" + VbText.Literal(forStations);
        if (report.NotStations is { } notStations)
            yield return "MimoStanice:=" + VbText.Literal(notStations);
    }

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);
}
