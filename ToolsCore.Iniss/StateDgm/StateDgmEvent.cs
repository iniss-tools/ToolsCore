namespace ToolsCore.Iniss.StateDgm;

/// <summary>
/// Akcia stavu (skupina <c>Event</c>).
/// </summary>
public sealed class StateDgmEvent : StateDgmElement
{
    /// <summary>Kluc akcie; odkazuju sa nan ovladace a startery.</summary>
    public string Key { get; set; } = "";

    /// <summary>Volitelny nazov pre obsluhu.</summary>
    public string? Name { get; set; }

    /// <summary>Cislo ikony; INISS predvolene -1.</summary>
    public int? Icon { get; set; }

    /// <summary>Volitelny komentar.</summary>
    public string? Comment { get; set; }

    /// <summary>Trieda akcie.</summary>
    public string Class { get; set; } = "SDEventUniPos";

    /// <summary>Stav, do ktoreho sa vlak prepne.</summary>
    public string? NextState { get; set; }

    /// <summary>Typ hlasenia z lokalneho Categori.txt.</summary>
    public string? ReportKey { get; set; }

    /// <summary>Dialog pre <c>SDEventWithDialog</c>.</summary>
    public string? Dialog { get; set; }

    /// <summary><c>Pozice pro příjezd</c> (0/1).</summary>
    public int? PositionForArrival { get; set; }

    /// <summary><c>Pozice pro odjezd</c> (0/1).</summary>
    public int? PositionForDeparture { get; set; }

    /// <summary><c>Kopírovat pozici</c> (0/1, predvolene 1).</summary>
    public int? CopyPosition { get; set; }

    /// <summary><c>ModifiReport</c> (0/1).</summary>
    public int? ModifyReport { get; set; }

    /// <summary><c>Dotaz na hlášení</c> (0/1).</summary>
    public int? AskBeforeReport { get; set; }

    /// <summary><c>SkrytOdkryt</c> (0/1).</summary>
    public int? HideShow { get; set; }

    /// <summary><c>Zpoždění na příjezdu</c> (0/1) pre <c>SDEventVlakAttr</c>.</summary>
    public int? DelayArrival { get; set; }

    /// <summary><c>Zpoždění na odjezdu</c> (0/1) pre <c>SDEventVlakAttr</c>.</summary>
    public int? DelayDeparture { get; set; }

    /// <summary>Vnorena udalost <c>DoEvent</c> (drzi sa ako strom).</summary>
    public StateDgmGroup? DoEvent { get; set; }

    /// <summary>Vnorena udalost <c>UndoEvent</c> (drzi sa ako strom).</summary>
    public StateDgmGroup? UndoEvent { get; set; }

    /// <summary>Akcia meni stav (ma NextState).</summary>
    public bool ChangesState => !string.IsNullOrEmpty(NextState);

    /// <inheritdoc />
    public override string ToString() => Key;
}
