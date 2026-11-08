namespace ToolsCore.Iniss.Registry;

/// <summary>Skupina sekcii v okne nastaveni (podla ucelu, nie podla nazvu sekcie).</summary>
public enum RegGroup
{
    /// <summary>Tabule - linky (Driver*) a prevadzka tabul (Tables).</summary>
    Boards,

    /// <summary>Zvuk a hlasenia.</summary>
    Sound,

    /// <summary>Grafikon.</summary>
    Timetable,

    /// <summary>Prostredie obsluhy (Environment).</summary>
    Operator,

    /// <summary>Logy.</summary>
    Logs,

    /// <summary>Rozhrania na ine systemy.</summary>
    Interfaces,

    /// <summary>Nazvy suborov.</summary>
    Files,

    /// <summary>Vlastnosti nasadenia.</summary>
    Installation,

    /// <summary>Vzhlad - farby, pismo, rozlozenie okna.</summary>
    Appearance,

    /// <summary>Ladenie a servisne hodnoty.</summary>
    Debug
}
