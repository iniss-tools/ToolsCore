namespace ToolsCore.StateDgm;

/// <summary>
///     Priznaky stavu (kluc <c>Attr</c>); mena bitov su z komentarov v suboroch INISSu.
/// </summary>
[Flags]
public enum StateDgmAttr
{
    /// <summary>Bez priznakov.</summary>
    None = 0,

    /// <summary><c>SVSA_Stoji</c> - vlak stoji v stanici.</summary>
    Stoji = 0x08,

    /// <summary><c>SVSA_PotvrzenaKolej</c> - kolaj je potvrdena.</summary>
    PotvrzenaKolej = 0x10,

    /// <summary><c>SVSA_Odbaven</c> - vlak je odbaveny.</summary>
    Odbaven = 0x20,

    /// <summary><c>SVSA_Shadow</c> - vlak je „v tieni“, uz sa nezobrazuje.</summary>
    Shadow = 0x40,

    /// <summary><c>SVSA_NyniStoji</c> - vlak prave teraz stoji.</summary>
    NyniStoji = 0x80
}

/// <summary>
///     Udalosti ILTISu, na ktore moze automaticka akcia cakat (kluc <c>Wait</c>); hodnoty su konstanty jazyka vyrazov.
/// </summary>
[Flags]
public enum StateDgmWaitEvent : uint
{
    /// <summary>Necaka.</summary>
    None = 0,

    /// <summary>Odchod vlaku (sprava 080-3).</summary>
    Odj = 0x08000000,

    /// <summary>Vchod vlaku do stanice (sprava 080-2).</summary>
    Vj = 0x10000000,

    /// <summary>Zavedenie cisla vlaku - pri ILTISe sa nepouziva.</summary>
    ZCV = 0x20000000,

    /// <summary>Odchodova vlakova cesta (sprava 080-4).</summary>
    OVC = 0x40000000,

    /// <summary>Vchodova vlakova cesta (sprava 080-4).</summary>
    VVC = 0x80000000
}

/// <summary>
///     Rezim automatiky stavu (kluc <c>AutoMode</c>).
/// </summary>
public enum StateDgmAutoMode
{
    /// <summary>Rucne - stav sa prepne len tlacidlom.</summary>
    Manual = 0,

    /// <summary>Poloautomat - obsluha prepnutie potvrdzuje.</summary>
    SemiAutomatic = 1,

    /// <summary>Automat - prepne sa bez zasahu.</summary>
    Automatic = 2
}

/// <summary>
///     Casovy bod automatiky (kluc <c>AutoTimePoint</c>).
/// </summary>
public enum StateDgmAutoTimePoint
{
    /// <summary>Cas prichodu (pristavenia).</summary>
    Arrival = 1,

    /// <summary>Cas odchodu.</summary>
    Departure = 2
}

/// <summary>
///     Hodnota, ktoru INISS cita dvakrat - ako cislo (<c>I:</c>) alebo ako vyraz (<c>S:</c>) vyhodnocovany pre kazdy vlak.
/// </summary>
public sealed class StateDgmDynamic : IEquatable<StateDgmDynamic>
{
    private StateDgmDynamic(int? number, string? expression)
    {
        Number = number;
        Expression = expression;
    }

    /// <summary>Ciselna hodnota (zapis <c>I:"kluc"=n</c>).</summary>
    public int? Number { get; }

    /// <summary>Vyraz (zapis <c>S:"kluc"="vyraz"</c>).</summary>
    public string? Expression { get; }

    /// <summary>Hodnota je vyraz.</summary>
    public bool IsExpression => Expression != null;

    /// <summary>Text hodnoty tak, ako sa zobrazi (cislo alebo vyraz).</summary>
    public string Text => Expression ?? Number!.Value.ToString();

    /// <summary>Ciselna hodnota.</summary>
    public static StateDgmDynamic FromNumber(int n) => new(n, null);

    /// <summary>Vyraz. Ak je to cely ciselny literal, vrati ciselnu hodnotu (INISS ich cita rovnako).</summary>
    public static StateDgmDynamic FromExpression(string expr)
    {
        var t = expr.Trim();
        return StateDgmReader.TryStrtol(t, out var n) ? new StateDgmDynamic(n, null) : new StateDgmDynamic(null, expr);
    }

    /// <summary>Konstanta pre kluc <c>Wait</c> - meno udalosti, alebo null pre <see cref="StateDgmWaitEvent.None" />.</summary>
    public static StateDgmDynamic? FromWait(StateDgmWaitEvent e) => e == StateDgmWaitEvent.None ? null : new StateDgmDynamic(null, WaitName(e));

    /// <summary>Mena udalosti spojene <c>|</c> (napr. <c>VVC|Vj</c>).</summary>
    public static string WaitName(StateDgmWaitEvent e) =>
        string.Join("|", Enum.GetValues<StateDgmWaitEvent>().Where(x => x != StateDgmWaitEvent.None && e.HasFlag(x)).Select(x => x.ToString()));

    /// <inheritdoc />
    public bool Equals(StateDgmDynamic? other) => other != null && Number == other.Number && Expression == other.Expression;

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as StateDgmDynamic);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Number, Expression);

    /// <inheritdoc />
    public override string ToString() => Text;
}

/// <summary>
///     Mena klucov a skupin suboru StateDgm.txt a zabudovane hodnoty.
/// </summary>
public static class StateDgmKeys
{
    // bloky
    public const string CTRLS = "StateDgmCtrls";
    public const string CTRL_DESIGN = "CtrlDesign";
    public const string STATE_DGM = "StateDgm";
    public const string CATEGORIE = "Categorie";
    public const string DESIGN = "Design";
    public const string STATE = "State";
    public const string EVENT = "Event";
    public const string CONTROL = "Control";
    public const string STARTER = "Starter";
    public const string TIME_POINT = "TimePoint";
    public const string DO_STATE = "DoState";
    public const string UNDO_STATE = "UndoState";
    public const string DO_EVENT = "DoEvent";
    public const string UNDO_EVENT = "UndoEvent";

    // spolocne
    public const string KEY = "Key";
    public const string NAME = "Name";
    public const string COMMENT = "Comment";
    public const string ICON = "Icon";
    public const string CLASS = "Class";

    // CtrlDesign
    public const string NUM_DESIGNS = "NumDesigns";
    public const string BITMAPS = "Bitmaps";
    public const string DEF_PUSH_BTN = "DefPushBtn";
    public const string CLASS_DESIGN_BTN = "SDCCtrlDesignBtn";
    public const string CLASS_DESIGN = "SDCCtrlDesign";

    // hlavicka
    public const string NUM_TIME_POINTS = "NumTimePoints";
    public const string NUM_CATEGORIES = "NumCategories";
    public const string IND_CAT = "IndCat";
    public const string TIME_POINT_KEY1 = "TimePointKey1";
    public const string TIME_POINT_KEY2 = "TimePointKey2";
    public const string TIME_POINT_OFFSET1 = "TimePointOffset1";
    public const string TIME_POINT_OFFSET2 = "TimePointOffset2";
    public const string OPERATOR = "Operator";
    public const string OPERATOR_MIN = "min";
    public const string OPERATOR_MAX = "max";

    // kategoria
    public const string NUM_STATES = "NumStates";

    // stav
    public const string ATTR = "Attr";
    public const string AUTO_MODE = "AutoMode";
    public const string AUTO_TIME_POINT = "AutoTimePoint";
    public const string AUTO_TIME_POINT_ADD = "AutoTimePointAdd";
    public const string AUTO_MODIF = "AutoModif";
    public const string AUTO_CONDITION = "AutoCondition";
    public const string WAIT = "Wait";
    public const string WAIT_PATH = "WaitPath";
    public const string DEFAULT_CONTROL = "DefaultControl";
    public const string NUM_EVENTS = "NumEvents";
    public const string NUM_CONTROLS = "NumControls";
    public const string NUM_STARTERS = "NumStarters";

    // DoState / UndoState
    public const string CLASS_TABLE_SET = "SVFTableSet";
    public const string ON_DEP_TABLE = "JeNaOdjezdové";
    public const string ON_ARR_TABLE = "JeNaPříjezdové";
    public const string ON_PLATFORM_TABLE = "JeNaSměrových";
    public const string SHOW_POSITION = "JeZobrazenaPozice";
    public const string SHOW_TRACK = "JeZobrazenaKolej";
    public const string ON_DEP_TABLE_OLD = "OnDepTable";
    public const string ON_ARR_TABLE_OLD = "OnArrTable";
    public const string ON_PLATFORM_TABLE_OLD = "OnPlatformTable";
    public const string SHOW_POSITION_OLD = "PlatformNumber";
    public const string SHOW_TRACK_OLD = "TrackNumber";

    // akcia
    public const string NEXT_STATE = "NextState";
    public const string REPORT_KEY = "ReportKey";
    public const string DIALOG = "Dialog";
    public const string POS_FOR_ARRIVAL = "Pozice pro příjezd";
    public const string POS_FOR_DEPARTURE = "Pozice pro odjezd";
    public const string COPY_POSITION = "Kopírovat pozici";
    public const string MODIFY_REPORT = "ModifiReport";
    public const string ASK_REPORT = "Dotaz na hlášení";
    public const string HIDE_SHOW = "SkrytOdkryt";
    public const string DELAY_ARRIVAL = "Zpoždění na příjezdu";
    public const string DELAY_DEPARTURE = "Zpoždění na odjezdu";
    public const string REPORT_ABOUT = "ReportAbout";
    public const string POS_GROUP_TXT = "PosGroupTxt";
    public const string POS_TEXT1 = "PosText1";
    public const string POS_TEXT2 = "PosText2";

    // ovladac
    public const string CTRL_ID = "CtrlID";
    public const string DESIGN_KEY = "DesignKey";
    public const string EVENT_KEY = "EventKey";

    // starter
    public const string CLASS_STARTER = "SDStarterShape";
    public const string TIME_POINT_KEY = "TimePointKey";
    public const string TIME_OFFSET = "TimeOffset";
    public const string TIME_OFFSET_STEP = "TimeOffsetStep";
    public const string TIME_POINT_KEY_LAST = "TimePointKeyLast";
    public const string TIME_OFFSET_LAST = "TimeOffsetLast";
    public const string START_LATER_TOO = "StartLaterToo";

    /// <summary>Zauzivany kluc prveho stavu kategorie (INISS berie ako pociatocny prvy stav bez ohladu na kluc).</summary>
    public const string START_STATE = "#Start";

    /// <summary>
    ///     Casovy bod stavu, ktory INISS nastavi na cas vstupu vlaku do stavu - stav ho musi deklarovat
    ///     skupinou <c>TimePoint</c> s tymto klucom (bez zdrojovych bodov).
    /// </summary>
    public const string START_TIME = "#StartTime";

    /// <summary>Predvoleny vyraz <c>IndCat</c>, ked kluc chyba.</summary>
    public const string DEFAULT_IND_CAT = "INDCAT6";

    /// <summary>Zabudovane casove body (kluce, na ktore sa odkazuju startery a vlastne casove body).</summary>
    public static readonly string[] BuiltInTimePoints =
    [
        "#Pravidelný příjezd",
        "#Pravidelný odjezd",
        "#Předpokládaný příjezd",
        "#Předpokládaný odjezd"
    ];

    /// <summary>Triedy akcii, ktore INISS 3.39 pozna.</summary>
    public static readonly string[] EventClasses =
    [
        "SDEventUniPos",
        "SDEventWithDialog",
        "SDEventChangeState",
        "SDEventReportAboutState",
        "SDEventSelectState",
        "SDEventVlakAttr",
        "SDEventDoFunction",
        "SDEventUndo",
        "SDEventUndoFunction"
    ];

    /// <summary>Triedy akcii, ktore pouzivaju ReportKey/NextState a volby pozicie.</summary>
    public static readonly string[] EventClassesWithReport = ["SDEventUniPos", "SDEventReportAboutState", "SDEventVlakAttr"];

    /// <summary>Dialogy pre <c>SDEventWithDialog</c>.</summary>
    public static readonly string[] Dialogs = ["SDDlgKolej", "SDDlgZpozdeni", "SDDlgZpozdeniG"];

    /// <summary>Najvyssie cislo ikony kategorie aj stavu.</summary>
    public const int MAX_ICON = 5;
}

/// <summary>
///     Obrazky tlacidla - hodnota kluca <c>Bitmaps</c> v tvare <c>posun-normalny,so zameranim,stlaceny</c>.
/// </summary>
/// <param name="Offset">Posun v pase obrazkov.</param>
/// <param name="Normal">Obrazok pre bezny stav.</param>
/// <param name="Focused">Obrazok pre tlacidlo so zameranim.</param>
/// <param name="Pushed">Obrazok pre stlacene tlacidlo.</param>
public readonly record struct StateDgmBitmaps(int Offset, int Normal, int Focused, int Pushed)
{
    /// <summary>Rozlozi text kluca <c>Bitmaps</c>; false pri nespravnom tvare.</summary>
    public static bool TryParse(string? text, out StateDgmBitmaps value)
    {
        value = default;
        if (string.IsNullOrWhiteSpace(text)) return false;
        var dash = text.IndexOf('-');
        if (dash <= 0) return false;
        if (!int.TryParse(text[..dash].Trim(), out var offset)) return false;
        var parts = text[(dash + 1)..].Split(',');
        if (parts.Length != 3) return false;
        if (!int.TryParse(parts[0].Trim(), out var n) || !int.TryParse(parts[1].Trim(), out var f) || !int.TryParse(parts[2].Trim(), out var p)) return false;
        value = new StateDgmBitmaps(offset, n, f, p);
        return true;
    }

    /// <summary>Tlacidlo sa neda stlacit (vsetky tri obrazky rovnake).</summary>
    public bool IsStatic => Normal == Focused && Focused == Pushed;

    /// <summary>Indexy obrazkov v pase (posun + cislo).</summary>
    public (int Normal, int Focused, int Pushed) Absolute => (Offset + Normal, Offset + Focused, Offset + Pushed);

    /// <inheritdoc />
    public override string ToString() => $"{Offset}-{Normal},{Focused},{Pushed}";
}

/// <summary>
///     Spolocny zaklad typovanych prvkov - drzi polozky, ktorym model nerozumie, aby sa pri zapise zachovali.
/// </summary>
public abstract class StateDgmElement
{
    /// <summary>Kluce a skupiny z povodneho suboru, ktore model nepozna (zapisu sa na koniec bloku).</summary>
    public List<StateDgmItem> Extras { get; } = [];

    /// <summary>Cislo riadka v povodnom subore (0-based), -1 pre nove prvky.</summary>
    public int Line { get; set; } = -1;
}

/// <summary>
///     Vzhlad tlacidla (<c>StateDgmCtrls\CtrlDesign</c>, skupina <c>Design</c>).
/// </summary>
public sealed class StateDgmDesign : StateDgmElement
{
    /// <summary>Kluc vzhladu, na ktory sa odkazuju ovladace.</summary>
    public string Key { get; set; } = "";

    /// <summary>Obrazky tlacidla (<c>posun-normalny,zameranie,stlaceny</c>).</summary>
    public string Bitmaps { get; set; } = "0-0,1,2";

    /// <summary>Predvolene tlacidlo (reaguje na Enter).</summary>
    public bool DefaultPushButton { get; set; }

    /// <summary>Trieda ovladaca.</summary>
    public string Class { get; set; } = StateDgmKeys.CLASS_DESIGN_BTN;

    /// <summary>Rozlozene obrazky, null pri nespravnom tvare.</summary>
    public StateDgmBitmaps? ParsedBitmaps => StateDgmBitmaps.TryParse(Bitmaps, out var b) ? b : null;

    /// <inheritdoc />
    public override string ToString() => Key;
}

/// <summary>
///     Vlastny casovy bod odvodeny z dvoch inych (<c>TimePoint</c> v hlavicke).
/// </summary>
public sealed class StateDgmTimePoint : StateDgmElement
{
    /// <summary>Jedinecny kluc.</summary>
    public string Key { get; set; } = "";

    /// <summary>Nazov pre obsluhu.</summary>
    public string Name { get; set; } = "";

    /// <summary>Kluc prveho zdrojoveho casoveho bodu.</summary>
    public string TimePointKey1 { get; set; } = "";

    /// <summary>Kluc druheho zdrojoveho casoveho bodu.</summary>
    public string TimePointKey2 { get; set; } = "";

    /// <summary>Posun prveho casu v sekundach.</summary>
    public int Offset1 { get; set; }

    /// <summary>Posun druheho casu v sekundach.</summary>
    public int Offset2 { get; set; }

    /// <summary><c>min</c> alebo <c>max</c>.</summary>
    public string Operator { get; set; } = StateDgmKeys.OPERATOR_MIN;

    /// <inheritdoc />
    public override string ToString() => Key;
}

/// <summary>
///     Co stav robi s tabulami (<c>DoState</c> / <c>UndoState</c>, trieda <c>SVFTableSet</c>).
/// </summary>
public sealed class StateDgmTableSet : StateDgmElement
{
    /// <summary>Trieda funkcie stavu - INISS ma len <c>SVFTableSet</c>.</summary>
    public string Class { get; set; } = StateDgmKeys.CLASS_TABLE_SET;

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

/// <summary>
///     Akcia stavu (skupina <c>Event</c>).
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

/// <summary>
///     Tlacidlo v paneli stavu (skupina <c>Control</c>).
/// </summary>
public sealed class StateDgmControl : StateDgmElement
{
    /// <summary>Poradie tlacidla v paneli od 0.</summary>
    public int CtrlId { get; set; }

    /// <summary>Vzhlad z bloku CtrlDesign.</summary>
    public string DesignKey { get; set; } = "";

    /// <summary>Akcia po stlaceni; prazdne = len obrazok.</summary>
    public string EventKey { get; set; } = "";

    /// <inheritdoc />
    public override string ToString() => $"{CtrlId}: {DesignKey} → {EventKey}";
}

/// <summary>
///     Starter - opakovane spustanie akcie podla casu (skupina <c>Starter</c>).
/// </summary>
public sealed class StateDgmStarter : StateDgmElement
{
    /// <summary>Kluc startera.</summary>
    public string Key { get; set; } = "";

    /// <summary>Akcia, ktoru spusta.</summary>
    public string EventKey { get; set; } = "";

    /// <summary>Trieda - jedina je <c>SDStarterShape</c>.</summary>
    public string Class { get; set; } = StateDgmKeys.CLASS_STARTER;

    /// <summary>Casovy bod prveho spustenia.</summary>
    public string TimePointKey { get; set; } = "";

    /// <summary>Posun prveho spustenia v sekundach.</summary>
    public int TimeOffset { get; set; }

    /// <summary>Interval opakovania v sekundach; null = bez opakovania.</summary>
    public int? TimeOffsetStep { get; set; }

    /// <summary>Casovy bod posledneho spustenia; null = ako <see cref="TimePointKey" />.</summary>
    public string? TimePointKeyLast { get; set; }

    /// <summary>Posun posledneho spustenia v sekundach.</summary>
    public int? TimeOffsetLast { get; set; }

    /// <summary>Spustit hned, ked prvy termin uz uplynul.</summary>
    public bool StartLaterToo { get; set; }

    /// <inheritdoc />
    public override string ToString() => Key;
}

/// <summary>
///     Stav vlaku v kategorii (skupina <c>State</c>).
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

    /// <summary>Casove body stavu (napr. <see cref="StateDgmKeys.START_TIME" />); platia len v tomto stave.</summary>
    public List<StateDgmTimePoint> TimePoints { get; } = [];

    /// <summary>Stav ma zapnutu automatiku (AutoMode je vyraz alebo nenulove cislo).</summary>
    public bool HasAutomation => AutoMode != null && (AutoMode.IsExpression || AutoMode.Number != 0);

    /// <summary>Akcia podla kluca.</summary>
    public StateDgmEvent? FindEvent(string key) => Events.FirstOrDefault(e => e.Key == key);

    /// <inheritdoc />
    public override string ToString() => string.IsNullOrEmpty(Name) ? Key : $"{Key} ({Name})";
}

/// <summary>
///     Kategoria vlaku (<c>StateDgmCtrls\StateDgm\CategorieN</c>).
/// </summary>
public sealed class StateDgmCategory : StateDgmElement
{
    /// <summary>Kluc kategorie.</summary>
    public string Key { get; set; } = "";

    /// <summary>Nazov pre obsluhu.</summary>
    public string Name { get; set; } = "";

    /// <summary>Vysvetlenie, kedy kategoria plati.</summary>
    public string Comment { get; set; } = "";

    /// <summary>Cislo ikony 0-2 (cervene 3-5 si INISS odvodi sam).</summary>
    public int Icon { get; set; }

    /// <summary>Stavy; prvy ma byt <c>#Start</c>.</summary>
    public List<StateDgmState> States { get; } = [];

    /// <summary>Stav podla kluca.</summary>
    public StateDgmState? FindState(string key) => States.FirstOrDefault(s => s.Key == key);

    /// <inheritdoc />
    public override string ToString() => string.IsNullOrEmpty(Name) ? Key : Name;
}

/// <summary>
///     Upozornenie z nacitania (subor je platny, ale INISS by sa zachoval inak, nez autor cakal).
/// </summary>
/// <param name="Message">Text upozornenia.</param>
/// <param name="Line">Riadok (0-based), -1 ak sa neda urcit.</param>
public sealed record StateDgmLoadWarning(string Message, int Line);

/// <summary>
///     Typovany stavovy diagram - obsah suboru StateDgm.txt.
/// </summary>
public sealed class StateDgmDiagram
{
    /// <summary>Hlavickove komentare (<c>C:"…"</c>).</summary>
    public List<string> HeaderComments { get; } = [];

    /// <summary>Vzhlady tlacidiel.</summary>
    public List<StateDgmDesign> Designs { get; } = [];

    /// <summary>Vlastne casove body.</summary>
    public List<StateDgmTimePoint> TimePoints { get; } = [];

    /// <summary>Vyraz urcujuci kategoriu vlaku; null = predvolene <c>INDCAT6</c>.</summary>
    public string? IndCat { get; set; }

    /// <summary>Kategorie.</summary>
    public List<StateDgmCategory> Categories { get; } = [];

    /// <summary>Nezname polozky bloku <c>CtrlDesign</c>.</summary>
    public List<StateDgmItem> DesignExtras { get; } = [];

    /// <summary>Nezname polozky hlavicky <c>StateDgm</c>.</summary>
    public List<StateDgmItem> HeaderExtras { get; } = [];

    /// <summary>Nezname polozky skupiny <c>StateDgmCtrls</c> (mimo CtrlDesign a StateDgm).</summary>
    public List<StateDgmItem> CtrlsExtras { get; } = [];

    /// <summary>Nezname bloky na najvyssej urovni (mimo <c>StateDgmCtrls</c>).</summary>
    public List<StateDgmItem> RootExtras { get; } = [];

    /// <summary>Upozornenia z posledneho nacitania.</summary>
    public List<StateDgmLoadWarning> Warnings { get; } = [];

    /// <summary>Vyraz IndCat, ktory INISS skutocne pouzije.</summary>
    public string EffectiveIndCat => string.IsNullOrWhiteSpace(IndCat) ? StateDgmKeys.DEFAULT_IND_CAT : IndCat;

    /// <summary>Vsetky kluce casovych bodov - zabudovane aj vlastne.</summary>
    public IEnumerable<string> AllTimePointKeys => StateDgmKeys.BuiltInTimePoints.Concat(TimePoints.Select(t => t.Key));

    /// <summary>Vzhlad podla kluca.</summary>
    public StateDgmDesign? FindDesign(string key) => Designs.FirstOrDefault(d => d.Key == key);

    /// <summary>Kategoria podla kluca.</summary>
    public StateDgmCategory? FindCategory(string key) => Categories.FirstOrDefault(c => c.Key == key);

    /// <summary>Nacita diagram z textu suboru. Chyby syntaxe vyhadzuje <see cref="StateDgmParseException" />.</summary>
    public static StateDgmDiagram Parse(string text) => StateDgmConverter.FromTree(StateDgmReader.Read(text));

    /// <summary>Nacita diagram zo suboru v kodovani windows-1250.</summary>
    public static StateDgmDiagram Load(string path) => StateDgmConverter.FromTree(StateDgmReader.ReadFile(path));

    /// <summary>Zapise diagram do textu suboru (kanonicky tvar s tabulatormi a komentarmi).</summary>
    public string ToText() => StateDgmWriter.Write(this);

    /// <summary>Zapise diagram do suboru v kodovani windows-1250.</summary>
    public void Save(string path) => File.WriteAllText(path, ToText(), Tools.Encodings.Win1250);
}
