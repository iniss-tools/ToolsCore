namespace ToolsCore.Iniss.StateDgm;

/// <summary>
/// Stav vlaku v kategorii (skupina <c>State</c>).
/// </summary>
public sealed class StateDgmState : StateDgmElement
{
    /// <summary>Kluc stavu; prvy stav kategorie je <c>#Start</c>.</summary>
    public string Key { get; set; } = "";

    /// <summary>Nazov pre obsluhu.</summary>
    public string Name { get; set; } = "";

    /// <summary>Cislo ikony 0-5.</summary>
    public int Icon { get; set; }

    /// <summary>Priznaky stavu.</summary>
    public StateDgmAttr Attr { get; set; }

    /// <summary>Rezim automatiky (cislo alebo vyraz); null = kluc chyba (rucne).</summary>
    public StateDgmDynamic? AutoMode { get; set; }

    /// <summary>Casovy bod automatiky (1 prichod, 2 odchod) alebo vyraz.</summary>
    public StateDgmDynamic? AutoTimePoint { get; set; }

    /// <summary>Posun voci casovemu bodu v sekundach alebo vyraz.</summary>
    public StateDgmDynamic? AutoTimePointAdd { get; set; }

    /// <summary>Variant hlasenia (1 kratke, 2 dlhe) alebo vyraz.</summary>
    public StateDgmDynamic? AutoModif { get; set; }

    /// <summary>Podmienka automatickeho prechodu (vyraz).</summary>
    public string? AutoCondition { get; set; }

    /// <summary>Udalost ILTISu, na ktoru automatika caka (vyraz s konstantami VVC/OVC/ZCV/Vj/Odj).</summary>
    public StateDgmDynamic? Wait { get; set; }

    /// <summary>Starsia podoba <see cref="Wait" /> ako vyraz; ciselny WaitPath sa pri nacitani prevedie na <c>Wait=VVC</c>.</summary>
    public StateDgmDynamic? WaitPath { get; set; }

    /// <summary>Poradie predvoleneho tlacidla od 1; 0 = bez predvoleneho.</summary>
    public int DefaultControl { get; set; }

    /// <summary>Co stav robi s tabulami.</summary>
    public StateDgmTableSet? DoState { get; set; }

    /// <summary>Opacna cast pri vrateni spat.</summary>
    public StateDgmTableSet? UndoState { get; set; }

    /// <summary>Akcie.</summary>
    public List<StateDgmEvent> Events { get; } = [];

    /// <summary>Tlacidla.</summary>
    public List<StateDgmControl> Controls { get; } = [];

    /// <summary>Startery.</summary>
    public List<StateDgmStarter> Starters { get; } = [];

    /// <summary>Casove body stavu (napr. <see cref="StateDgmKeys.StartTime" />); platia len v tomto stave.</summary>
    public List<StateDgmTimePoint> TimePoints { get; } = [];

    /// <summary>Stav ma zapnutu automatiku (AutoMode je vyraz alebo nenulove cislo).</summary>
    public bool HasAutomation => AutoMode != null && (AutoMode.IsExpression || AutoMode.Number != 0);

    /// <summary>Akcia podla kluca.</summary>
    public StateDgmEvent? FindEvent(string key) => Events.FirstOrDefault(e => e.Key == key);

    /// <inheritdoc />
    public override string ToString() => string.IsNullOrEmpty(Name) ? Key : $"{Key} ({Name})";
}
