namespace ToolsCore.Iniss.Tables;

/// <summary>Druh zistenia o linke alebo tabuli.</summary>
public enum DriverLineProblemCode
{
    /// <summary>Cislo linky mimo 1-100 (parameter: cislo).</summary>
    LineOutOfRange,

    /// <summary>Cislo linky uz pouziva skorsia sekcia (parametre: cislo, sekcia).</summary>
    LineDuplicate,

    /// <summary>Tabula nepatri na linku s protokolom (parametre: tabula, vyrobca, trieda).</summary>
    WrongFamily,

    /// <summary>Linka nema ziadnu tabulu.</summary>
    NoTables,

    /// <summary>Tabula s automatickym priradenim nema linku s vhodnym protokolom.</summary>
    NoAutomaticLine,

    /// <summary>Linku tabule neobsluhuje ziadna sekcia Driver (parameter: cislo linky).</summary>
    NoDriver
}
