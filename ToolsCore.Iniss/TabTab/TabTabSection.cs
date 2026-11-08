namespace ToolsCore.Iniss.TabTab;

/// <summary>
/// Rozobrany text jednej sekcie TabTab.
/// </summary>
public sealed class TabTabSection
{
    /// <summary>Text, z ktoreho sekcia vznikla.</summary>
    public string Text { get; init; } = "";

    /// <summary>Logicke riadky v poradi.</summary>
    public IReadOnlyList<TabTabLine> Lines { get; init; } = [];

    /// <summary>Ci ma sekcia volbu <c>IgnoreCase</c> (prava strana sa porovnava bez ohladu na velkost pismen).</summary>
    public bool IgnoreCase { get; init; }

    /// <summary>Ci ma sekcia volbu <c>ViewValues</c>.</summary>
    public bool ViewValues { get; init; }

    /// <summary>Pravidla (riadky druhu <see cref="TabTabLineKind.Rule"/>).</summary>
    public IEnumerable<TabTabLine> Rules => Lines.Where(l => l.Kind == TabTabLineKind.Rule);

    /// <summary>
    /// Rozoberie text sekcie tak, ako ho cita INISS - spojenie riadkov s <c>\</c>,
    /// komentare, posledne neescapovane <c>=</c> ako oddelovac, <c>;</c> ako koniec riadka.
    /// </summary>
    public static TabTabSection Parse(string text) => TabTabSectionParser.Parse(text ?? "");
}
