namespace ToolsCore.StateDgm;

/// <summary>
///     Prenos noveho kluca do odkazov po premenovani prvku diagramu. Prvok uz ma novy kluc, metody dostanu stary.
///     <list type="bullet">
///         <item>stav → <c>NextState</c> akcii vo vsetkych stavoch tej istej kategorie,</item>
///         <item>vzhlad → <c>DesignKey</c> tlacidiel celeho diagramu,</item>
///         <item>casovy bod hlavicky → <c>TimePointKey1/2</c> bodov a <c>TimePointKey(Last)</c> starterov celeho diagramu
///             (okrem stavov, ktore maju vlastny bod s rovnakym klucom),</item>
///         <item>casovy bod stavu → body a startery toho stavu,</item>
///         <item>akcia → <c>EventKey</c> tlacidiel a starterov toho stavu.</item>
///     </list>
///     Premenovanie sa odmietne (odkazy ostanu), ked by bolo nejednoznacne: prazdny stary alebo novy kluc, stary kluc
///     ma aj iny prvok v rozsahu (odkazy teda patria aj jemu), novy kluc uz ma iny prvok v rozsahu alebo sa nan
///     v rozsahu uz nieco odkazuje (spojili by sa s cudzimi odkazmi). Duplicitu potom ohlasi validator.
/// </summary>
public static class StateDgmRename
{
    /// <summary>
    ///     Prenesie novy kluc prvku (stav, vzhlad, casovy bod, akcia) do odkazov nan.
    /// </summary>
    /// <param name="d">Diagram, v ktorom prvok je.</param>
    /// <param name="element">Premenovany prvok (uz s novym klucom).</param>
    /// <param name="oldKey">Kluc, na ktory ukazuju odkazy.</param>
    /// <param name="changed">Pocet prepisanych odkazov.</param>
    /// <returns>false, ked sa premenovanie odmietlo alebo prvok v diagrame nie je; odkazy ostali nezmenene.</returns>
    public static bool TryRename(StateDgmDiagram d, StateDgmElement element, string oldKey, out int changed)
    {
        changed = 0;
        return element switch
        {
            StateDgmState s => TryRenameState(d, s, oldKey, out changed),
            StateDgmDesign ds => TryRenameDesign(d, ds, oldKey, out changed),
            StateDgmTimePoint t => TryRenameTimePoint(d, t, oldKey, out changed),
            StateDgmEvent e => TryRenameEvent(d, e, oldKey, out changed),
            _ => false
        };
    }

    /// <summary>Stav → <c>NextState</c> akcii tej istej kategorie.</summary>
    public static bool TryRenameState(StateDgmDiagram d, StateDgmState state, string oldKey, out int changed)
    {
        changed = 0;
        var cat = d.Categories.FirstOrDefault(c => c.States.Contains(state));
        if (cat == null) return false;
        var newKey = state.Key;
        if (oldKey == newKey) return true;
        var others = cat.States.Where(s => s != state).Select(s => s.Key);
        var events = cat.States.SelectMany(s => s.Events).ToList();
        if (!CanRename(oldKey, newKey, others, events.Select(e => e.NextState))) return false;

        foreach (var e in events.Where(e => e.NextState == oldKey))
        {
            e.NextState = newKey;
            changed++;
        }

        return true;
    }

    /// <summary>Vzhlad → <c>DesignKey</c> tlacidiel celeho diagramu.</summary>
    public static bool TryRenameDesign(StateDgmDiagram d, StateDgmDesign design, string oldKey, out int changed)
    {
        changed = 0;
        if (!d.Designs.Contains(design)) return false;
        var newKey = design.Key;
        if (oldKey == newKey) return true;
        var controls = d.Categories.SelectMany(c => c.States).SelectMany(s => s.Controls).ToList();
        if (!CanRename(oldKey, newKey, d.Designs.Where(x => x != design).Select(x => x.Key), controls.Select(c => c.DesignKey))) return false;

        foreach (var c in controls.Where(c => c.DesignKey == oldKey))
        {
            c.DesignKey = newKey;
            changed++;
        }

        return true;
    }

    /// <summary>Casovy bod hlavicky → cely diagram; bod stavu → ten stav.</summary>
    public static bool TryRenameTimePoint(StateDgmDiagram d, StateDgmTimePoint tp, string oldKey, out int changed)
    {
        changed = 0;
        var newKey = tp.Key;
        var states = d.Categories.SelectMany(c => c.States).ToList();
        List<StateDgmTimePoint> points;
        List<StateDgmStarter> starters;
        IEnumerable<string> others;

        if (d.TimePoints.Contains(tp))
        {
            if (oldKey == newKey) return true;
            // stav s vlastnym bodom stareho kluca ho zakryva - jeho odkazy nepatria bodu hlavicky
            var scope = states.Where(s => s.TimePoints.All(x => x.Key != oldKey)).ToList();
            points = d.TimePoints.Concat(scope.SelectMany(s => s.TimePoints)).ToList();
            starters = scope.SelectMany(s => s.Starters).ToList();
            // novy kluc nesmie mat ani vlastny bod niektoreho stavu - odkazy v nom by sa naviazali nanho
            others = StateDgmKeys.BuiltInTimePoints.Concat(d.TimePoints.Where(x => x != tp).Select(x => x.Key))
                .Concat(states.SelectMany(s => s.TimePoints).Select(x => x.Key).Where(k => k == newKey));
        }
        else
        {
            var owner = states.FirstOrDefault(s => s.TimePoints.Contains(tp));
            if (owner == null) return false;
            if (oldKey == newKey) return true;
            points = owner.TimePoints;
            starters = owner.Starters;
            others = StateDgmKeys.BuiltInTimePoints.Concat(d.TimePoints.Select(x => x.Key)).Concat(owner.TimePoints.Where(x => x != tp).Select(x => x.Key));
        }

        var refs = points.Where(x => x != tp).SelectMany(x => new[] { x.TimePointKey1, x.TimePointKey2 })
            .Concat(starters.SelectMany(s => new[] { s.TimePointKey, s.TimePointKeyLast }));
        if (!CanRename(oldKey, newKey, others, refs)) return false;

        foreach (var x in points.Where(x => x != tp))
        {
            if (x.TimePointKey1 == oldKey)
            {
                x.TimePointKey1 = newKey;
                changed++;
            }

            if (x.TimePointKey2 == oldKey)
            {
                x.TimePointKey2 = newKey;
                changed++;
            }
        }

        foreach (var s in starters)
        {
            if (s.TimePointKey == oldKey)
            {
                s.TimePointKey = newKey;
                changed++;
            }

            if (s.TimePointKeyLast == oldKey)
            {
                s.TimePointKeyLast = newKey;
                changed++;
            }
        }

        return true;
    }

    /// <summary>Akcia → <c>EventKey</c> tlacidiel a starterov toho stavu.</summary>
    public static bool TryRenameEvent(StateDgmDiagram d, StateDgmEvent ev, string oldKey, out int changed)
    {
        changed = 0;
        var owner = d.Categories.SelectMany(c => c.States).FirstOrDefault(s => s.Events.Contains(ev));
        if (owner == null) return false;
        var newKey = ev.Key;
        if (oldKey == newKey) return true;
        var refs = owner.Controls.Select(c => c.EventKey).Concat(owner.Starters.Select(s => s.EventKey));
        if (!CanRename(oldKey, newKey, owner.Events.Where(x => x != ev).Select(x => x.Key), refs)) return false;

        foreach (var c in owner.Controls.Where(c => c.EventKey == oldKey))
        {
            c.EventKey = newKey;
            changed++;
        }

        foreach (var s in owner.Starters.Where(s => s.EventKey == oldKey))
        {
            s.EventKey = newKey;
            changed++;
        }

        return true;
    }

    /// <summary>Premenovanie je jednoznacne (pozri popis triedy).</summary>
    private static bool CanRename(string oldKey, string newKey, IEnumerable<string> otherKeys, IEnumerable<string?> references)
    {
        if (oldKey.Length == 0 || newKey.Length == 0) return false;
        var others = otherKeys.ToHashSet(StringComparer.Ordinal);
        if (others.Contains(oldKey) || others.Contains(newKey)) return false;
        return !references.Contains(newKey, StringComparer.Ordinal);
    }
}
