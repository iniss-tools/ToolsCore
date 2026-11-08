namespace ToolsCore.Iniss.StateDgm;

/// <summary>
/// Mena klucov a skupin suboru StateDgm.txt a zabudovane hodnoty.
/// </summary>
public static class StateDgmKeys
{
    // bloky
    public const string Ctrls = "StateDgmCtrls";
    public const string CtrlDesign = "CtrlDesign";
    public const string StateDgm = "StateDgm";
    public const string Categorie = "Categorie";
    public const string Design = "Design";
    public const string State = "State";
    public const string Event = "Event";
    public const string Control = "Control";
    public const string Starter = "Starter";
    public const string TimePoint = "TimePoint";
    public const string DoState = "DoState";
    public const string UndoState = "UndoState";
    public const string DoEvent = "DoEvent";
    public const string UndoEvent = "UndoEvent";

    // spolocne
    public const string Key = "Key";
    public const string Name = "Name";
    public const string Comment = "Comment";
    public const string Icon = "Icon";
    public const string Class = "Class";

    // CtrlDesign
    public const string NumDesigns = "NumDesigns";
    public const string Bitmaps = "Bitmaps";
    public const string DefPushBtn = "DefPushBtn";
    public const string ClassDesignBtn = "SDCCtrlDesignBtn";
    public const string ClassDesign = "SDCCtrlDesign";

    // hlavicka
    public const string NumTimePoints = "NumTimePoints";
    public const string NumCategories = "NumCategories";
    public const string IndCat = "IndCat";
    public const string TimePointKey1 = "TimePointKey1";
    public const string TimePointKey2 = "TimePointKey2";
    public const string TimePointOffset1 = "TimePointOffset1";
    public const string TimePointOffset2 = "TimePointOffset2";
    public const string Operator = "Operator";
    public const string OperatorMin = "min";
    public const string OperatorMax = "max";

    // kategoria
    public const string NumStates = "NumStates";

    // stav
    public const string Attr = "Attr";
    public const string AutoMode = "AutoMode";
    public const string AutoTimePoint = "AutoTimePoint";
    public const string AutoTimePointAdd = "AutoTimePointAdd";
    public const string AutoModif = "AutoModif";
    public const string AutoCondition = "AutoCondition";
    public const string Wait = "Wait";
    public const string WaitPath = "WaitPath";
    public const string DefaultControl = "DefaultControl";
    public const string NumEvents = "NumEvents";
    public const string NumControls = "NumControls";
    public const string NumStarters = "NumStarters";

    // DoState / UndoState
    public const string ClassTableSet = "SVFTableSet";
    public const string OnDepTable = "JeNaOdjezdové";
    public const string OnArrTable = "JeNaPříjezdové";
    public const string OnPlatformTable = "JeNaSměrových";
    public const string ShowPosition = "JeZobrazenaPozice";
    public const string ShowTrack = "JeZobrazenaKolej";
    public const string OnDepTableOld = "OnDepTable";
    public const string OnArrTableOld = "OnArrTable";
    public const string OnPlatformTableOld = "OnPlatformTable";
    public const string ShowPositionOld = "PlatformNumber";
    public const string ShowTrackOld = "TrackNumber";

    // akcia
    public const string NextState = "NextState";
    public const string ReportKey = "ReportKey";
    public const string Dialog = "Dialog";
    public const string PosForArrival = "Pozice pro příjezd";
    public const string PosForDeparture = "Pozice pro odjezd";
    public const string CopyPosition = "Kopírovat pozici";
    public const string ModifyReport = "ModifiReport";
    public const string AskReport = "Dotaz na hlášení";
    public const string HideShow = "SkrytOdkryt";
    public const string DelayArrival = "Zpoždění na příjezdu";
    public const string DelayDeparture = "Zpoždění na odjezdu";
    public const string ReportAbout = "ReportAbout";
    public const string PosGroupTxt = "PosGroupTxt";
    public const string PosText1 = "PosText1";
    public const string PosText2 = "PosText2";

    // ovladac
    public const string CtrlID = "CtrlID";
    public const string DesignKey = "DesignKey";
    public const string EventKey = "EventKey";

    // starter
    public const string ClassStarter = "SDStarterShape";
    public const string TimePointKey = "TimePointKey";
    public const string TimeOffset = "TimeOffset";
    public const string TimeOffsetStep = "TimeOffsetStep";
    public const string TimePointKeyLast = "TimePointKeyLast";
    public const string TimeOffsetLast = "TimeOffsetLast";
    public const string StartLaterToo = "StartLaterToo";

    /// <summary>Zauzivany kluc prveho stavu kategorie (INISS berie ako pociatocny prvy stav bez ohladu na kluc).</summary>
    public const string StartState = "#Start";

    /// <summary>
    /// Casovy bod stavu, ktory INISS nastavi na cas vstupu vlaku do stavu - stav ho musi deklarovat
    /// skupinou <c>TimePoint</c> s tymto klucom (bez zdrojovych bodov).
    /// </summary>
    public const string StartTime = "#StartTime";

    /// <summary>Predvoleny vyraz <c>IndCat</c>, ked kluc chyba.</summary>
    public const string DefaultIndCat = "INDCAT6";

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
    public const int MaxIcon = 5;
}
