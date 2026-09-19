using ToolsCore.Expressions;

namespace ToolsCore.StateDgm;

/// <summary>
///     Kody kontrol stavoveho diagramu.
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
    LoadWarning
}

/// <summary>
///     Druh prvku, na ktory sa hlasenie viaze.
/// </summary>
public enum StateDgmElementKind
{
    Diagram,
    Design,
    TimePoint,
    Category,
    State,
    Event,
    Control,
    Starter
}

/// <summary>
///     Umiestnenie hlasenia v diagrame (indexy do zoznamov modelu), aby sa dalo v editore preskocit na prvok.
/// </summary>
/// <param name="Kind">Druh prvku.</param>
/// <param name="Category">Index kategorie alebo -1.</param>
/// <param name="State">Index stavu v kategorii alebo -1.</param>
/// <param name="Index">Index prvku v jeho zozname (vzhlad, casovy bod, akcia, ovladac, starter) alebo -1.</param>
public readonly record struct StateDgmLocation(StateDgmElementKind Kind, int Category = -1, int State = -1, int Index = -1)
{
    /// <summary>Cely diagram.</summary>
    public static readonly StateDgmLocation Root = new(StateDgmElementKind.Diagram);
}

/// <summary>
///     Hlasenie kontroly diagramu.
/// </summary>
public sealed record StateDgmDiagnostic(ExprSeverity Severity, StateDgmDiagnosticCode Code, string Message, StateDgmLocation Location)
{
    /// <summary>Kluc/nazov prvku pre zobrazenie (napr. <c>Výchozí vlak › Vypiš › #GoToPřijíždí</c>).</summary>
    public string Path { get; init; } = "";

    /// <summary>Hlasenie z validatora vyrazov (pozicia vo vyraze), ak ide o vyraz.</summary>
    public ExprDiagnostic? Expr { get; init; }

    /// <summary>Kluc, v ktorom je vyraz (IndCat, AutoCondition, AutoMode…).</summary>
    public string? ExprKey { get; init; }

    /// <summary>Je to chyba (INISS diagram nenacita alebo sa spravi zjavne zle).</summary>
    public bool IsError => Severity == ExprSeverity.Error;

    /// <inheritdoc />
    public override string ToString() => $"{Severity} {Code}: {Path}: {Message}";
}

/// <summary>
///     Nastavenie kontroly.
/// </summary>
public sealed class StateDgmValidationOptions
{
    /// <summary>Typy hlaseni z lokalneho Categori.txt; null = ReportKey sa nekontroluje.</summary>
    public IReadOnlyCollection<string>? ReportKeys { get; init; }

    /// <summary>Symboly pre vyrazy (druhy vlakov, stanice…).</summary>
    public IExprSymbolProvider? Symbols { get; init; }

    /// <summary>Stanica ma ILTIS - kluc <c>Wait</c> ma zmysel; false = upozornit.</summary>
    public bool HasIltis { get; init; } = true;

    /// <summary>Hlasit aj informacie (slepe stavy).</summary>
    public bool ReportInfos { get; init; } = true;
}

/// <summary>
///     Kontrola stavoveho diagramu - to, co by INISS pri nacitani odmietol (chyby), a to, co by sa spravalo
///     inak, nez autor zrejme chcel (upozornenia).
/// </summary>
public static class StateDgmValidator
{
    /// <summary>
    ///     Skontroluje diagram.
    /// </summary>
    public static List<StateDgmDiagnostic> Validate(StateDgmDiagram d, StateDgmValidationOptions? options = null)
    {
        options ??= new StateDgmValidationOptions();
        var list = new List<StateDgmDiagnostic>();

        foreach (var w in d.Warnings)
            list.Add(new StateDgmDiagnostic(ExprSeverity.Warning, StateDgmDiagnosticCode.LoadWarning, w.Message, StateDgmLocation.Root));

        CheckDesigns(d, list);
        CheckTimePoints(d, list);
        CheckIndCat(d, options, list);

        if (d.Categories.Count == 0)
            list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.NoCategories, "Diagram nemá žiadnu kategóriu vlakov", StateDgmLocation.Root));

        CheckDuplicates(d.Categories.Select(c => c.Key), "kategórie", list, i => new StateDgmLocation(StateDgmElementKind.Category, i), i => d.Categories[i].Name);

        for (var ci = 0; ci < d.Categories.Count; ci++)
            CheckCategory(d, ci, options, list);

        return list;
    }

    private static void CheckDesigns(StateDgmDiagram d, List<StateDgmDiagnostic> list)
    {
        CheckDuplicates(d.Designs.Select(x => x.Key), "vzhľadu", list, i => new StateDgmLocation(StateDgmElementKind.Design, Index: i), i => d.Designs[i].Key);
        for (var i = 0; i < d.Designs.Count; i++)
        {
            var des = d.Designs[i];
            var loc = new StateDgmLocation(StateDgmElementKind.Design, Index: i);
            if (des.Key.Length == 0)
                list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.EmptyKey, "Vzhľad bez kľúča", loc) { Path = $"Vzhľad {i + 1}" });
            if (!StateDgmBitmaps.TryParse(des.Bitmaps, out _))
                list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.BitmapsFormat, $"Bitmaps „{des.Bitmaps}“ nemá tvar posun-normálny,zameranie,stlačený (napr. 6-7,8,9)", loc) { Path = des.Key });
        }
    }

    private static void CheckTimePoints(StateDgmDiagram d, List<StateDgmDiagnostic> list)
    {
        CheckDuplicates(d.TimePoints.Select(x => x.Key), "časového bodu", list, i => new StateDgmLocation(StateDgmElementKind.TimePoint, Index: i), i => d.TimePoints[i].Key);
        var all = d.AllTimePointKeys.ToHashSet(StringComparer.Ordinal);
        for (var i = 0; i < d.TimePoints.Count; i++)
            CheckTimePoint(d.TimePoints[i], new StateDgmLocation(StateDgmElementKind.TimePoint, Index: i), "", all, list);
    }

    private static void CheckTimePoint(StateDgmTimePoint tp, StateDgmLocation loc, string prefix, HashSet<string> all, List<StateDgmDiagnostic> list)
    {
        var path = prefix + (tp.Key.Length == 0 ? $"časový bod {loc.Index + 1}" : tp.Key);
        {
            if (tp.Key.Length == 0)
                list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.EmptyKey, "Časový bod bez kľúča – INISS diagram nenačíta", loc) { Path = path });
            else if (StateDgmKeys.BuiltInTimePoints.Contains(tp.Key))
                list.Add(new StateDgmDiagnostic(ExprSeverity.Warning, StateDgmDiagnosticCode.TimePointShadowsBuiltIn, $"Časový bod „{tp.Key}“ má kľúč zabudovaného bodu – vlastný bod je zbytočný", loc) { Path = path });
            if (tp.Operator is not (StateDgmKeys.OPERATOR_MIN or StateDgmKeys.OPERATOR_MAX))
                list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.TimePointOperator, $"Operator „{tp.Operator}“ – povolené je len min alebo max", loc) { Path = path });
            foreach (var k in new[] { tp.TimePointKey1, tp.TimePointKey2 })
                if (k.Length > 0 && !all.Contains(k))
                    list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.TimePointKeyMissing, $"Zdrojový časový bod „{k}“ neexistuje", loc) { Path = path });
                else if (k == tp.Key && k.Length > 0)
                    list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.TimePointKeyMissing, "Časový bod sa odvodzuje sám od seba", loc) { Path = path });
        }
    }

    private static void CheckIndCat(StateDgmDiagram d, StateDgmValidationOptions options, List<StateDgmDiagnostic> list)
    {
        var expr = d.EffectiveIndCat;
        CheckExpression(expr, StateDgmKeys.IND_CAT, ExprContext.Condition, false, options, StateDgmLocation.Root, "IndCat", list);
        var t = expr.Trim();
        var expected = t.Equals("INDCAT6", StringComparison.OrdinalIgnoreCase) ? 6 : t.Equals("INDCAT8", StringComparison.OrdinalIgnoreCase) ? 8 : 0;
        if (expected != 0 && d.Categories.Count != expected)
            list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.IndCatCategoryCount,
                $"{t.ToUpperInvariant()} vracia čísla 1–{expected}, ale diagram má {d.Categories.Count} kategórií" +
                (d.Categories.Count < expected ? " – vlaky s vyšším číslom skončia v poslednej kategórii a v logu" : " – ďalšie kategórie sa nikdy nepoužijú"),
                StateDgmLocation.Root) { Path = "IndCat", ExprKey = StateDgmKeys.IND_CAT });
    }

    private static void CheckCategory(StateDgmDiagram d, int ci, StateDgmValidationOptions options, List<StateDgmDiagnostic> list)
    {
        var cat = d.Categories[ci];
        var cloc = new StateDgmLocation(StateDgmElementKind.Category, ci);
        var cpath = cat.Name.Length > 0 ? cat.Name : cat.Key;
        if (cat.Key.Length == 0)
            list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.EmptyKey, "Kategória bez kľúča", cloc) { Path = $"Kategória {ci + 1}" });
        if (cat.Icon is < 0 or > 2)
            list.Add(new StateDgmDiagnostic(cat.Icon is < 0 or > StateDgmKeys.MAX_ICON ? ExprSeverity.Error : ExprSeverity.Warning, StateDgmDiagnosticCode.IconRange,
                $"Ikona kategórie {cat.Icon} – do súboru patrí čierna podoba 0–2, červenú (+3) si INISS pridá pri rýchlikoch sám", cloc) { Path = cpath });
        if (cat.States.Count == 0)
        {
            list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.NoStates, "Kategória nemá žiadny stav", cloc) { Path = cpath });
            return;
        }

        CheckDuplicates(cat.States.Select(s => s.Key), "stavu", list, i => new StateDgmLocation(StateDgmElementKind.State, ci, i), i => $"{cpath} › {cat.States[i]}");

        var stateKeys = cat.States.Select(s => s.Key).ToHashSet(StringComparer.Ordinal);
        for (var si = 0; si < cat.States.Count; si++)
            CheckState(d, cat, ci, si, stateKeys, options, list);

        CheckReachability(cat, ci, cpath, options, list);
    }

    private static void CheckState(StateDgmDiagram d, StateDgmCategory cat, int ci, int si, HashSet<string> stateKeys, StateDgmValidationOptions options, List<StateDgmDiagnostic> list)
    {
        var s = cat.States[si];
        var loc = new StateDgmLocation(StateDgmElementKind.State, ci, si);
        var cpath = cat.Name.Length > 0 ? cat.Name : cat.Key;
        var spath = $"{cpath} › {(s.Name.Length > 0 ? s.Name : s.Key)}";

        if (s.Key.Length == 0)
            list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.EmptyKey, "Stav bez kľúča", loc) { Path = $"{cpath} › stav {si + 1}" });
        if (s.Icon is < 0 or > StateDgmKeys.MAX_ICON)
            list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.IconRange, $"Ikona stavu {s.Icon} – INISS má ikony 0–{StateDgmKeys.MAX_ICON}", loc) { Path = spath });
        if (s.DefaultControl != 0 && (s.DefaultControl < 1 || s.DefaultControl > s.Controls.Count))
            list.Add(new StateDgmDiagnostic(ExprSeverity.Warning, StateDgmDiagnosticCode.DefaultControlRange, $"DefaultControl={s.DefaultControl}, ale stav má {s.Controls.Count} ovládačov (číslované od 1)", loc) { Path = spath });

        // automatika
        if (s.AutoMode is { IsExpression: false } am && am.Number is < 0 or > 2)
            list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.AutoModeRange, $"AutoMode={am.Number} – povolené 0 (ručne), 1 (poloautomat), 2 (automat); INISS by načítanie diagramu prerušil", loc) { Path = spath, ExprKey = StateDgmKeys.AUTO_MODE });
        if (s.AutoTimePoint is { IsExpression: false } atp && atp.Number is < 1 or > 2)
            list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.AutoTimePointRange, $"AutoTimePoint={atp.Number} – povolené 1 (príchod), 2 (odchod); INISS by načítanie diagramu prerušil", loc) { Path = spath, ExprKey = StateDgmKeys.AUTO_TIME_POINT });
        if (s.AutoModif is { IsExpression: false } amf && amf.Number is < 1 or > 2)
            list.Add(new StateDgmDiagnostic(ExprSeverity.Warning, StateDgmDiagnosticCode.AutoModifRange, $"AutoModif={amf.Number} – povolené 1 (krátke) alebo 2 (dlhé hlásenie)", loc) { Path = spath, ExprKey = StateDgmKeys.AUTO_MODIF });
        if (s.HasAutomation && s.AutoTimePoint == null)
            list.Add(new StateDgmDiagnostic(ExprSeverity.Warning, StateDgmDiagnosticCode.AutomationIncomplete, "Automatika je zapnutá, ale chýba AutoTimePoint – INISS nemá od čoho počítať čas", loc) { Path = spath, ExprKey = StateDgmKeys.AUTO_TIME_POINT });
        // automatika hovori, kedy vlak do TOHTO stavu vstupi sam (riadok v Kalendari akcii vlaku) - akcie stavu s tym nesuvisia
        if (!s.HasAutomation && s.AutoMode == null && (s.AutoTimePoint != null || s.AutoTimePointAdd != null))
            list.Add(new StateDgmDiagnostic(ExprSeverity.Info, StateDgmDiagnosticCode.AutomationIncomplete, "Časový bod automatiky bez AutoMode – do stavu sa vlak dostane len ručne", loc) { Path = spath, ExprKey = StateDgmKeys.AUTO_MODE });
        if (s.Wait != null && !options.HasIltis)
            list.Add(new StateDgmDiagnostic(ExprSeverity.Warning, StateDgmDiagnosticCode.WaitWithoutIltis, "Wait čaká na udalosť ILTISu, ktorú stanica bez ILTISu nikdy nedostane – akcia sa nevykoná", loc) { Path = spath, ExprKey = StateDgmKeys.WAIT });

        CheckDynamic(s.AutoMode, StateDgmKeys.AUTO_MODE, options, loc, spath, list);
        CheckDynamic(s.AutoTimePoint, StateDgmKeys.AUTO_TIME_POINT, options, loc, spath, list);
        CheckDynamic(s.AutoTimePointAdd, StateDgmKeys.AUTO_TIME_POINT_ADD, options, loc, spath, list);
        CheckDynamic(s.AutoModif, StateDgmKeys.AUTO_MODIF, options, loc, spath, list);
        if (s.Wait != null) CheckExpression(s.Wait.Text, StateDgmKeys.WAIT, ExprContext.StateDgmWait, false, options, loc, spath, list);
        if (s.WaitPath != null) CheckDynamic(s.WaitPath, StateDgmKeys.WAIT_PATH, options, loc, spath, list);
        if (s.AutoCondition != null) CheckExpression(s.AutoCondition, StateDgmKeys.AUTO_CONDITION, ExprContext.Condition, true, options, loc, spath, list);

        // akcie
        CheckDuplicates(s.Events.Select(e => e.Key), "akcie", list, i => new StateDgmLocation(StateDgmElementKind.Event, ci, si, i), i => $"{spath} › {s.Events[i].Key}");
        var eventKeys = s.Events.Select(e => e.Key).ToHashSet(StringComparer.Ordinal);
        for (var ei = 0; ei < s.Events.Count; ei++)
            CheckEvent(s.Events[ei], new StateDgmLocation(StateDgmElementKind.Event, ci, si, ei), $"{spath} › {s.Events[ei].Key}", stateKeys, options, list);

        // ovladace
        var ids = new HashSet<int>();
        for (var i = 0; i < s.Controls.Count; i++)
        {
            var c = s.Controls[i];
            var cl = new StateDgmLocation(StateDgmElementKind.Control, ci, si, i);
            var cp = $"{spath} › tlačidlo {c.CtrlId}";
            if (!ids.Add(c.CtrlId))
                list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.DuplicateCtrlId, $"Dva ovládače s CtrlID={c.CtrlId}", cl) { Path = cp });
            if (c.DesignKey.Length == 0 || d.FindDesign(c.DesignKey) == null)
                list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.DesignKeyMissing, $"Vzhľad „{c.DesignKey}“ nie je v bloku CtrlDesign", cl) { Path = cp });
            if (c.EventKey.Length > 0 && !eventKeys.Contains(c.EventKey))
                list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.EventKeyMissing, $"Akcia „{c.EventKey}“ v tomto stave nie je", cl) { Path = cp });
        }

        // casove body stavu
        var timePoints = d.AllTimePointKeys.Concat(s.TimePoints.Select(t => t.Key)).ToHashSet(StringComparer.Ordinal);
        CheckDuplicates(s.TimePoints.Select(x => x.Key), "časového bodu", list, i => new StateDgmLocation(StateDgmElementKind.TimePoint, ci, si, i), i => $"{spath} › {s.TimePoints[i].Key}");
        for (var i = 0; i < s.TimePoints.Count; i++)
            CheckTimePoint(s.TimePoints[i], new StateDgmLocation(StateDgmElementKind.TimePoint, ci, si, i), spath + " › ", timePoints, list);

        // startery
        CheckDuplicates(s.Starters.Select(x => x.Key), "štartéra", list, i => new StateDgmLocation(StateDgmElementKind.Starter, ci, si, i), i => $"{spath} › {s.Starters[i].Key}");
        for (var i = 0; i < s.Starters.Count; i++)
        {
            var st = s.Starters[i];
            var sl = new StateDgmLocation(StateDgmElementKind.Starter, ci, si, i);
            var sp = $"{spath} › {(st.Key.Length > 0 ? st.Key : $"štartér {i + 1}")}";
            if (st.Key.Length == 0)
                list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.EmptyKey, "Štartér bez kľúča", sl) { Path = sp });
            if (st.EventKey.Length == 0 || !eventKeys.Contains(st.EventKey))
                list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.EventKeyMissing, $"Akcia „{st.EventKey}“ v tomto stave nie je", sl) { Path = sp });
            if (st.TimePointKey.Length == 0 || !timePoints.Contains(st.TimePointKey))
                list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.TimePointKeyMissing, $"Časový bod „{st.TimePointKey}“ neexistuje", sl) { Path = sp });
            if (st.TimePointKeyLast != null && !timePoints.Contains(st.TimePointKeyLast))
                list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.TimePointKeyMissing, $"Časový bod „{st.TimePointKeyLast}“ neexistuje", sl) { Path = sp });
            if (st.Class != StateDgmKeys.CLASS_STARTER)
                list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.UnknownEventClass, $"Trieda štartéra „{st.Class}“ – INISS pozná len {StateDgmKeys.CLASS_STARTER}", sl) { Path = sp });
        }
    }

    private static void CheckEvent(StateDgmEvent e, StateDgmLocation loc, string path, HashSet<string> stateKeys, StateDgmValidationOptions options, List<StateDgmDiagnostic> list)
    {
        if (e.Key.Length == 0)
            list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.EmptyKey, "Akcia bez kľúča", loc) { Path = path });
        if (!StateDgmKeys.EventClasses.Contains(e.Class))
            list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.UnknownEventClass, $"Neznáma trieda akcie „{e.Class}“", loc) { Path = path });
        if (!string.IsNullOrEmpty(e.NextState) && !stateKeys.Contains(e.NextState))
            list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.NextStateMissing, $"NextState „{e.NextState}“ v tejto kategórii nie je", loc) { Path = path });
        if (options.ReportKeys != null && !string.IsNullOrEmpty(e.ReportKey) && !options.ReportKeys.Contains(e.ReportKey))
            list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.ReportKeyUnknown, $"Typ hlásenia „{e.ReportKey}“ nie je v Categori.txt", loc) { Path = path });

        switch (e.Class)
        {
            case "SDEventChangeState":
            case "SDEventSelectState":
                if (string.IsNullOrEmpty(e.NextState))
                    list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.ChangeStateNoNext, $"{e.Class} vyžaduje NextState", loc) { Path = path });
                break;
            case "SDEventWithDialog":
                if (string.IsNullOrEmpty(e.Dialog))
                    list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.DialogInvalid, "SDEventWithDialog bez dialógu", loc) { Path = path });
                else if (!StateDgmKeys.Dialogs.Contains(e.Dialog))
                    list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.DialogInvalid, $"Neznámy dialóg „{e.Dialog}“ – INISS má {string.Join(", ", StateDgmKeys.Dialogs)}", loc) { Path = path });
                break;
            case "SDEventVlakAttr":
                if (e.DelayArrival is not > 0 && e.DelayDeparture is not > 0)
                    list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.VlakAttrNoDelay, $"SDEventVlakAttr musí mať zapnuté „{StateDgmKeys.DELAY_ARRIVAL}“ alebo „{StateDgmKeys.DELAY_DEPARTURE}“", loc) { Path = path });
                break;
        }
    }

    private static void CheckReachability(StateDgmCategory cat, int ci, string cpath, StateDgmValidationOptions options, List<StateDgmDiagnostic> list)
    {
        // pociatocny je prvy stav kategorie (kluc #Start je len zvyklost)
        var index = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var i = 0; i < cat.States.Count; i++) index.TryAdd(cat.States[i].Key, i);
        var seen = new HashSet<int> { 0 };
        var queue = new Queue<int>();
        queue.Enqueue(0);
        while (queue.Count > 0)
        {
            var s = cat.States[queue.Dequeue()];
            foreach (var e in s.Events)
                if (e.ChangesState && index.TryGetValue(e.NextState!, out var n) && seen.Add(n))
                    queue.Enqueue(n);
        }

        for (var i = 0; i < cat.States.Count; i++)
        {
            var s = cat.States[i];
            var loc = new StateDgmLocation(StateDgmElementKind.State, ci, i);
            var path = $"{cpath} › {(s.Name.Length > 0 ? s.Name : s.Key)}";
            if (!seen.Contains(i))
                list.Add(new StateDgmDiagnostic(ExprSeverity.Warning, StateDgmDiagnosticCode.StateUnreachable, "Stav sa z počiatočného stavu kategórie žiadnou akciou nedosiahne", loc) { Path = path });
            else if (options.ReportInfos && !s.Events.Any(e => e.ChangesState) && !s.Attr.HasFlag(StateDgmAttr.Shadow))
                list.Add(new StateDgmDiagnostic(ExprSeverity.Info, StateDgmDiagnosticCode.StateDeadEnd, "Zo stavu nevedie žiadna akcia a nie je koncový (Shadow) – vlak v ňom ostane", loc) { Path = path });
        }
    }

    private static void CheckDynamic(StateDgmDynamic? v, string key, StateDgmValidationOptions options, StateDgmLocation loc, string path, List<StateDgmDiagnostic> list)
    {
        if (v is { IsExpression: true })
            CheckExpression(v.Expression!, key, ExprContext.Condition, false, options, loc, path, list);
    }

    private static void CheckExpression(string text, string key, ExprContext context, bool isCondition, StateDgmValidationOptions options, StateDgmLocation loc, string path, List<StateDgmDiagnostic> list)
    {
        var r = ExprValidator.Validate(text, new ExprValidationOptions { Context = context, Symbols = options.Symbols, IsCondition = isCondition, ReportContextDependent = false });
        foreach (var x in r.Diagnostics)
        {
            var sev = x.Severity;
            var msg = x.IsError ? $"{key}: výraz sa nepreloží – {x.Message}; INISS by načítanie diagramu prerušil" : $"{key}: {x.Message}";
            list.Add(new StateDgmDiagnostic(sev, StateDgmDiagnosticCode.Expression, msg, loc) { Path = path, Expr = x, ExprKey = key });
        }
    }

    private static void CheckDuplicates(IEnumerable<string> keys, string what, List<StateDgmDiagnostic> list, Func<int, StateDgmLocation> loc, Func<int, string> path)
    {
        var seen = new Dictionary<string, int>(StringComparer.Ordinal);
        var i = 0;
        foreach (var k in keys)
        {
            if (k.Length > 0 && seen.TryGetValue(k, out var first))
                list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.DuplicateKey, $"Kľúč {what} „{k}“ je použitý dvakrát (prvýkrát ako {first + 1}.)", loc(i)) { Path = path(i) });
            else if (k.Length > 0)
                seen[k] = i;
            i++;
        }
    }
}
