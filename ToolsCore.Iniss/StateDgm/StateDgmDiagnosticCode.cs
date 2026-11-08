namespace ToolsCore.Iniss.StateDgm;

/// <summary>
/// Kody kontrol stavoveho diagramu.
/// </summary>
public enum StateDgmDiagnosticCode
{
    /// <summary>Diagram nema kategorie - INISS nema podla coho zaradit vlaky.</summary>
    NoCategories,

    /// <summary>Kategoria nema stavy.</summary>
    NoStates,

    /// <summary>Prazdny kluc (kategoria, stav, akcia, vzhlad, casovy bod, starter).</summary>
    EmptyKey,

    /// <summary>Dva prvky s rovnakym klucom.</summary>
    DuplicateKey,

    /// <summary><c>NextState</c> odkazuje na stav, ktory v tej istej kategorii nie je.</summary>
    NextStateMissing,

    /// <summary><c>EventKey</c> ovladaca alebo startera odkazuje na akciu, ktora v stave nie je.</summary>
    EventKeyMissing,

    /// <summary><c>ReportKey</c> nie je v lokalnom Categori.txt.</summary>
    ReportKeyUnknown,

    /// <summary><c>DesignKey</c> nie je v bloku CtrlDesign.</summary>
    DesignKeyMissing,

    /// <summary>Odkaz na casovy bod, ktory nie je zabudovany ani vlastny.</summary>
    TimePointKeyMissing,

    /// <summary><c>Operator</c> casoveho bodu nie je <c>min</c>/<c>max</c>.</summary>
    TimePointOperator,

    /// <summary><c>SDEventVlakAttr</c> bez zapnuteho meskania na prichode/odchode.</summary>
    VlakAttrNoDelay,

    /// <summary><c>SDEventChangeState</c>/<c>SDEventSelectState</c> bez <c>NextState</c>.</summary>
    ChangeStateNoNext,

    /// <summary><c>SDEventWithDialog</c> bez dialogu alebo s neznamym dialogom.</summary>
    DialogInvalid,

    /// <summary>Neznama trieda akcie.</summary>
    UnknownEventClass,

    /// <summary><c>DefaultControl</c> mimo poctu ovladacov.</summary>
    DefaultControlRange,

    /// <summary>Ikona mimo rozsahu 0-5 (kategoria 0-2).</summary>
    IconRange,

    /// <summary>Ciselny <c>AutoMode</c> mimo 0-2 - INISS nacitanie diagramu prerusi.</summary>
    AutoModeRange,

    /// <summary>Ciselny <c>AutoTimePoint</c> mimo 1-2 - INISS nacitanie diagramu prerusi.</summary>
    AutoTimePointRange,

    /// <summary><c>AutoModif</c> mimo 1-2.</summary>
    AutoModifRange,

    /// <summary>Automatika bez casoveho bodu alebo casovy bod bez rezimu.</summary>
    AutomationIncomplete,

    /// <summary>Vyraz sa nepreklada alebo validator jazyka vyrazov nieco hlasi.</summary>
    Expression,

    /// <summary><c>INDCAT6</c>/<c>INDCAT8</c> s inym poctom kategorii.</summary>
    IndCatCategoryCount,

    /// <summary>Dva ovladace s rovnakym <c>CtrlID</c>.</summary>
    DuplicateCtrlId,

    /// <summary><c>Bitmaps</c> nema tvar <c>posun-a,b,c</c>.</summary>
    BitmapsFormat,

    /// <summary>Stav sa z <c>#Start</c> ziadnou akciou nedosiahne.</summary>
    StateUnreachable,

    /// <summary>Stav nema akciu, ktora by ho opustila, a nie je koncovy (Shadow).</summary>
    StateDeadEnd,

    /// <summary>Vlastny casovy bod ma kluc zabudovaneho.</summary>
    TimePointShadowsBuiltIn,

    /// <summary><c>Wait</c> caka na udalost ILTISu, ale stanica ILTIS nema.</summary>
    WaitWithoutIltis,

    /// <summary>Upozornenie z nacitania (napr. nesediaci <c>Num…</c>).</summary>
    LoadWarning,

    /// <summary>Text suboru sa neda rozlozit (<see cref="StateDgmParseException" />) - hlasi editor textu, nie validator.</summary>
    Syntax
}
