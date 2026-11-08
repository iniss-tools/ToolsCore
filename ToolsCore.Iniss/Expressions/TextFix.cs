namespace ToolsCore.Iniss.Expressions;

/// <summary>
/// Navrhovana oprava - nazov a zoznam uprav (neprekryvajucich sa, v lubovolnom poradi).
/// </summary>
public sealed record TextFix(string Title, IReadOnlyList<TextEdit> Edits)
{
    /// <summary>Oprava s jedinou upravou.</summary>
    public static TextFix Single(string title, int start, int length, string newText) => new(title, [new TextEdit(start, length, newText)]);

    /// <summary>Posunie vsetky upravy o <paramref name="offset"/> znakov.</summary>
    public TextFix Shift(int offset) => new(Title, Edits.Select(e => e.Shift(offset)).ToList());

    /// <summary>
    /// Pouzije upravy na text (od konca, aby sa pozicie neposuvali).
    /// </summary>
    public string Apply(string text)
    {
        var sb = new StringBuilder(text);
        foreach (var e in Edits.OrderByDescending(e => e.Start))
        {
            sb.Remove(e.Start, e.Length);
            sb.Insert(e.Start, e.NewText);
        }
        return sb.ToString();
    }
}
