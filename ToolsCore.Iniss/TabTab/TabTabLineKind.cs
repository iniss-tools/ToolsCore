namespace ToolsCore.Iniss.TabTab;

/// <summary>
/// Druh logickeho riadka sekcie TabTab (klasifikacia INISSu).
/// </summary>
public enum TabTabLineKind
{
    /// <summary>Prazdny riadok.</summary>
    Empty,

    /// <summary>Komentar <c>; …</c>.</summary>
    Comment,

    /// <summary>Hlavicka sekcie <c>[Nazov]</c> (v texte sekcie by nemala byt - GVDEditor sekcie drzi zvlast).</summary>
    SectionHeader,

    /// <summary><c>[</c> bez <c>]</c> - INISS riadok ignoruje.</summary>
    BadSectionHeader,

    /// <summary>Pravidlo <c>lava = prava</c>.</summary>
    Rule,

    /// <summary>Riadok bez <c>=</c> - volby sekcie (<c>IgnoreCase</c>, <c>ViewValues</c>).</summary>
    Options
}
