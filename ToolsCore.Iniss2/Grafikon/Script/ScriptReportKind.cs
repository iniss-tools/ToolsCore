namespace ToolsCore.Iniss2.Grafikon.Script;

/// <summary>
/// Druh hlasenia podla oznacenia funkcie skriptu. Nazov oznacenia v kode urcuje <see cref="ScriptReportKinds" />.
/// </summary>
public enum ScriptReportKind
{
    /// <summary>Zakladne hlasenie vlaku (<c>ZakladniHlaseni</c>).</summary>
    Basic,

    /// <summary>Doplnkove hlasenie vlaku (<c>DoplnkoveHlaseni</c>).</summary>
    Supplementary,

    /// <summary>Ostatne hlasenie (<c>OstatniHlaseni</c>).</summary>
    Other,

    /// <summary>Dodatok vlaku (<c>Dodatek</c>).</summary>
    Addition,

    /// <summary>Vyluka (<c>Vyluka</c>).</summary>
    Exclusion,

    /// <summary>Odklon (<c>Odklon</c>).</summary>
    Diversion,

    /// <summary>Vzor dodatku so zoznamom stanic (<c>VzorDodatku</c>).</summary>
    AdditionPattern,

    /// <summary>Vzor vyluky so zoznamom stanic (<c>VzorVyluky</c>).</summary>
    ExclusionPattern,

    /// <summary>Vzor odklonu so zoznamom stanic (<c>VzorOdklonu</c>).</summary>
    DiversionPattern,

    /// <summary>Hlasenie bez druhu, napr. znelka (<c>InterpreterAttribute</c>).</summary>
    Generic
}
