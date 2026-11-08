namespace ToolsCore.Iniss.TabTab;

/// <summary>
/// Jeden logicky riadok sekcie (po spojeni pokracovani <c>\</c>).
/// </summary>
public sealed class TabTabLine
{
    /// <summary>Druh riadka.</summary>
    public TabTabLineKind Kind { get; init; }

    /// <summary>Cislo prveho fyzickeho riadka (od 0).</summary>
    public int LineIndex { get; init; }

    /// <summary>Usek celeho logickeho riadka v texte (od prveho po posledny fyzicky riadok).</summary>
    public TabTabSpan Span { get; init; }

    /// <summary>Lava strana pravidla (text pre tabulu / zoznam poloziek), bez orezania.</summary>
    public string Left { get; init; } = "";

    /// <summary>Usek lavej strany.</summary>
    public TabTabSpan LeftSpan { get; init; }

    /// <summary>Prava strana pravidla (text INISSu / udalost), orezana sprava.</summary>
    public string Right { get; init; } = "";

    /// <summary>Usek pravej strany (orezanej).</summary>
    public TabTabSpan RightSpan { get; init; }

    /// <summary>Udalost pravej strany.</summary>
    public TabTabEventKind Event { get; init; }

    /// <summary>Kolaj pri <see cref="TabTabEventKind.PozOdj"/>.</summary>
    public string? PozOdjTrack { get; init; }

    /// <summary>Polozky lavej strany pri <c>#SWITCH</c>/<c>#MERGE</c>/<c>#MERGE2</c>.</summary>
    public IReadOnlyList<TabTabItem> Items { get; init; } = [];

    /// <summary>Pri <see cref="TabTabLineKind.Options"/>: volby (polozky oddelene ciarkou).</summary>
    public IReadOnlyList<string> Options { get; init; } = [];

    /// <summary>Text komentara alebo nazov sekcie pri prislusnych druhoch.</summary>
    public string Text { get; init; } = "";

    /// <summary>Ci je riadok pravidlom s udalostou.</summary>
    public bool IsEventRule => Kind == TabTabLineKind.Rule && Event != TabTabEventKind.None;
}
