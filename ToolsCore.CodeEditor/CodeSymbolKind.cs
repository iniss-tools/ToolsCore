namespace ToolsCore.CodeEditor;

/// <summary>
/// Druh mena pri dopĺňaní.
/// </summary>
public enum CodeSymbolKind
{
    /// <summary>Clen objektu (napr. vlaku) - ponukne sa za bodkou.</summary>
    Member,

    /// <summary>Metoda objektu - ponukne sa za bodkou.</summary>
    MemberMethod,

    /// <summary>Meno dostupne vsade (typ, parameter, funkcia).</summary>
    Global,

    /// <summary>Klucove slovo jazyka.</summary>
    Keyword
}
