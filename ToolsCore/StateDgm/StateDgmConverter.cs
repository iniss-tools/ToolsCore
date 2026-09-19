namespace ToolsCore.StateDgm;

/// <summary>
///     Prevod stromu suboru StateDgm.txt na typovany diagram. Kluce a skupiny, ktore model nepozna,
///     konci v <c>Extras</c> prislusneho prvku, aby sa pri zapise nestratili.
/// </summary>
public static class StateDgmConverter
{
    /// <summary>
    ///     Prevedie strom na typovany diagram.
    /// </summary>
    public static StateDgmDiagram FromTree(StateDgmTextFile file)
    {
        var d = new StateDgmDiagram();
        d.HeaderComments.AddRange(file.HeaderComments);

        var ctrls = file.Root.GroupsNamed(StateDgmKeys.CTRLS).ToList();
        d.RootExtras.AddRange(file.Root.Items.Where(i => !(i is StateDgmGroup g && ctrls.Contains(g))));
        if (ctrls.Count == 0)
        {
            d.Warnings.Add(new StateDgmLoadWarning($"Súbor neobsahuje blok {StateDgmKeys.CTRLS} – INISS v ňom nenájde diagram", -1));
            return d;
        }

        // samostatny blok P:"StateDgmCtrls" vytvori dalsiu skupinu (posledny clanok cesty je vzdy novy);
        // pre model su vsetky jedna skupina
        var ctrl = new StateDgmGroup(StateDgmKeys.CTRLS);
        foreach (var c in ctrls) ctrl.Items.AddRange(c.Items);

        var design = ctrl.Group(StateDgmKeys.CTRL_DESIGN);
        var header = ctrl.Group(StateDgmKeys.STATE_DGM);
        d.CtrlsExtras.AddRange(ctrl.Items.Where(i => i != design && i != header));

        if (design != null) ReadDesigns(d, design);
        if (header != null) ReadHeader(d, header);
        else d.Warnings.Add(new StateDgmLoadWarning($"Chýba blok {StateDgmKeys.CTRLS}\\{StateDgmKeys.STATE_DGM}", -1));
        return d;
    }

    private static void ReadDesigns(StateDgmDiagram d, StateDgmGroup design)
    {
        var r = new GroupReader(design);
        foreach (var g in r.Groups(StateDgmKeys.DESIGN))
        {
            var gr = new GroupReader(g);
            var item = new StateDgmDesign
            {
                Line = g.Line,
                Key = gr.Str(StateDgmKeys.KEY) ?? "",
                Bitmaps = gr.Str(StateDgmKeys.BITMAPS) ?? "",
                DefaultPushButton = gr.Int(StateDgmKeys.DEF_PUSH_BTN) is > 0,
                Class = gr.Str(StateDgmKeys.CLASS) ?? StateDgmKeys.CLASS_DESIGN_BTN
            };
            item.Extras.AddRange(gr.Extras());
            d.Designs.Add(item);
        }

        CheckCount(d, r.Int(StateDgmKeys.NUM_DESIGNS), d.Designs.Count, StateDgmKeys.NUM_DESIGNS, "vzhľadov", design.Line);
        d.DesignExtras.AddRange(r.Extras());
    }

    private static void ReadHeader(StateDgmDiagram d, StateDgmGroup header)
    {
        var r = new GroupReader(header);
        d.IndCat = r.Str(StateDgmKeys.IND_CAT);

        foreach (var g in r.Groups(StateDgmKeys.TIME_POINT))
            d.TimePoints.Add(ReadTimePoint(g));

        CheckCount(d, r.Int(StateDgmKeys.NUM_TIME_POINTS), d.TimePoints.Count, StateDgmKeys.NUM_TIME_POINTS, "časových bodov", header.Line);

        foreach (var g in r.Groups(StateDgmKeys.CATEGORIE))
            d.Categories.Add(ReadCategory(d, g));

        CheckCount(d, r.Int(StateDgmKeys.NUM_CATEGORIES), d.Categories.Count, StateDgmKeys.NUM_CATEGORIES, "kategórií", header.Line);
        d.HeaderExtras.AddRange(r.Extras());
    }

    private static StateDgmTimePoint ReadTimePoint(StateDgmGroup g)
    {
        var gr = new GroupReader(g);
        var tp = new StateDgmTimePoint
        {
            Line = g.Line,
            Key = gr.Str(StateDgmKeys.KEY) ?? "",
            Name = gr.Str(StateDgmKeys.NAME) ?? "",
            TimePointKey1 = gr.Str(StateDgmKeys.TIME_POINT_KEY1) ?? "",
            TimePointKey2 = gr.Str(StateDgmKeys.TIME_POINT_KEY2) ?? "",
            Offset1 = gr.Int(StateDgmKeys.TIME_POINT_OFFSET1) ?? 0,
            Offset2 = gr.Int(StateDgmKeys.TIME_POINT_OFFSET2) ?? 0,
            Operator = gr.Str(StateDgmKeys.OPERATOR) ?? StateDgmKeys.OPERATOR_MIN
        };
        tp.Extras.AddRange(gr.Extras());
        return tp;
    }

    private static StateDgmCategory ReadCategory(StateDgmDiagram d, StateDgmGroup g)
    {
        var r = new GroupReader(g);
        var cat = new StateDgmCategory
        {
            Line = g.Line,
            Key = r.Str(StateDgmKeys.KEY) ?? "",
            Name = r.Str(StateDgmKeys.NAME) ?? "",
            Comment = r.Str(StateDgmKeys.COMMENT) ?? "",
            Icon = r.Int(StateDgmKeys.ICON) ?? 0
        };
        foreach (var sg in r.Groups(StateDgmKeys.STATE))
            cat.States.Add(ReadState(d, sg));
        CheckCount(d, r.Int(StateDgmKeys.NUM_STATES), cat.States.Count, StateDgmKeys.NUM_STATES, $"stavov kategórie {cat.Key}", g.Line);
        cat.Extras.AddRange(r.Extras());
        return cat;
    }

    private static StateDgmState ReadState(StateDgmDiagram d, StateDgmGroup g)
    {
        var r = new GroupReader(g);
        var s = new StateDgmState
        {
            Line = g.Line,
            Key = r.Str(StateDgmKeys.KEY) ?? "",
            Name = r.Str(StateDgmKeys.NAME) ?? "",
            Icon = r.Int(StateDgmKeys.ICON) ?? 0,
            Attr = (StateDgmAttr)(r.Int(StateDgmKeys.ATTR) ?? 0),
            AutoMode = r.Dyn(StateDgmKeys.AUTO_MODE),
            AutoTimePoint = r.Dyn(StateDgmKeys.AUTO_TIME_POINT),
            AutoTimePointAdd = r.Dyn(StateDgmKeys.AUTO_TIME_POINT_ADD),
            AutoModif = r.Dyn(StateDgmKeys.AUTO_MODIF),
            AutoCondition = r.Str(StateDgmKeys.AUTO_CONDITION),
            Wait = r.Dyn(StateDgmKeys.WAIT),
            WaitPath = r.Dyn(StateDgmKeys.WAIT_PATH),
            DefaultControl = r.Int(StateDgmKeys.DEFAULT_CONTROL) ?? 0
        };

        // ciselny WaitPath je len stara podoba Wait=VVC
        if (s.WaitPath is { IsExpression: false })
        {
            if (s.WaitPath.Number != 0 && s.Wait == null)
                s.Wait = StateDgmDynamic.FromWait(StateDgmWaitEvent.VVC);
            s.WaitPath = null;
        }

        var doGroup = r.Group(StateDgmKeys.DO_STATE);
        if (doGroup != null) s.DoState = ReadTableSet(doGroup);
        var undoGroup = r.Group(StateDgmKeys.UNDO_STATE);
        if (undoGroup != null) s.UndoState = ReadTableSet(undoGroup);

        foreach (var eg in r.Groups(StateDgmKeys.EVENT)) s.Events.Add(ReadEvent(eg));
        foreach (var cg in r.Groups(StateDgmKeys.CONTROL)) s.Controls.Add(ReadControl(cg));
        foreach (var sg in r.Groups(StateDgmKeys.STARTER)) s.Starters.Add(ReadStarter(sg));
        foreach (var tg in r.Groups(StateDgmKeys.TIME_POINT)) s.TimePoints.Add(ReadTimePoint(tg));

        CheckCount(d, r.Int(StateDgmKeys.NUM_EVENTS), s.Events.Count, StateDgmKeys.NUM_EVENTS, $"akcií stavu {s.Key}", g.Line);
        CheckCount(d, r.Int(StateDgmKeys.NUM_CONTROLS), s.Controls.Count, StateDgmKeys.NUM_CONTROLS, $"ovládačov stavu {s.Key}", g.Line);
        CheckCount(d, r.Int(StateDgmKeys.NUM_STARTERS), s.Starters.Count, StateDgmKeys.NUM_STARTERS, $"štartérov stavu {s.Key}", g.Line);
        CheckCount(d, r.Int(StateDgmKeys.NUM_TIME_POINTS), s.TimePoints.Count, StateDgmKeys.NUM_TIME_POINTS, $"časových bodov stavu {s.Key}", g.Line);
        s.Extras.AddRange(r.Extras());
        return s;
    }

    private static StateDgmTableSet ReadTableSet(StateDgmGroup g)
    {
        var r = new GroupReader(g);
        var t = new StateDgmTableSet
        {
            Line = g.Line,
            Class = r.Str(StateDgmKeys.CLASS) ?? StateDgmKeys.CLASS_TABLE_SET
        };
        // starsie anglicke kluce su aliasy - hodnota ktorehokolvek z dvojice zapne tabulu
        t.OnDepartureTable = (r.Bool(StateDgmKeys.ON_DEP_TABLE) ?? false) || (r.Bool(StateDgmKeys.ON_DEP_TABLE_OLD) ?? false);
        t.OnArrivalTable = (r.Bool(StateDgmKeys.ON_ARR_TABLE) ?? false) || (r.Bool(StateDgmKeys.ON_ARR_TABLE_OLD) ?? false);
        t.OnPlatformTables = (r.Bool(StateDgmKeys.ON_PLATFORM_TABLE) ?? false) || (r.Bool(StateDgmKeys.ON_PLATFORM_TABLE_OLD) ?? false);
        t.ShowPosition = (r.Bool(StateDgmKeys.SHOW_POSITION) ?? false) || (r.Bool(StateDgmKeys.SHOW_POSITION_OLD) ?? false);
        // kolaj: predvolene Ano v oboch klucoch, Ne v ktoromkolvek ju vypne
        t.ShowTrack = (r.Bool(StateDgmKeys.SHOW_TRACK) ?? true) && (r.Bool(StateDgmKeys.SHOW_TRACK_OLD) ?? true);
        t.Extras.AddRange(r.Extras());
        return t;
    }

    private static StateDgmEvent ReadEvent(StateDgmGroup g)
    {
        var r = new GroupReader(g);
        var e = new StateDgmEvent
        {
            Line = g.Line,
            Key = r.Str(StateDgmKeys.KEY) ?? "",
            Name = r.Str(StateDgmKeys.NAME),
            Icon = r.Int(StateDgmKeys.ICON),
            Comment = r.Str(StateDgmKeys.COMMENT),
            Class = r.Str(StateDgmKeys.CLASS) ?? "",
            NextState = r.Str(StateDgmKeys.NEXT_STATE),
            ReportKey = r.Str(StateDgmKeys.REPORT_KEY),
            Dialog = r.Str(StateDgmKeys.DIALOG),
            PositionForArrival = r.Int(StateDgmKeys.POS_FOR_ARRIVAL),
            PositionForDeparture = r.Int(StateDgmKeys.POS_FOR_DEPARTURE),
            CopyPosition = r.Int(StateDgmKeys.COPY_POSITION),
            ModifyReport = r.Int(StateDgmKeys.MODIFY_REPORT),
            AskBeforeReport = r.Int(StateDgmKeys.ASK_REPORT),
            HideShow = r.Int(StateDgmKeys.HIDE_SHOW),
            DelayArrival = r.Int(StateDgmKeys.DELAY_ARRIVAL),
            DelayDeparture = r.Int(StateDgmKeys.DELAY_DEPARTURE),
            DoEvent = r.Group(StateDgmKeys.DO_EVENT),
            UndoEvent = r.Group(StateDgmKeys.UNDO_EVENT)
        };
        e.Extras.AddRange(r.Extras());
        return e;
    }

    private static StateDgmControl ReadControl(StateDgmGroup g)
    {
        var r = new GroupReader(g);
        var c = new StateDgmControl
        {
            Line = g.Line,
            CtrlId = r.Int(StateDgmKeys.CTRL_ID) ?? 0,
            DesignKey = r.Str(StateDgmKeys.DESIGN_KEY) ?? "",
            EventKey = r.Str(StateDgmKeys.EVENT_KEY) ?? ""
        };
        c.Extras.AddRange(r.Extras());
        return c;
    }

    private static StateDgmStarter ReadStarter(StateDgmGroup g)
    {
        var r = new GroupReader(g);
        var s = new StateDgmStarter
        {
            Line = g.Line,
            Key = r.Str(StateDgmKeys.KEY) ?? "",
            EventKey = r.Str(StateDgmKeys.EVENT_KEY) ?? "",
            Class = r.Str(StateDgmKeys.CLASS) ?? StateDgmKeys.CLASS_STARTER,
            TimePointKey = r.Str(StateDgmKeys.TIME_POINT_KEY) ?? "",
            TimeOffset = r.Int(StateDgmKeys.TIME_OFFSET) ?? 0,
            TimeOffsetStep = r.Int(StateDgmKeys.TIME_OFFSET_STEP),
            TimePointKeyLast = r.Str(StateDgmKeys.TIME_POINT_KEY_LAST),
            TimeOffsetLast = r.Int(StateDgmKeys.TIME_OFFSET_LAST),
            StartLaterToo = r.Int(StateDgmKeys.START_LATER_TOO) is > 0
        };
        s.Extras.AddRange(r.Extras());
        return s;
    }

    /// <summary>
    ///     INISS pouzije mensie z dvojice (kluc Num…, pocet skupin) a nesulad zapise do logu; editor nacita vsetky skupiny.
    /// </summary>
    private static void CheckCount(StateDgmDiagram d, int? declared, int actual, string key, string what, int line)
    {
        if (declared == null || declared == actual) return;
        d.Warnings.Add(new StateDgmLoadWarning(
            declared < actual
                ? $"{key}={declared}, ale {what} je {actual} – INISS načíta len prvých {declared}; po uložení sa počet opraví"
                : $"{key}={declared}, ale {what} je len {actual} – po uložení sa počet opraví", line));
    }

    /// <summary>
    ///     Citanie skupiny so sledovanim spotrebovanych poloziek.
    /// </summary>
    private sealed class GroupReader(StateDgmGroup group)
    {
        private readonly HashSet<StateDgmItem> _used = [];

        public string? Str(string key)
        {
            var v = group.Value(key);
            if (v == null) return null;
            Use(key);
            return group.GetString(key);
        }

        public int? Int(string key)
        {
            var v = group.Value(key);
            if (v == null) return null;
            Use(key);
            return group.GetInt(key);
        }

        public bool? Bool(string key)
        {
            var v = group.Value(key);
            if (v == null) return null;
            Use(key);
            return group.GetBool(key);
        }

        /// <summary>Kluc citany INISSom ako cislo aj ako vyraz.</summary>
        public StateDgmDynamic? Dyn(string key)
        {
            var v = group.Value(key);
            if (v == null) return null;
            Use(key);
            return v.Kind switch
            {
                StateDgmValueKind.Int => StateDgmDynamic.FromNumber(v.Number),
                StateDgmValueKind.Bool => StateDgmDynamic.FromNumber(v.Flag ? 1 : 0),
                _ => StateDgmDynamic.FromExpression(v.Text)
            };
        }

        public StateDgmGroup? Group(string name)
        {
            var g = group.Group(name);
            if (g == null) return null;
            foreach (var x in group.Groups.Where(x => x.Name == name)) _used.Add(x);
            return g;
        }

        public List<StateDgmGroup> Groups(string baseName)
        {
            var list = group.GroupsNamed(baseName).ToList();
            foreach (var g in list) _used.Add(g);
            return list;
        }

        public IEnumerable<StateDgmItem> Extras() => group.Items.Where(i => !_used.Contains(i)).Select(i => i.Clone());

        private void Use(string key)
        {
            foreach (var v in group.Values.Where(v => v.Key == key)) _used.Add(v);
        }
    }
}
