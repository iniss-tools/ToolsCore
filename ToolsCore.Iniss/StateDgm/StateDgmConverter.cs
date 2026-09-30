using System.Globalization;
using ToolsCore.Iniss.Properties;

namespace ToolsCore.Iniss.StateDgm;

/// <summary>
/// Prevod stromu suboru StateDgm.txt na typovany diagram. Kluce a skupiny, ktore model nepozna,
/// konci v <c>Extras</c> prislusneho prvku, aby sa pri zapise nestratili.
/// </summary>
public static class StateDgmConverter
{
    /// <summary>
    /// Prevedie strom na typovany diagram.
    /// </summary>
    public static StateDgmDiagram FromTree(StateDgmTextFile file)
    {
        var d = new StateDgmDiagram();
        d.HeaderComments.AddRange(file.HeaderComments);

        var ctrls = file.Root.GroupsNamed(StateDgmKeys.Ctrls).ToList();
        d.RootExtras.AddRange(file.Root.Items.Where(i => !(i is StateDgmGroup g && ctrls.Contains(g))));
        if (ctrls.Count == 0)
        {
            d.Warnings.Add(new StateDgmLoadWarning(string.Format(CultureInfo.CurrentCulture, Resources.Sdc_NoCtrls, StateDgmKeys.Ctrls), -1));
            return d;
        }

        // samostatny blok P:"StateDgmCtrls" vytvori dalsiu skupinu (posledny clanok cesty je vzdy novy);
        // pre model su vsetky jedna skupina
        var ctrl = new StateDgmGroup(StateDgmKeys.Ctrls);
        foreach (var c in ctrls) ctrl.Items.AddRange(c.Items);

        var design = ctrl.Group(StateDgmKeys.CtrlDesign);
        var header = ctrl.Group(StateDgmKeys.StateDgm);
        d.CtrlsExtras.AddRange(ctrl.Items.Where(i => i != design && i != header));

        if (design != null) ReadDesigns(d, design);
        if (header != null) ReadHeader(d, header);
        else d.Warnings.Add(new StateDgmLoadWarning(string.Format(CultureInfo.CurrentCulture, Resources.Sdc_BlockMissing, StateDgmKeys.Ctrls, StateDgmKeys.StateDgm), -1));
        return d;
    }

    private static void ReadDesigns(StateDgmDiagram d, StateDgmGroup design)
    {
        var r = new GroupReader(design);
        foreach (var g in r.Groups(StateDgmKeys.Design))
        {
            var gr = new GroupReader(g);
            var item = new StateDgmDesign
            {
                Line = g.Line,
                Key = gr.Str(StateDgmKeys.Key) ?? "",
                Bitmaps = gr.Str(StateDgmKeys.Bitmaps) ?? "",
                DefaultPushButton = gr.Int(StateDgmKeys.DefPushBtn) is > 0,
                Class = gr.Str(StateDgmKeys.Class) ?? StateDgmKeys.ClassDesignBtn
            };
            item.Extras.AddRange(gr.Extras());
            d.Designs.Add(item);
        }

        CheckCount(d, r.Int(StateDgmKeys.NumDesigns), d.Designs.Count, StateDgmKeys.NumDesigns, Resources.Sdc_WhatDesigns, design.Line);
        d.DesignExtras.AddRange(r.Extras());
    }

    private static void ReadHeader(StateDgmDiagram d, StateDgmGroup header)
    {
        var r = new GroupReader(header);
        d.IndCat = r.Str(StateDgmKeys.IndCat);

        foreach (var g in r.Groups(StateDgmKeys.TimePoint))
            d.TimePoints.Add(ReadTimePoint(g));

        CheckCount(d, r.Int(StateDgmKeys.NumTimePoints), d.TimePoints.Count, StateDgmKeys.NumTimePoints, Resources.Sdc_WhatTimePoints, header.Line);

        foreach (var g in r.Groups(StateDgmKeys.Categorie))
            d.Categories.Add(ReadCategory(d, g));

        CheckCount(d, r.Int(StateDgmKeys.NumCategories), d.Categories.Count, StateDgmKeys.NumCategories, Resources.Sdc_WhatCategories, header.Line);
        d.HeaderExtras.AddRange(r.Extras());
    }

    private static StateDgmTimePoint ReadTimePoint(StateDgmGroup g)
    {
        var gr = new GroupReader(g);
        var tp = new StateDgmTimePoint
        {
            Line = g.Line,
            Key = gr.Str(StateDgmKeys.Key) ?? "",
            Name = gr.Str(StateDgmKeys.Name) ?? "",
            TimePointKey1 = gr.Str(StateDgmKeys.TimePointKey1) ?? "",
            TimePointKey2 = gr.Str(StateDgmKeys.TimePointKey2) ?? "",
            Offset1 = gr.Int(StateDgmKeys.TimePointOffset1) ?? 0,
            Offset2 = gr.Int(StateDgmKeys.TimePointOffset2) ?? 0,
            Operator = gr.Str(StateDgmKeys.Operator) ?? StateDgmKeys.OperatorMin
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
            Key = r.Str(StateDgmKeys.Key) ?? "",
            Name = r.Str(StateDgmKeys.Name) ?? "",
            Comment = r.Str(StateDgmKeys.Comment) ?? "",
            Icon = r.Int(StateDgmKeys.Icon) ?? 0
        };
        foreach (var sg in r.Groups(StateDgmKeys.State))
            cat.States.Add(ReadState(d, sg));
        CheckCount(d, r.Int(StateDgmKeys.NumStates), cat.States.Count, StateDgmKeys.NumStates, string.Format(CultureInfo.CurrentCulture, Resources.Sdc_WhatStates, cat.Key), g.Line);
        cat.Extras.AddRange(r.Extras());
        return cat;
    }

    private static StateDgmState ReadState(StateDgmDiagram d, StateDgmGroup g)
    {
        var r = new GroupReader(g);
        var s = new StateDgmState
        {
            Line = g.Line,
            Key = r.Str(StateDgmKeys.Key) ?? "",
            Name = r.Str(StateDgmKeys.Name) ?? "",
            Icon = r.Int(StateDgmKeys.Icon) ?? 0,
            Attr = (StateDgmAttr)(r.Int(StateDgmKeys.Attr) ?? 0),
            AutoMode = r.Dyn(StateDgmKeys.AutoMode),
            AutoTimePoint = r.Dyn(StateDgmKeys.AutoTimePoint),
            AutoTimePointAdd = r.Dyn(StateDgmKeys.AutoTimePointAdd),
            AutoModif = r.Dyn(StateDgmKeys.AutoModif),
            AutoCondition = r.Str(StateDgmKeys.AutoCondition),
            Wait = r.Dyn(StateDgmKeys.Wait),
            WaitPath = r.Dyn(StateDgmKeys.WaitPath),
            DefaultControl = r.Int(StateDgmKeys.DefaultControl) ?? 0
        };

        // ciselny WaitPath je len stara podoba Wait=VVC
        if (s.WaitPath is { IsExpression: false })
        {
            if (s.WaitPath.Number != 0 && s.Wait == null)
                s.Wait = StateDgmDynamic.FromWait(StateDgmWaitEvent.Vvc);
            s.WaitPath = null;
        }

        var doGroup = r.Group(StateDgmKeys.DoState);
        if (doGroup != null) s.DoState = ReadTableSet(doGroup);
        var undoGroup = r.Group(StateDgmKeys.UndoState);
        if (undoGroup != null) s.UndoState = ReadTableSet(undoGroup);

        foreach (var eg in r.Groups(StateDgmKeys.Event)) s.Events.Add(ReadEvent(eg));
        foreach (var cg in r.Groups(StateDgmKeys.Control)) s.Controls.Add(ReadControl(cg));
        foreach (var sg in r.Groups(StateDgmKeys.Starter)) s.Starters.Add(ReadStarter(sg));
        foreach (var tg in r.Groups(StateDgmKeys.TimePoint)) s.TimePoints.Add(ReadTimePoint(tg));

        CheckCount(d, r.Int(StateDgmKeys.NumEvents), s.Events.Count, StateDgmKeys.NumEvents, string.Format(CultureInfo.CurrentCulture, Resources.Sdc_WhatEvents, s.Key), g.Line);
        CheckCount(d, r.Int(StateDgmKeys.NumControls), s.Controls.Count, StateDgmKeys.NumControls, string.Format(CultureInfo.CurrentCulture, Resources.Sdc_WhatControls, s.Key), g.Line);
        CheckCount(d, r.Int(StateDgmKeys.NumStarters), s.Starters.Count, StateDgmKeys.NumStarters, string.Format(CultureInfo.CurrentCulture, Resources.Sdc_WhatStarters, s.Key), g.Line);
        CheckCount(d, r.Int(StateDgmKeys.NumTimePoints), s.TimePoints.Count, StateDgmKeys.NumTimePoints, string.Format(CultureInfo.CurrentCulture, Resources.Sdc_WhatStateTimePoints, s.Key), g.Line);
        s.Extras.AddRange(r.Extras());
        return s;
    }

    private static StateDgmTableSet ReadTableSet(StateDgmGroup g)
    {
        var r = new GroupReader(g);
        var t = new StateDgmTableSet
        {
            Line = g.Line,
            Class = r.Str(StateDgmKeys.Class) ?? StateDgmKeys.ClassTableSet
        };
        // starsie anglicke kluce su aliasy - hodnota ktorehokolvek z dvojice zapne tabulu
        t.OnDepartureTable = (r.Bool(StateDgmKeys.OnDepTable) ?? false) || (r.Bool(StateDgmKeys.OnDepTableOld) ?? false);
        t.OnArrivalTable = (r.Bool(StateDgmKeys.OnArrTable) ?? false) || (r.Bool(StateDgmKeys.OnArrTableOld) ?? false);
        t.OnPlatformTables = (r.Bool(StateDgmKeys.OnPlatformTable) ?? false) || (r.Bool(StateDgmKeys.OnPlatformTableOld) ?? false);
        t.ShowPosition = (r.Bool(StateDgmKeys.ShowPosition) ?? false) || (r.Bool(StateDgmKeys.ShowPositionOld) ?? false);
        // kolaj: predvolene Ano v oboch klucoch, Ne v ktoromkolvek ju vypne
        t.ShowTrack = (r.Bool(StateDgmKeys.ShowTrack) ?? true) && (r.Bool(StateDgmKeys.ShowTrackOld) ?? true);
        t.Extras.AddRange(r.Extras());
        return t;
    }

    private static StateDgmEvent ReadEvent(StateDgmGroup g)
    {
        var r = new GroupReader(g);
        var e = new StateDgmEvent
        {
            Line = g.Line,
            Key = r.Str(StateDgmKeys.Key) ?? "",
            Name = r.Str(StateDgmKeys.Name),
            Icon = r.Int(StateDgmKeys.Icon),
            Comment = r.Str(StateDgmKeys.Comment),
            Class = r.Str(StateDgmKeys.Class) ?? "",
            NextState = r.Str(StateDgmKeys.NextState),
            ReportKey = r.Str(StateDgmKeys.ReportKey),
            Dialog = r.Str(StateDgmKeys.Dialog),
            PositionForArrival = r.Int(StateDgmKeys.PosForArrival),
            PositionForDeparture = r.Int(StateDgmKeys.PosForDeparture),
            CopyPosition = r.Int(StateDgmKeys.CopyPosition),
            ModifyReport = r.Int(StateDgmKeys.ModifyReport),
            AskBeforeReport = r.Int(StateDgmKeys.AskReport),
            HideShow = r.Int(StateDgmKeys.HideShow),
            DelayArrival = r.Int(StateDgmKeys.DelayArrival),
            DelayDeparture = r.Int(StateDgmKeys.DelayDeparture),
            DoEvent = r.Group(StateDgmKeys.DoEvent),
            UndoEvent = r.Group(StateDgmKeys.UndoEvent)
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
            CtrlId = r.Int(StateDgmKeys.CtrlID) ?? 0,
            DesignKey = r.Str(StateDgmKeys.DesignKey) ?? "",
            EventKey = r.Str(StateDgmKeys.EventKey) ?? ""
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
            Key = r.Str(StateDgmKeys.Key) ?? "",
            EventKey = r.Str(StateDgmKeys.EventKey) ?? "",
            Class = r.Str(StateDgmKeys.Class) ?? StateDgmKeys.ClassStarter,
            TimePointKey = r.Str(StateDgmKeys.TimePointKey) ?? "",
            TimeOffset = r.Int(StateDgmKeys.TimeOffset) ?? 0,
            TimeOffsetStep = r.Int(StateDgmKeys.TimeOffsetStep),
            TimePointKeyLast = r.Str(StateDgmKeys.TimePointKeyLast),
            TimeOffsetLast = r.Int(StateDgmKeys.TimeOffsetLast),
            StartLaterToo = r.Int(StateDgmKeys.StartLaterToo) is > 0
        };
        s.Extras.AddRange(r.Extras());
        return s;
    }

    /// <summary>
    /// INISS pouzije mensie z dvojice (kluc Num…, pocet skupin) a nesulad zapise do logu; editor nacita vsetky skupiny.
    /// </summary>
    private static void CheckCount(StateDgmDiagram d, int? declared, int actual, string key, string what, int line)
    {
        if (declared == null || declared == actual) return;
        d.Warnings.Add(new StateDgmLoadWarning(
            declared < actual
                ? string.Format(CultureInfo.CurrentCulture, Resources.Sdc_CountMore, key, declared, what, actual, declared)
                : string.Format(CultureInfo.CurrentCulture, Resources.Sdc_CountLess, key, declared, what, actual), line));
    }

    /// <summary>
    /// Citanie skupiny so sledovanim spotrebovanych poloziek.
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
