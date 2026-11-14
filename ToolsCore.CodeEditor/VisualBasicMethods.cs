using ScintillaNET;

namespace ToolsCore.CodeEditor;

/// <summary>
/// Farba nazvu metody v kode VB.NET. Lexer <c>vb</c> nema styl pre metody, nazov sa preto prefarbi indikatorom
/// <see cref="IndicatorStyle.TextFore" /> nad textom: identifikator, za ktorym nasleduje <c>(</c> - volanie aj definicia
/// <c>Function</c>/<c>Sub</c>, napr. <c>.DatumCasOdj(True).AddSeconds(-600)</c>. Bez prekladaca sa neda odlisit pole od
/// metody, vynechaju sa aspon deklaracie (<c>Dim pole(5)</c>), typy (<c>New Zoznam(</c>, <c>As Zoznam(Of</c>) a klucove slova
/// (<c>CType(</c>).
/// </summary>
public static class VisualBasicMethods
{
    /// <summary>
    /// Indikator nazvu metody (8 a 9 su podciarknutia chyby a upozornenia).
    /// </summary>
    public const int Indicator = 10;

    // klucove slova, za ktorymi identifikator s ( nie je volanie metody, ale deklaracia alebo typ
    private static readonly HashSet<string> NotCallAfter = new(StringComparer.OrdinalIgnoreCase)
    {
        "Dim", "ReDim", "Const", "New", "As", "Of", "Inherits", "Implements"
    };

    /// <summary>
    /// Nastavi indikator na farbu nazvu metody.
    /// </summary>
    /// <param name="scintilla">editor</param>
    /// <param name="color">farba nazvu metody</param>
    public static void Configure(Scintilla scintilla, Color color)
    {
        ArgumentNullException.ThrowIfNull(scintilla);
        scintilla.Indicators[Indicator].Style = IndicatorStyle.TextFore;
        scintilla.Indicators[Indicator].ForeColor = color;
    }

    /// <summary>
    /// Oznaci nazvy metod v riadkoch <paramref name="firstLine" /> az <paramref name="lastLine" /> (vratane); predtym v nich
    /// indikator zmaze.
    /// </summary>
    /// <param name="scintilla">editor s lexerom <c>vb</c></param>
    /// <param name="firstLine">prvy riadok</param>
    /// <param name="lastLine">posledny riadok</param>
    public static void Mark(Scintilla scintilla, int firstLine, int lastLine)
    {
        ArgumentNullException.ThrowIfNull(scintilla);
        if (scintilla.Lines.Count == 0)
            return;
        firstLine = Math.Clamp(firstLine, 0, scintilla.Lines.Count - 1);
        lastLine = Math.Clamp(lastLine, firstLine, scintilla.Lines.Count - 1);
        var start = scintilla.Lines[firstLine].Position;
        var end = scintilla.Lines[lastLine].EndPosition;
        if (end <= start)
            return;

        // styly lexera vzniknu az pri kresleni - pred citanim stylov sa rozsah nastyluje
        scintilla.Colorize(start, end);
        scintilla.IndicatorCurrent = Indicator;
        scintilla.IndicatorClearRange(start, end - start);
        var text = scintilla.GetTextRange(start, end - start);
        foreach (var (offset, length) in Methods(text, index => scintilla.GetStyleAt(start + index)))
            scintilla.IndicatorFillRange(start + offset, length);
    }

    /// <summary>
    /// Nazvy metod v texte (poloha a dlzka).
    /// </summary>
    /// <param name="text">text</param>
    /// <param name="style">styl lexera znaku na danej polohe textu</param>
    public static IEnumerable<(int Start, int Length)> Methods(string text, Func<int, int> style)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(style);
        var previousKeyword = "";
        var i = 0;
        while (i < text.Length)
        {
            var kind = style(i);
            if (kind != Style.Vb.Identifier && kind != Style.Vb.Keyword)
            {
                // iny token (operator, cislo, retazec...) ukonci suvislost s predchadzajucim klucovym slovom
                if (!char.IsWhiteSpace(text[i]))
                    previousKeyword = "";
                i++;
                continue;
            }

            var begin = i;
            while (i < text.Length && style(i) == kind && IsWordChar(text[i]))
                i++;
            if (i == begin)
            {
                i++;
                continue;
            }

            var word = text[begin..i];
            if (kind == Style.Vb.Keyword)
            {
                previousKeyword = word;
                continue;
            }

            if (IsCall(text, i) && !NotCallAfter.Contains(previousKeyword))
                yield return (begin, i - begin);
            previousKeyword = "";
        }
    }

    private static bool IsWordChar(char c) => char.IsLetterOrDigit(c) || c == '_';

    // za identifikatorom nasleduje ( (aj po medzerach), ale nie (Of - genericky typ
    private static bool IsCall(string text, int index)
    {
        while (index < text.Length && text[index] is ' ' or '\t')
            index++;
        if (index >= text.Length || text[index] != '(')
            return false;

        index++;
        while (index < text.Length && text[index] is ' ' or '\t')
            index++;
        return !(index + 2 <= text.Length && text.AsSpan(index, 2).Equals("Of", StringComparison.OrdinalIgnoreCase) &&
                 (index + 2 == text.Length || !IsWordChar(text[index + 2])));
    }
}
