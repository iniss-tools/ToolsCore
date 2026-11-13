namespace ToolsCore.CodeEditor;

/// <summary>
/// Meno, ktore editor ponukne pri dopĺňaní.
/// </summary>
/// <param name="Name">meno, ktore sa vlozi</param>
/// <param name="Kind">druh mena</param>
/// <param name="Signature">zapis s parametrami a typom pre popis</param>
/// <param name="Description">popis pre obsluhu; prazdny = bez popisu</param>
public sealed record CodeSymbol(string Name, CodeSymbolKind Kind, string Signature, string Description);
