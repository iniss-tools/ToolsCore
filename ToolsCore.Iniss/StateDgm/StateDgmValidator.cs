using System.Globalization;
using ToolsCore.Iniss.Expressions;
using ToolsCore.Iniss.Properties;

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

/// <summary>
/// Druh prvku, na ktory sa hlasenie viaze.
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
/// Umiestnenie hlasenia v diagrame (indexy do zoznamov modelu), aby sa dalo v editore preskocit na prvok.
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
/// Hlasenie kontroly diagramu.
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
/// Nastavenie kontroly.
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
/// Kontrola stavoveho diagramu - to, co by INISS pri nacitani odmietol (chyby), a to, co by sa spravalo
/// inak, nez autor zrejme chcel (upozornenia).
/// </summary>
public static class StateDgmValidator
{
    /// <summary>
    /// Skontroluje diagram.
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
            list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.NoCategories, Resources.Sdv_NoCategories, StateDgmLocation.Root));

        CheckDuplicates(d.Categories.Select(c => c.Key), Resources.Sdv_WhatCategory, list, i => new StateDgmLocation(StateDgmElementKind.Category, i), i => d.Categories[i].Name);

        for (var ci = 0; ci < d.Categories.Count; ci++)
            CheckCategory(d, ci, options, list);

        return list;
    }

    private static void CheckDesigns(StateDgmDiagram d, List<StateDgmDiagnostic> list)
    {
        CheckDuplicates(d.Designs.Select(x => x.Key), Resources.Sdv_WhatDesign, list, i => new StateDgmLocation(StateDgmElementKind.Design, Index: i), i => d.Designs[i].Key);
        for (var i = 0; i < d.Designs.Count; i++)
        {
            var des = d.Designs[i];
            var loc = new StateDgmLocation(StateDgmElementKind.Design, Index: i);
            if (des.Key.Length == 0)
                list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.EmptyKey, Resources.Sdv_DesignNoKey, loc) { Path = string.Format(CultureInfo.CurrentCulture, Resources.Sdv_DesignN, i + 1) });
            if (!StateDgmBitmaps.TryParse(des.Bitmaps, out _))
                list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.BitmapsFormat, string.Format(CultureInfo.CurrentCulture, Resources.Sdv_BitmapsFormat, des.Bitmaps), loc) { Path = des.Key });
        }
    }

    private static void CheckTimePoints(StateDgmDiagram d, List<StateDgmDiagnostic> list)
    {
        CheckDuplicates(d.TimePoints.Select(x => x.Key), Resources.Sdv_WhatTimePoint, list, i => new StateDgmLocation(StateDgmElementKind.TimePoint, Index: i), i => d.TimePoints[i].Key);
        var all = d.AllTimePointKeys.ToHashSet(StringComparer.Ordinal);
        for (var i = 0; i < d.TimePoints.Count; i++)
            CheckTimePoint(d.TimePoints[i], new StateDgmLocation(StateDgmElementKind.TimePoint, Index: i), "", all, list);
    }

    private static void CheckTimePoint(StateDgmTimePoint tp, StateDgmLocation loc, string prefix, HashSet<string> all, List<StateDgmDiagnostic> list)
    {
        var path = prefix + (tp.Key.Length == 0 ? string.Format(CultureInfo.CurrentCulture, Resources.Sdv_TimePointN, loc.Index + 1) : tp.Key);
        {
            if (tp.Key.Length == 0)
                list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.EmptyKey, Resources.Sdv_TimePointNoKey, loc) { Path = path });
            else if (StateDgmKeys.BuiltInTimePoints.Contains(tp.Key))
                list.Add(new StateDgmDiagnostic(ExprSeverity.Warning, StateDgmDiagnosticCode.TimePointShadowsBuiltIn, string.Format(CultureInfo.CurrentCulture, Resources.Sdv_TimePointShadows, tp.Key), loc) { Path = path });
            if (tp.Operator is not (StateDgmKeys.OperatorMin or StateDgmKeys.OperatorMax))
                list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.TimePointOperator, string.Format(CultureInfo.CurrentCulture, Resources.Sdv_TimePointOperator, tp.Operator), loc) { Path = path });
            foreach (var k in new[] { tp.TimePointKey1, tp.TimePointKey2 })
                if (k.Length > 0 && !all.Contains(k))
                    list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.TimePointKeyMissing, string.Format(CultureInfo.CurrentCulture, Resources.Sdv_TimePointSourceMissing, k), loc) { Path = path });
                else if (k == tp.Key && k.Length > 0)
                    list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.TimePointKeyMissing, Resources.Sdv_TimePointSelf, loc) { Path = path });
        }
    }

    private static void CheckIndCat(StateDgmDiagram d, StateDgmValidationOptions options, List<StateDgmDiagnostic> list)
    {
        var expr = d.EffectiveIndCat;
        CheckExpression(expr, StateDgmKeys.IndCat, ExprContext.Condition, false, options, StateDgmLocation.Root, "IndCat", list);
        var t = expr.Trim();
        var expected = t.Equals("INDCAT6", StringComparison.OrdinalIgnoreCase) ? 6 : t.Equals("INDCAT8", StringComparison.OrdinalIgnoreCase) ? 8 : 0;
        if (expected != 0 && d.Categories.Count != expected)
            list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.IndCatCategoryCount,
                string.Format(CultureInfo.CurrentCulture, Resources.Sdv_CategoryCount, t.ToUpperInvariant(), expected, d.Categories.Count) +
                (d.Categories.Count < expected ? Resources.Sdv_CategoryCountFewer : Resources.Sdv_CategoryCountMore),
                StateDgmLocation.Root) { Path = "IndCat", ExprKey = StateDgmKeys.IndCat });
    }

    private static void CheckCategory(StateDgmDiagram d, int ci, StateDgmValidationOptions options, List<StateDgmDiagnostic> list)
    {
        var cat = d.Categories[ci];
        var cloc = new StateDgmLocation(StateDgmElementKind.Category, ci);
        var cpath = cat.Name.Length > 0 ? cat.Name : cat.Key;
        if (cat.Key.Length == 0)
            list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.EmptyKey, Resources.Sdv_CategoryNoKey, cloc) { Path = string.Format(CultureInfo.CurrentCulture, Resources.Sdv_CategoryN, ci + 1) });
        if (cat.Icon is < 0 or > 2)
            list.Add(new StateDgmDiagnostic(cat.Icon is < 0 or > StateDgmKeys.MaxIcon ? ExprSeverity.Error : ExprSeverity.Warning, StateDgmDiagnosticCode.IconRange,
                string.Format(CultureInfo.CurrentCulture, Resources.Sdv_CategoryIcon, cat.Icon), cloc) { Path = cpath });
        if (cat.States.Count == 0)
        {
            list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.NoStates, Resources.Sdv_CategoryNoStates, cloc) { Path = cpath });
            return;
        }

        CheckDuplicates(cat.States.Select(s => s.Key), Resources.Sdv_WhatState, list, i => new StateDgmLocation(StateDgmElementKind.State, ci, i), i => $"{cpath} › {cat.States[i]}");

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
            list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.EmptyKey, Resources.Sdv_StateNoKey, loc) { Path = string.Format(CultureInfo.CurrentCulture, Resources.Sdv_PathStateN, cpath, si + 1) });
        if (s.Icon is < 0 or > StateDgmKeys.MaxIcon)
            list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.IconRange, string.Format(CultureInfo.CurrentCulture, Resources.Sdv_StateIcon, s.Icon, StateDgmKeys.MaxIcon), loc) { Path = spath });
        if (s.DefaultControl != 0 && (s.DefaultControl < 1 || s.DefaultControl > s.Controls.Count))
            list.Add(new StateDgmDiagnostic(ExprSeverity.Warning, StateDgmDiagnosticCode.DefaultControlRange, string.Format(CultureInfo.CurrentCulture, Resources.Sdv_DefaultControl, s.DefaultControl, s.Controls.Count), loc) { Path = spath });

        // automatika
        if (s.AutoMode is { IsExpression: false } am && am.Number is < 0 or > 2)
            list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.AutoModeRange, string.Format(CultureInfo.CurrentCulture, Resources.Sdv_AutoMode, am.Number), loc) { Path = spath, ExprKey = StateDgmKeys.AutoMode });
        if (s.AutoTimePoint is { IsExpression: false } atp && atp.Number is < 1 or > 2)
            list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.AutoTimePointRange, string.Format(CultureInfo.CurrentCulture, Resources.Sdv_AutoTimePoint, atp.Number), loc) { Path = spath, ExprKey = StateDgmKeys.AutoTimePoint });
        if (s.AutoModif is { IsExpression: false } amf && amf.Number is < 1 or > 2)
            list.Add(new StateDgmDiagnostic(ExprSeverity.Warning, StateDgmDiagnosticCode.AutoModifRange, string.Format(CultureInfo.CurrentCulture, Resources.Sdv_AutoModif, amf.Number), loc) { Path = spath, ExprKey = StateDgmKeys.AutoModif });
        if (s.HasAutomation && s.AutoTimePoint == null)
            list.Add(new StateDgmDiagnostic(ExprSeverity.Warning, StateDgmDiagnosticCode.AutomationIncomplete, Resources.Sdv_AutoNoTimePoint, loc) { Path = spath, ExprKey = StateDgmKeys.AutoTimePoint });
        // automatika hovori, kedy vlak do TOHTO stavu vstupi sam (riadok v Kalendari akcii vlaku) - akcie stavu s tym nesuvisia
        if (!s.HasAutomation && s.AutoMode == null && (s.AutoTimePoint != null || s.AutoTimePointAdd != null))
            list.Add(new StateDgmDiagnostic(ExprSeverity.Info, StateDgmDiagnosticCode.AutomationIncomplete, Resources.Sdv_AutoNoMode, loc) { Path = spath, ExprKey = StateDgmKeys.AutoMode });
        if (s.Wait != null && !options.HasIltis)
            list.Add(new StateDgmDiagnostic(ExprSeverity.Warning, StateDgmDiagnosticCode.WaitWithoutIltis, Resources.Sdv_WaitWithoutIltis, loc) { Path = spath, ExprKey = StateDgmKeys.Wait });

        CheckDynamic(s.AutoMode, StateDgmKeys.AutoMode, options, loc, spath, list);
        CheckDynamic(s.AutoTimePoint, StateDgmKeys.AutoTimePoint, options, loc, spath, list);
        CheckDynamic(s.AutoTimePointAdd, StateDgmKeys.AutoTimePointAdd, options, loc, spath, list);
        CheckDynamic(s.AutoModif, StateDgmKeys.AutoModif, options, loc, spath, list);
        if (s.Wait != null) CheckExpression(s.Wait.Text, StateDgmKeys.Wait, ExprContext.StateDgmWait, false, options, loc, spath, list);
        if (s.WaitPath != null) CheckDynamic(s.WaitPath, StateDgmKeys.WaitPath, options, loc, spath, list);
        if (s.AutoCondition != null) CheckExpression(s.AutoCondition, StateDgmKeys.AutoCondition, ExprContext.Condition, true, options, loc, spath, list);

        // akcie
        CheckDuplicates(s.Events.Select(e => e.Key), Resources.Sdv_WhatEvent, list, i => new StateDgmLocation(StateDgmElementKind.Event, ci, si, i), i => $"{spath} › {s.Events[i].Key}");
        var eventKeys = s.Events.Select(e => e.Key).ToHashSet(StringComparer.Ordinal);
        for (var ei = 0; ei < s.Events.Count; ei++)
            CheckEvent(s.Events[ei], new StateDgmLocation(StateDgmElementKind.Event, ci, si, ei), $"{spath} › {s.Events[ei].Key}", stateKeys, options, list);

        // ovladace
        var ids = new HashSet<int>();
        for (var i = 0; i < s.Controls.Count; i++)
        {
            var c = s.Controls[i];
            var cl = new StateDgmLocation(StateDgmElementKind.Control, ci, si, i);
            var cp = string.Format(CultureInfo.CurrentCulture, Resources.Sdv_PathButton, spath, c.CtrlId);
            if (!ids.Add(c.CtrlId))
                list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.DuplicateCtrlId, string.Format(CultureInfo.CurrentCulture, Resources.Sdv_DuplicateCtrlId, c.CtrlId), cl) { Path = cp });
            if (c.DesignKey.Length == 0 || d.FindDesign(c.DesignKey) == null)
                list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.DesignKeyMissing, string.Format(CultureInfo.CurrentCulture, Resources.Sdv_DesignMissing, c.DesignKey), cl) { Path = cp });
            if (c.EventKey.Length > 0 && !eventKeys.Contains(c.EventKey))
                list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.EventKeyMissing, string.Format(CultureInfo.CurrentCulture, Resources.Sdv_EventMissing, c.EventKey), cl) { Path = cp });
        }

        // casove body stavu
        var timePoints = d.AllTimePointKeys.Concat(s.TimePoints.Select(t => t.Key)).ToHashSet(StringComparer.Ordinal);
        CheckDuplicates(s.TimePoints.Select(x => x.Key), Resources.Sdv_WhatTimePoint, list, i => new StateDgmLocation(StateDgmElementKind.TimePoint, ci, si, i), i => $"{spath} › {s.TimePoints[i].Key}");
        for (var i = 0; i < s.TimePoints.Count; i++)
            CheckTimePoint(s.TimePoints[i], new StateDgmLocation(StateDgmElementKind.TimePoint, ci, si, i), spath + " › ", timePoints, list);

        // startery
        CheckDuplicates(s.Starters.Select(x => x.Key), Resources.Sdv_WhatStarter, list, i => new StateDgmLocation(StateDgmElementKind.Starter, ci, si, i), i => $"{spath} › {s.Starters[i].Key}");
        for (var i = 0; i < s.Starters.Count; i++)
        {
            var st = s.Starters[i];
            var sl = new StateDgmLocation(StateDgmElementKind.Starter, ci, si, i);
            var sp = $"{spath} › {(st.Key.Length > 0 ? st.Key : string.Format(CultureInfo.CurrentCulture, Resources.Sdv_StarterN, i + 1))}";
            if (st.Key.Length == 0)
                list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.EmptyKey, Resources.Sdv_StarterNoKey, sl) { Path = sp });
            if (st.EventKey.Length == 0 || !eventKeys.Contains(st.EventKey))
                list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.EventKeyMissing, string.Format(CultureInfo.CurrentCulture, Resources.Sdv_EventMissing, st.EventKey), sl) { Path = sp });
            if (st.TimePointKey.Length == 0 || !timePoints.Contains(st.TimePointKey))
                list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.TimePointKeyMissing, string.Format(CultureInfo.CurrentCulture, Resources.Sdv_TimePointMissing, st.TimePointKey), sl) { Path = sp });
            if (st.TimePointKeyLast != null && !timePoints.Contains(st.TimePointKeyLast))
                list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.TimePointKeyMissing, string.Format(CultureInfo.CurrentCulture, Resources.Sdv_TimePointMissing, st.TimePointKeyLast), sl) { Path = sp });
            if (st.Class != StateDgmKeys.ClassStarter)
                list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.UnknownEventClass, string.Format(CultureInfo.CurrentCulture, Resources.Sdv_StarterClass, st.Class, StateDgmKeys.ClassStarter), sl) { Path = sp });
        }
    }

    private static void CheckEvent(StateDgmEvent e, StateDgmLocation loc, string path, HashSet<string> stateKeys, StateDgmValidationOptions options, List<StateDgmDiagnostic> list)
    {
        if (e.Key.Length == 0)
            list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.EmptyKey, Resources.Sdv_EventNoKey, loc) { Path = path });
        if (!StateDgmKeys.EventClasses.Contains(e.Class))
            list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.UnknownEventClass, string.Format(CultureInfo.CurrentCulture, Resources.Sdv_EventClassUnknown, e.Class), loc) { Path = path });
        if (!string.IsNullOrEmpty(e.NextState) && !stateKeys.Contains(e.NextState))
            list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.NextStateMissing, string.Format(CultureInfo.CurrentCulture, Resources.Sdv_NextStateMissing, e.NextState), loc) { Path = path });
        if (options.ReportKeys != null && !string.IsNullOrEmpty(e.ReportKey) && !options.ReportKeys.Contains(e.ReportKey))
            list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.ReportKeyUnknown, string.Format(CultureInfo.CurrentCulture, Resources.Sdv_ReportKeyUnknown, e.ReportKey), loc) { Path = path });

        switch (e.Class)
        {
            case "SDEventChangeState":
            case "SDEventSelectState":
                if (string.IsNullOrEmpty(e.NextState))
                    list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.ChangeStateNoNext, string.Format(CultureInfo.CurrentCulture, Resources.Sdv_NeedsNextState, e.Class), loc) { Path = path });
                break;
            case "SDEventWithDialog":
                if (string.IsNullOrEmpty(e.Dialog))
                    list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.DialogInvalid, Resources.Sdv_DialogMissing, loc) { Path = path });
                else if (!StateDgmKeys.Dialogs.Contains(e.Dialog))
                    list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.DialogInvalid, string.Format(CultureInfo.CurrentCulture, Resources.Sdv_DialogUnknown, e.Dialog, string.Join(", ", StateDgmKeys.Dialogs)), loc) { Path = path });
                break;
            case "SDEventVlakAttr":
                if (e.DelayArrival is not > 0 && e.DelayDeparture is not > 0)
                    list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.VlakAttrNoDelay, string.Format(CultureInfo.CurrentCulture, Resources.Sdv_VlakAttrNoDelay, StateDgmKeys.DelayArrival, StateDgmKeys.DelayDeparture), loc) { Path = path });
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
                list.Add(new StateDgmDiagnostic(ExprSeverity.Warning, StateDgmDiagnosticCode.StateUnreachable, Resources.Sdv_StateUnreachable, loc) { Path = path });
            else if (options.ReportInfos && !s.Events.Any(e => e.ChangesState) && !s.Attr.HasFlag(StateDgmAttr.Shadow))
                list.Add(new StateDgmDiagnostic(ExprSeverity.Info, StateDgmDiagnosticCode.StateDeadEnd, Resources.Sdv_StateDeadEnd, loc) { Path = path });
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
            var msg = x.IsError ? string.Format(CultureInfo.CurrentCulture, Resources.Sdv_ExpressionError, key, x.Message) : $"{key}: {x.Message}";
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
                list.Add(new StateDgmDiagnostic(ExprSeverity.Error, StateDgmDiagnosticCode.DuplicateKey, string.Format(CultureInfo.CurrentCulture, Resources.Sdv_DuplicateKey, what, k, first + 1), loc(i)) { Path = path(i) });
            else if (k.Length > 0)
                seen[k] = i;
            i++;
        }
    }
}
