namespace ToolsCore.Iniss.TabTab;

/// <summary>
/// Polozka lavej strany pravidla s udalostou (<c>#SWITCH</c>, <c>#MERGE</c>, <c>#MERGE2</c>).
/// </summary>
public sealed class TabTabItem
{
    /// <summary>Poradie polozky v zozname (od 0).</summary>
    public int Index { get; init; }

    /// <summary>Text polozky tak, ako je v subore (bez okolitych medzier).</summary>
    public string Text { get; init; } = "";

    /// <summary>Usek polozky v texte sekcie.</summary>
    public TabTabSpan Span { get; init; }

    /// <summary>Ci je polozka podmienkou (vyrazom).</summary>
    public bool IsCondition { get; init; }

    /// <summary>Ci je polozka oddelovacom (<c>#MERGE</c>, <c>#MERGE2</c>).</summary>
    public bool IsSeparator { get; init; }

    /// <summary>Text polozky po dekodovani (<see cref="TabTabText.Decode"/>) - len pri textoch a oddelovacoch.</summary>
    public TabTabText? Decoded { get; init; }
}
