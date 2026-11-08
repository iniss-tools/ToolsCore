namespace ToolsCore.Iniss.TabTab;

/// <summary>
/// Vysledok skladania textu stlpca.
/// </summary>
/// <param name="Value">Vysledny text a pismo.</param>
/// <param name="Steps">Postup po krokoch.</param>
public sealed record TabTabComposeResult(TabTabValue Value, IReadOnlyList<TabTabComposeStep> Steps)
{
    /// <summary>Chyba vyhodnotenia (delenie nulou …), ak nastala.</summary>
    public string? Error { get; init; }
}
