namespace ToolsCore.StateDgm;

/// <summary>
///     Zapisovac suboru StateDgm.txt. Vytvara kanonicky tvar s tabulatormi a orientacnymi komentarmi
///     (rovnaky styl ako povodne subory INISSu); komentare z povodneho suboru sa nezachovavaju,
///     okrem hlavickovych <c>C:"…"</c>. Kluce <c>Num…</c> sa vzdy dopocitaju.
/// </summary>
public static class StateDgmWriter
{
    private const string NL = "\r\n";

    /// <summary>
    ///     Zapise diagram do textu.
    /// </summary>
    public static string Write(StateDgmDiagram d)
    {
        var sb = new StringBuilder();
        sb.Append(StateDgmReader.VERSION).Append(NL);
        foreach (var c in d.HeaderComments)
            sb.Append("C:\"").Append(c).Append('"').Append(NL);
        sb.Append(NL);

        // hodnoty priamo v StateDgmCtrls musia ist pred bloky s cestou, aby sa cesty pripojili k tejto skupine
        // (posledny clanok cesty INISS vzdy vytvori novy, medzilanky pouzije existujuce)
        var ctrlValues = d.CtrlsExtras.Where(i => i is StateDgmValue).ToList();
        if (ctrlValues.Count > 0)
        {
            sb.Append("P:\"").Append(StateDgmKeys.CTRLS).Append('"').Append(NL).Append('{').Append(NL);
            foreach (var item in ctrlValues) WriteItem(sb, item, 1);
            sb.Append('}').Append(NL).Append(NL);
        }

        WriteDesigns(sb, d);
        WriteHeader(sb, d);
        for (var i = 0; i < d.Categories.Count; i++)
            WriteCategory(sb, d.Categories[i], i + 1);

        foreach (var g in d.CtrlsExtras.OfType<StateDgmGroup>())
        {
            sb.Append("P:\"").Append(StateDgmKeys.CTRLS).Append("\\\\").Append(Escape(g.Name)).Append('"').Append(NL).Append('{').Append(NL);
            foreach (var sub in g.Items) WriteItem(sb, sub, 1);
            sb.Append('}').Append(NL).Append(NL);
        }

        foreach (var item in d.RootExtras)
        {
            sb.Append(NL);
            if (item is StateDgmGroup g)
            {
                sb.Append("P:\"").Append(Escape(g.Name)).Append('"').Append(NL).Append('{').Append(NL);
                foreach (var sub in g.Items) WriteItem(sb, sub, 1);
                sb.Append('}').Append(NL);
            }
            else
            {
                WriteItem(sb, item, 0);
            }
        }

        return sb.ToString();
    }

    private static void WriteDesigns(StringBuilder sb, StateDgmDiagram d)
    {
        sb.Append(';').Append(NL).Append("; Vzhľady tlačidiel").Append(NL).Append(';').Append(NL);
        sb.Append("P:\"").Append(StateDgmKeys.CTRLS).Append("\\\\").Append(StateDgmKeys.CTRL_DESIGN).Append('"').Append(NL);
        sb.Append("{\t;Bitmaps=\"Offset-Normal,NormalFokus,Pushed\"").Append(NL);
        var keyWidth = d.Designs.Count == 0 ? 0 : d.Designs.Max(x => x.Key.Length);
        for (var i = 0; i < d.Designs.Count; i++)
        {
            var des = d.Designs[i];
            var name = $"{StateDgmKeys.DESIGN}{i + 1}";
            sb.Append('\t').Append("G:\"").Append(name).Append('"').Append(name.Length < 8 ? " " : "").Append('{');
            sb.Append(Str(StateDgmKeys.KEY, des.Key)).Append(Pad(des.Key.Length, keyWidth));
            sb.Append(Str(StateDgmKeys.BITMAPS, des.Bitmaps)).Append(des.Bitmaps.Length < 8 ? "\t\t" : "\t");
            sb.Append(Int(StateDgmKeys.DEF_PUSH_BTN, des.DefaultPushButton ? 1 : 0)).Append('\t');
            sb.Append(Str(StateDgmKeys.CLASS, des.Class));
            foreach (var x in des.Extras) sb.Append('\t').Append(Inline(x));
            sb.Append('}').Append(NL);
        }

        sb.Append('\t').Append(Int(StateDgmKeys.NUM_DESIGNS, d.Designs.Count)).Append(NL);
        foreach (var x in d.DesignExtras) WriteItem(sb, x, 1);
        sb.Append('}').Append(NL).Append(NL);
    }

    private static void WriteHeader(StringBuilder sb, StateDgmDiagram d)
    {
        sb.Append("P:\"").Append(StateDgmKeys.CTRLS).Append("\\\\").Append(StateDgmKeys.STATE_DGM).Append('"').Append(NL).Append('{').Append(NL);
        sb.Append('\t').Append(Int(StateDgmKeys.NUM_TIME_POINTS, d.TimePoints.Count)).Append(NL);
        sb.Append('\t').Append(Int(StateDgmKeys.NUM_CATEGORIES, d.Categories.Count)).Append(NL);
        if (!string.IsNullOrWhiteSpace(d.IndCat))
            sb.Append('\t').Append(Str(StateDgmKeys.IND_CAT, d.IndCat)).Append(NL);
        for (var i = 0; i < d.TimePoints.Count; i++)
            WriteTimePoint(sb, d.TimePoints[i], i + 1, 1);

        foreach (var x in d.HeaderExtras) WriteItem(sb, x, 1);
        sb.Append('}').Append(NL).Append(NL);
    }

    private static void WriteTimePoint(StringBuilder sb, StateDgmTimePoint tp, int index, int indent)
    {
        var t1 = new string('\t', indent);
        var t2 = t1 + '\t';
        // bod bez zdrojov (napr. #StartTime) staci na jeden riadok
        if (tp.TimePointKey1.Length == 0 && tp.TimePointKey2.Length == 0 && tp.Extras.Count == 0)
        {
            sb.Append(t1).Append("G:\"").Append(StateDgmKeys.TIME_POINT).Append(index).Append("\" {").Append(Str(StateDgmKeys.KEY, tp.Key));
            if (tp.Name.Length > 0) sb.Append('\t').Append(Str(StateDgmKeys.NAME, tp.Name));
            sb.Append('}').Append(NL);
            return;
        }

        sb.Append(t1).Append("G:\"").Append(StateDgmKeys.TIME_POINT).Append(index).Append('"').Append(NL).Append(t1).Append('{').Append(NL);
        sb.Append(t2).Append(Str(StateDgmKeys.KEY, tp.Key)).Append(NL);
        if (tp.Name.Length > 0) sb.Append(t2).Append(Str(StateDgmKeys.NAME, tp.Name)).Append(NL);
        sb.Append(t2).Append(Str(StateDgmKeys.TIME_POINT_KEY1, tp.TimePointKey1)).Append(NL);
        sb.Append(t2).Append(Int(StateDgmKeys.TIME_POINT_OFFSET1, tp.Offset1)).Append("\t; ").Append(SecondsName(tp.Offset1)).Append(NL);
        sb.Append(t2).Append(Str(StateDgmKeys.TIME_POINT_KEY2, tp.TimePointKey2)).Append(NL);
        sb.Append(t2).Append(Int(StateDgmKeys.TIME_POINT_OFFSET2, tp.Offset2)).Append("\t; ").Append(SecondsName(tp.Offset2)).Append(NL);
        sb.Append(t2).Append(Str(StateDgmKeys.OPERATOR, tp.Operator)).Append(NL);
        foreach (var x in tp.Extras) WriteItem(sb, x, indent + 1);
        sb.Append(t1).Append('}').Append(NL);
    }

    private static void WriteCategory(StringBuilder sb, StateDgmCategory cat, int index)
    {
        sb.Append("P:\"").Append(StateDgmKeys.CTRLS).Append("\\\\").Append(StateDgmKeys.STATE_DGM).Append("\\\\").Append(StateDgmKeys.CATEGORIE).Append(index).Append('"').Append(NL);
        sb.Append('{').Append(NL);
        sb.Append('\t').Append(Str(StateDgmKeys.KEY, cat.Key)).Append(NL);
        sb.Append('\t').Append(Str(StateDgmKeys.NAME, cat.Name)).Append(NL);
        sb.Append('\t').Append(Str(StateDgmKeys.COMMENT, cat.Comment)).Append(NL);
        sb.Append('\t').Append(Int(StateDgmKeys.ICON, cat.Icon)).Append(NL);
        for (var i = 0; i < cat.States.Count; i++)
            WriteState(sb, cat.States[i], i + 1);
        sb.Append('\t').Append(Int(StateDgmKeys.NUM_STATES, cat.States.Count)).Append(NL);
        foreach (var x in cat.Extras) WriteItem(sb, x, 1);
        sb.Append('}').Append(NL).Append(NL);
    }

    private static void WriteState(StringBuilder sb, StateDgmState s, int index)
    {
        const string t2 = "\t\t";
        sb.Append('\t').Append("G:\"").Append(StateDgmKeys.STATE).Append(index).Append('"').Append(NL).Append("\t{").Append(NL);
        sb.Append(t2).Append(Str(StateDgmKeys.KEY, s.Key)).Append(NL);
        if (s.Name.Length > 0) sb.Append(t2).Append(Str(StateDgmKeys.NAME, s.Name)).Append(NL);
        sb.Append(t2).Append(Int(StateDgmKeys.ICON, s.Icon)).Append(NL);
        sb.Append(t2).Append("I:\"").Append(StateDgmKeys.ATTR).Append("\"=0x").Append(((int)s.Attr).ToString("X2", System.Globalization.CultureInfo.InvariantCulture));
        if (s.Attr != StateDgmAttr.None) sb.Append("\t; ").Append(AttrNames(s.Attr));
        sb.Append(NL);

        if (s.DoState != null) WriteTableSet(sb, StateDgmKeys.DO_STATE, s.DoState);
        if (s.UndoState != null) WriteTableSet(sb, StateDgmKeys.UNDO_STATE, s.UndoState);
        if (s.DefaultControl != 0) sb.Append(t2).Append(Int(StateDgmKeys.DEFAULT_CONTROL, s.DefaultControl)).Append(NL);

        if (s.AutoMode != null || s.AutoTimePoint != null || s.AutoTimePointAdd != null || s.AutoModif != null
            || s.AutoCondition != null || s.Wait != null || s.WaitPath != null)
        {
            sb.Append(t2).Append("; automatika").Append(NL);
            if (s.AutoMode != null) sb.Append(t2).Append(Dyn(StateDgmKeys.AUTO_MODE, s.AutoMode)).Append(Comment(s.AutoMode, AutoModeName)).Append(NL);
            if (s.AutoTimePoint != null) sb.Append(t2).Append(Dyn(StateDgmKeys.AUTO_TIME_POINT, s.AutoTimePoint)).Append(Comment(s.AutoTimePoint, AutoTimePointName)).Append(NL);
            if (s.AutoTimePointAdd != null) sb.Append(t2).Append(Dyn(StateDgmKeys.AUTO_TIME_POINT_ADD, s.AutoTimePointAdd)).Append(Comment(s.AutoTimePointAdd, SecondsName)).Append(NL);
            if (s.AutoModif != null) sb.Append(t2).Append(Dyn(StateDgmKeys.AUTO_MODIF, s.AutoModif)).Append(Comment(s.AutoModif, AutoModifName)).Append(NL);
            if (s.Wait != null) sb.Append(t2).Append(Dyn(StateDgmKeys.WAIT, s.Wait)).Append(NL);
            if (s.WaitPath != null) sb.Append(t2).Append(Dyn(StateDgmKeys.WAIT_PATH, s.WaitPath)).Append(NL);
            if (s.AutoCondition != null) sb.Append(t2).Append(Str(StateDgmKeys.AUTO_CONDITION, s.AutoCondition)).Append(NL);
        }

        if (s.TimePoints.Count > 0)
        {
            sb.Append(t2).Append(';').Append(NL).Append(t2).Append("; Časové body stavu").Append(NL).Append(t2).Append(';').Append(NL);
            for (var i = 0; i < s.TimePoints.Count; i++) WriteTimePoint(sb, s.TimePoints[i], i + 1, 2);
            sb.Append(t2).Append(Int(StateDgmKeys.NUM_TIME_POINTS, s.TimePoints.Count)).Append(NL);
        }

        if (s.Starters.Count > 0)
        {
            sb.Append(t2).Append(';').Append(NL).Append(t2).Append("; Štartéry").Append(NL).Append(t2).Append(';').Append(NL);
            for (var i = 0; i < s.Starters.Count; i++) WriteStarter(sb, s.Starters[i], i + 1);
            sb.Append(t2).Append(Int(StateDgmKeys.NUM_STARTERS, s.Starters.Count)).Append(NL);
        }

        sb.Append(t2).Append(';').Append(NL).Append(t2).Append("; Akcie").Append(NL).Append(t2).Append(';').Append(NL);
        var keyWidth = s.Events.Count == 0 ? 0 : s.Events.Max(e => e.Key.Length);
        for (var i = 0; i < s.Events.Count; i++) WriteEvent(sb, s.Events[i], i + 1, keyWidth);
        sb.Append(t2).Append(Int(StateDgmKeys.NUM_EVENTS, s.Events.Count)).Append(NL);

        sb.Append(t2).Append(';').Append(NL).Append(t2).Append("; Ovládače").Append(NL).Append(t2).Append(';').Append(NL);
        var designWidth = s.Controls.Count == 0 ? 0 : s.Controls.Max(c => c.DesignKey.Length);
        for (var i = 0; i < s.Controls.Count; i++)
        {
            var c = s.Controls[i];
            sb.Append(t2).Append("G:\"").Append(StateDgmKeys.CONTROL).Append(i + 1).Append("\" {");
            sb.Append(Int(StateDgmKeys.CTRL_ID, c.CtrlId)).Append('\t');
            sb.Append(Str(StateDgmKeys.DESIGN_KEY, c.DesignKey)).Append(Pad(c.DesignKey.Length, designWidth));
            sb.Append(Str(StateDgmKeys.EVENT_KEY, c.EventKey));
            foreach (var x in c.Extras) sb.Append('\t').Append(Inline(x));
            sb.Append('}').Append(NL);
        }

        sb.Append(t2).Append(Int(StateDgmKeys.NUM_CONTROLS, s.Controls.Count)).Append(NL);
        foreach (var x in s.Extras) WriteItem(sb, x, 2);
        sb.Append("\t}").Append(NL);
    }

    private static void WriteTableSet(StringBuilder sb, string name, StateDgmTableSet t)
    {
        const string t2 = "\t\t", t3 = "\t\t\t";
        sb.Append(t2).Append("G:\"").Append(name).Append('"').Append(NL).Append(t2).Append('{').Append(NL);
        sb.Append(t3).Append(Str(StateDgmKeys.CLASS, t.Class)).Append(NL);
        sb.Append(t3).Append(Bool(StateDgmKeys.ON_DEP_TABLE, t.OnDepartureTable)).Append(NL);
        sb.Append(t3).Append(Bool(StateDgmKeys.ON_ARR_TABLE, t.OnArrivalTable)).Append(NL);
        sb.Append(t3).Append(Bool(StateDgmKeys.ON_PLATFORM_TABLE, t.OnPlatformTables)).Append(NL);
        sb.Append(t3).Append(Bool(StateDgmKeys.SHOW_POSITION, t.ShowPosition)).Append(NL);
        sb.Append(t3).Append(Bool(StateDgmKeys.SHOW_TRACK, t.ShowTrack)).Append(NL);
        foreach (var x in t.Extras) WriteItem(sb, x, 3);
        sb.Append(t2).Append('}').Append(NL);
    }

    private static void WriteEvent(StringBuilder sb, StateDgmEvent e, int index, int keyWidth)
    {
        sb.Append("\t\t").Append("G:\"").Append(StateDgmKeys.EVENT).Append(index).Append("\" {");
        sb.Append(Str(StateDgmKeys.KEY, e.Key)).Append(Pad(e.Key.Length, keyWidth));
        if (e.Name != null) sb.Append(Str(StateDgmKeys.NAME, e.Name)).Append('\t');
        if (e.Icon != null) sb.Append(Int(StateDgmKeys.ICON, e.Icon.Value)).Append('\t');
        if (e.Comment != null) sb.Append(Str(StateDgmKeys.COMMENT, e.Comment)).Append('\t');
        if (e.Dialog != null) sb.Append(Str(StateDgmKeys.DIALOG, e.Dialog)).Append('\t');
        if (e.NextState != null) sb.Append(Str(StateDgmKeys.NEXT_STATE, e.NextState)).Append('\t');
        if (e.ReportKey != null) sb.Append(Str(StateDgmKeys.REPORT_KEY, e.ReportKey)).Append('\t');
        Opt(sb, StateDgmKeys.POS_FOR_ARRIVAL, e.PositionForArrival);
        Opt(sb, StateDgmKeys.POS_FOR_DEPARTURE, e.PositionForDeparture);
        Opt(sb, StateDgmKeys.COPY_POSITION, e.CopyPosition);
        Opt(sb, StateDgmKeys.MODIFY_REPORT, e.ModifyReport);
        Opt(sb, StateDgmKeys.ASK_REPORT, e.AskBeforeReport);
        Opt(sb, StateDgmKeys.HIDE_SHOW, e.HideShow);
        Opt(sb, StateDgmKeys.DELAY_ARRIVAL, e.DelayArrival);
        Opt(sb, StateDgmKeys.DELAY_DEPARTURE, e.DelayDeparture);
        sb.Append(Str(StateDgmKeys.CLASS, e.Class));
        foreach (var x in e.Extras) sb.Append('\t').Append(Inline(x));
        if (e.DoEvent != null) sb.Append('\t').Append(Inline(e.DoEvent));
        if (e.UndoEvent != null) sb.Append('\t').Append(Inline(e.UndoEvent));
        sb.Append('}').Append(NL);
    }

    private static void WriteStarter(StringBuilder sb, StateDgmStarter s, int index)
    {
        const string t2 = "\t\t", t3 = "\t\t\t";
        sb.Append(t2).Append("G:\"").Append(StateDgmKeys.STARTER).Append(index).Append("\" {")
            .Append(Str(StateDgmKeys.KEY, s.Key)).Append('\t').Append(Str(StateDgmKeys.EVENT_KEY, s.EventKey)).Append(NL);
        sb.Append(t3).Append(Str(StateDgmKeys.CLASS, s.Class)).Append(NL);
        sb.Append(t3).Append(Str(StateDgmKeys.TIME_POINT_KEY, s.TimePointKey)).Append(NL);
        sb.Append(t3).Append(Int(StateDgmKeys.TIME_OFFSET, s.TimeOffset)).Append("\t; ").Append(SecondsName(s.TimeOffset)).Append(NL);
        if (s.TimeOffsetStep != null) sb.Append(t3).Append(Int(StateDgmKeys.TIME_OFFSET_STEP, s.TimeOffsetStep.Value)).Append("\t; opakovať každých ").Append(SecondsName(s.TimeOffsetStep.Value)).Append(NL);
        if (s.TimePointKeyLast != null) sb.Append(t3).Append(Str(StateDgmKeys.TIME_POINT_KEY_LAST, s.TimePointKeyLast)).Append(NL);
        if (s.TimeOffsetLast != null) sb.Append(t3).Append(Int(StateDgmKeys.TIME_OFFSET_LAST, s.TimeOffsetLast.Value)).Append("\t; naposledy ").Append(SecondsName(s.TimeOffsetLast.Value)).Append(NL);
        if (s.StartLaterToo) sb.Append(t3).Append(Int(StateDgmKeys.START_LATER_TOO, 1)).Append(NL);
        foreach (var x in s.Extras) WriteItem(sb, x, 3);
        sb.Append(t2).Append('}').Append(NL);
    }

    #region Vseobecne polozky stromu

    /// <summary>Zapise polozku stromu (neznamy kluc alebo skupinu) na samostatny riadok s odsadenim.</summary>
    public static void WriteItem(StringBuilder sb, StateDgmItem item, int indent)
    {
        var tab = new string('\t', indent);
        if (item is StateDgmGroup g)
        {
            if (g.Items.Any(i => i is StateDgmGroup) || g.Items.Count > 4)
            {
                sb.Append(tab).Append("G:\"").Append(Escape(g.Name)).Append('"').Append(NL).Append(tab).Append('{').Append(NL);
                foreach (var sub in g.Items) WriteItem(sb, sub, indent + 1);
                sb.Append(tab).Append('}').Append(NL);
            }
            else
            {
                sb.Append(tab).Append(Inline(g)).Append(NL);
            }
        }
        else
        {
            sb.Append(tab).Append(Inline(item)).Append(NL);
        }
    }

    /// <summary>Polozka stromu v jednom riadku.</summary>
    public static string Inline(StateDgmItem item)
    {
        switch (item)
        {
            case StateDgmGroup g:
            {
                var sb = new StringBuilder();
                sb.Append("G:\"").Append(Escape(g.Name)).Append("\" {");
                for (var i = 0; i < g.Items.Count; i++)
                {
                    if (i > 0) sb.Append('\t');
                    sb.Append(Inline(g.Items[i]));
                }

                sb.Append('}');
                return sb.ToString();
            }
            case StateDgmValue v:
                if (v.IsRemoval)
                    return $"{Prefix(v.Kind)}:\"{Escape(v.Key)}\"=#";
                return v.Kind switch
                {
                    StateDgmValueKind.String => Str(v.Key, v.Text),
                    StateDgmValueKind.Int => v.Raw != null ? $"I:\"{Escape(v.Key)}\"={v.Raw}" : Int(v.Key, v.Number),
                    _ => Bool(v.Key, v.Flag)
                };
            default:
                throw new ArgumentOutOfRangeException(nameof(item));
        }
    }

    private static char Prefix(StateDgmValueKind kind) => kind switch
    {
        StateDgmValueKind.String => 'S',
        StateDgmValueKind.Int => 'I',
        _ => 'B'
    };

    /// <summary>Zapis <c>S:"kluc"="text"</c>.</summary>
    public static string Str(string key, string text) => $"S:\"{Escape(key)}\"=\"{Escape(text)}\"";

    /// <summary>Zapis <c>I:"kluc"=cislo</c>.</summary>
    public static string Int(string key, int number) => $"I:\"{Escape(key)}\"={number}";

    /// <summary>Zapis <c>B:"kluc"=Ano|Ne</c>.</summary>
    public static string Bool(string key, bool flag) => $"B:\"{Escape(key)}\"={(flag ? "Ano" : "Ne")}";

    /// <summary>Zapis dynamickej hodnoty - <c>I:</c> pre cislo, <c>S:</c> pre vyraz.</summary>
    public static string Dyn(string key, StateDgmDynamic value) => value.IsExpression ? Str(key, value.Expression!) : Int(key, value.Number!.Value);

    private static void Opt(StringBuilder sb, string key, int? value)
    {
        if (value != null) sb.Append(Int(key, value.Value)).Append('\t');
    }

    /// <summary>Escapovanie retazca do uvodzoviek podla citaca INISSu.</summary>
    public static string Escape(string s)
    {
        if (s.All(c => c != '\\' && c != '"' && c >= ' ')) return s;
        var sb = new StringBuilder(s.Length + 4);
        foreach (var c in s)
        {
            switch (c)
            {
                case '\\': sb.Append("\\\\"); break;
                case '"': sb.Append("\\\""); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                case < ' ': sb.Append("\\x").Append(((int)c).ToString("x2")); break;
                default: sb.Append(c); break;
            }
        }

        return sb.ToString();
    }

    #endregion

    #region Komentare

    private static string Pad(int length, int width)
    {
        // zarovnanie tabulatormi na sirku 8
        var tabs = (width + 8) / 8 - length / 8;
        return new string('\t', Math.Max(1, tabs));
    }

    private static string Comment(StateDgmDynamic v, Func<int, string?> name)
    {
        if (v.IsExpression) return "";
        var n = name(v.Number!.Value);
        return n == null ? "" : "\t; " + n;
    }

    /// <summary>Mena nastavenych bitov <c>Attr</c> (<c>SVSA_…</c>).</summary>
    public static string AttrNames(StateDgmAttr attr) =>
        string.Join(", ", Enum.GetValues<StateDgmAttr>().Where(a => a != StateDgmAttr.None && attr.HasFlag(a)).Select(a => "SVSA_" + a));

    private static string? AutoModeName(int n) => n switch
    {
        0 => "manual",
        1 => "poloautomat",
        2 => "automat",
        _ => null
    };

    private static string? AutoTimePointName(int n) => n switch
    {
        1 => "čas príchodu",
        2 => "čas odchodu",
        _ => null
    };

    private static string? AutoModifName(int n) => n switch
    {
        1 => "krátke hlásenie",
        2 => "dlhé hlásenie",
        _ => null
    };

    /// <summary>Sekundy ako citatelny posun (<c>-20 min</c>, <c>+90 s</c>).</summary>
    public static string SecondsName(int seconds)
    {
        var sign = seconds < 0 ? "-" : "+";
        var abs = Math.Abs(seconds);
        return abs % 60 == 0 ? $"{sign}{abs / 60} min" : $"{sign}{abs} s";
    }

    #endregion
}
