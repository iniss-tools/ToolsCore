namespace ToolsCore.Iniss.StateDgm;

/// <summary>
/// Co stav robi s tabulami (<c>DoState</c> / <c>UndoState</c>, trieda <c>SVFTableSet</c>).
/// </summary>
public sealed class StateDgmTableSet : StateDgmElement
{
    /// <summary>Trieda funkcie stavu - INISS ma len <c>SVFTableSet</c>.</summary>
    public string Class { get; set; } = StateDgmKeys.ClassTableSet;

    /// <summary><c>JeNaOdjezdové</c></summary>
    public bool OnDepartureTable { get; set; }

    /// <summary><c>JeNaPříjezdové</c></summary>
    public bool OnArrivalTable { get; set; }

    /// <summary><c>JeNaSměrových</c></summary>
    public bool OnPlatformTables { get; set; }

    /// <summary><c>JeZobrazenaPozice</c></summary>
    public bool ShowPosition { get; set; }

    /// <summary><c>JeZobrazenaKolej</c></summary>
    public bool ShowTrack { get; set; }

    /// <summary>Hlboka kopia.</summary>
    public StateDgmTableSet Clone()
    {
        var c = new StateDgmTableSet
        {
            Class = Class, OnDepartureTable = OnDepartureTable, OnArrivalTable = OnArrivalTable,
            OnPlatformTables = OnPlatformTables, ShowPosition = ShowPosition, ShowTrack = ShowTrack
        };
        c.Extras.AddRange(Extras.Select(e => e.Clone()));
        return c;
    }
}
