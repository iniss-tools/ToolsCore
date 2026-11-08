namespace ToolsCore.Iniss.TabTab;

/// <summary>
/// Jeden krok skladania - na zobrazenie postupu v nahlade.
/// </summary>
/// <param name="Source">Zdroj (napr. <c>#SWITCH</c>, <c>TTexts</c>, <c>vlastná hodnota</c>, <c>spôsob plnenia 3</c>).</param>
/// <param name="Value">Vysledok zdroja.</param>
/// <param name="Note">Poznamka (ktora polozka platila, ci sa odlozilo …).</param>
public sealed record TabTabComposeStep(string Source, TabTabValue Value, string Note = "");
