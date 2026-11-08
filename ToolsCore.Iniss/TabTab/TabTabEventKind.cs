namespace ToolsCore.Iniss.TabTab;

/// <summary>
/// Udalost na pravej strane pravidla.
/// </summary>
public enum TabTabEventKind
{
    /// <summary>Obycajne pravidlo - prava strana je text INISSu.</summary>
    None,

    /// <summary><c>#SWITCH</c> - dvojice podmienka, text.</summary>
    Switch,

    /// <summary><c>#MERGE</c> - trojice podmienka, oddelovac, text.</summary>
    Merge,

    /// <summary><c>#MERGE2</c> - trojice podmienka, oddelovac, text.</summary>
    Merge2,

    /// <summary><c>#VYLUKA</c>.</summary>
    Vyluka,

    /// <summary><c>#ODKLON</c>.</summary>
    Odklon,

    /// <summary><c>#POZODJ_&lt;kolaj&gt;</c>.</summary>
    PozOdj,

    /// <summary>Ine meno zacinajuce <c>#</c> - INISS hlasi <c>unknown magic item</c>.</summary>
    Unknown
}
