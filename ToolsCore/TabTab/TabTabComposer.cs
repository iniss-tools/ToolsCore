using ToolsCore.Expressions;

namespace ToolsCore.TabTab;

/// <summary>
///     Text s pismom - medzivysledok skladania textu stlpca.
/// </summary>
/// <param name="Text">Text.</param>
/// <param name="Font">Cislo pisma; <see langword="null"/> = nezmenene, <see cref="TabTabText.DefaultFont"/> = predvolene.</param>
public readonly record struct TabTabValue(string Text, int? Font)
{
    /// <summary>Prazdny text bez pisma.</summary>
    public static readonly TabTabValue Empty = new("", null);

    /// <summary>Ci text obsahuje <c>@</c> (odklad na dalsi zdroj).</summary>
    public bool HasPlaceholder => Text.Contains('@');

    /// <summary>Dosadi <paramref name="inner"/> za kazde <c>@</c>; pismo ostava vonkajsie, ak je zadane.</summary>
    public TabTabValue Fill(TabTabValue inner) => new(Text.Replace("@", inner.Text), Font ?? inner.Font);
}

/// <summary>
///     Vstup skladania textu jedneho stlpca katalogovej tabule.
/// </summary>
public sealed class TabTabColumnInput
{
    /// <summary>Vlak, pre ktory sa text sklada.</summary>
    public required IExprTrainContext Train { get; init; }

    /// <summary>Miesto vyhodnotenia (prichodova tabula alebo ine) - ovplyvnuje <c>VYLUKAZDE</c>, <c>ZPOZDENI</c> a pravidlo <c>#VYLUKA</c>.</summary>
    public ExprEvalSite Site { get; init; } = ExprEvalSite.Default;

    /// <summary>"Teraz" pre <c>DATE</c>/<c>TIME</c>.</summary>
    public DateTime? Now { get; init; }

    /// <summary>Hodnota, ktoru stlpec pocita sam (podla TYPE_ITEMS_IDX).</summary>
    public TabTabValue OwnValue { get; init; } = TabTabValue.Empty;

    /// <summary>Text z TTexts.txt pre tento stlpec a vlak; <see langword="null"/>, ak nie je.</summary>
    public TabTabValue? TTextsValue { get; init; }

    /// <summary>TYPE_ITEMS_DIVTYPE stlpca (0-4).</summary>
    public int DivType { get; init; }

    /// <summary>TYPE_ITEMS_IDX stlpca - pri 3 az 5 (smer, ciel) plati nahradny text vyluky/odklonu z programu.</summary>
    public int TypeItemsIdx { get; init; }

    /// <summary>Sekcia TAB1; <see langword="null"/>, ak nie je.</summary>
    public TabTabSection? Tab1 { get; init; }

    /// <summary>Sekcia TAB2; <see langword="null"/>, ak nie je.</summary>
    public TabTabSection? Tab2 { get; init; }

    /// <summary>Hodnota ineho stlpca tej istej tabule pre <c>%meno%</c>; <see langword="null"/>, ak stlpec nie je.</summary>
    public Func<string, TabTabValue?>? ColumnValue { get; init; }

    /// <summary>Nahradny text pri vyluke nastaveny v INISSe (krok 7); prazdny = nie je.</summary>
    public string FallbackLockoutText { get; init; } = "";

    /// <summary>Nahradny text pri odklone nastaveny v INISSe (krok 7); prazdny = nie je.</summary>
    public string FallbackDeflectText { get; init; } = "";

    /// <summary>Ci tabula vie striedat casti textu (<c>#</c>) - <c>#MERGE</c> potom spaja po castiach; inak ako <c>#MERGE2</c>.</summary>
    public bool MergeByParts { get; init; } = true;
}

/// <summary>
///     Jeden krok skladania - na zobrazenie postupu v nahlade.
/// </summary>
/// <param name="Source">Zdroj (napr. <c>#SWITCH</c>, <c>TTexts</c>, <c>vlastná hodnota</c>, <c>DIVTYPE 3</c>).</param>
/// <param name="Value">Vysledok zdroja.</param>
/// <param name="Note">Poznamka (ktora polozka platila, ci sa odlozilo …).</param>
public sealed record TabTabComposeStep(string Source, TabTabValue Value, string Note = "");

/// <summary>
///     Vysledok skladania textu stlpca.
/// </summary>
/// <param name="Value">Vysledny text a pismo.</param>
/// <param name="Steps">Postup po krokoch.</param>
public sealed record TabTabComposeResult(TabTabValue Value, IReadOnlyList<TabTabComposeStep> Steps)
{
    /// <summary>Chyba vyhodnotenia (delenie nulou …), ak nastala.</summary>
    public string? Error { get; init; }
}

/// <summary>
///     Sklada text stlpca tak, ako INISS - pevne poradie zdrojov
///     <c>#VYLUKA</c> → <c>#ODKLON</c> → <c>#POZODJ_</c> → <c>#SWITCH</c> → <c>#MERGE</c> → <c>#MERGE2</c> →
///     nahradny text programu → TTexts → vlastna hodnota; text s <c>@</c> sa odlozi a <c>@</c> vyplni dalsi
///     zdroj. Na hotovy text sa uplatnia jednoduche pravidla podla DIVTYPE.
/// </summary>
public static class TabTabComposer
{
    /// <summary>
    ///     Zlozi text stlpca.
    /// </summary>
    public static TabTabComposeResult Compose(TabTabColumnInput input)
    {
        var steps = new List<TabTabComposeStep>();
        string? error = null;
        var evaluator = new ExprEvaluator(input.Train, input.Site, input.Now);

        TabTabValue? pending = null;
        TabTabValue? final = null;

        // prijme vysledok zdroja: uzavrie, alebo odlozi (ak obsahuje @) / vyplni odlozene
        bool Accept(string source, TabTabValue v, string note)
        {
            if (pending is { } p)
            {
                v = p.Fill(v);
                note = note.Length == 0 ? "vyplnilo @ z predchádzajúceho zdroja" : note + "; vyplnilo @";
            }
            steps.Add(new TabTabComposeStep(source, v, note));
            if (v.HasPlaceholder)
            {
                pending = v;
                steps.Add(new TabTabComposeStep(source, v, "text obsahuje @ – čaká na ďalší zdroj"));
                return false;
            }
            final = v;
            return true;
        }

        var lockout = input.Site switch
        {
            ExprEvalSite.ArrivalTable => (input.Train.Flags & 0x100) != 0,
            ExprEvalSite.DepartureTable => (input.Train.Flags & 0x200) != 0,
            _ => (input.Train.Flags & 0x300) != 0
        };
        var deflected = input.Train.IsDeflected;

        // tabulky sa pouziju len pri DIVTYPE 1-4 (INISS sa pri 0 na tabulku vobec nepyta)
        var tab = input.DivType != 0 ? input.Tab1 : null;
        var rules = tab is null ? null : SimpleRuleTable.Build(tab);

        if (tab is not null && rules is not null)
        {
            // 1. #VYLUKA, 2. #ODKLON, 3. #POZODJ_<kolaj>
            if (lockout && rules.TryGet("#VYLUKA", out var vyl) && Accept("#VYLUKA", vyl, "vlak má výluku"))
                return Finish(input, final!.Value, steps, error);
            if (deflected && rules.TryGet("#ODKLON", out var odk) && Accept("#ODKLON", odk, "vlak má odklon"))
                return Finish(input, final!.Value, steps, error);
            var depTrack = input.Train.DepartureTrack;
            if (depTrack.Length > 0 && rules.TryGet("#POZODJ_" + depTrack, out var poz) && Accept("#POZODJ_" + depTrack, poz, $"koľaj odchodu {depTrack}"))
                return Finish(input, final!.Value, steps, error);

            // 4. #SWITCH
            foreach (var rule in tab.Rules.Where(r => r.Event == TabTabEventKind.Switch))
            {
                var hit = EvaluateSwitch(rule, evaluator, input, out var itemNote, ref error);
                if (hit is { } v && Accept("#SWITCH", v, itemNote))
                    return Finish(input, final!.Value, steps, error);
                if (hit is null) steps.Add(new TabTabComposeStep("#SWITCH", TabTabValue.Empty, itemNote));
            }

            // 5. #MERGE, 6. #MERGE2
            foreach (var kind in new[] { TabTabEventKind.Merge, TabTabEventKind.Merge2 })
            {
                var name = kind == TabTabEventKind.Merge ? "#MERGE" : "#MERGE2";
                foreach (var rule in tab.Rules.Where(r => r.Event == kind))
                {
                    var merged = EvaluateMerge(rule, kind == TabTabEventKind.Merge && input.MergeByParts, evaluator, input, out var note, ref error);
                    if (merged is { } v && Accept(name, v, note))
                        return Finish(input, final!.Value, steps, error);
                    if (merged is null) steps.Add(new TabTabComposeStep(name, TabTabValue.Empty, note));
                }
            }
        }

        // 7. nahradny text programu (len smer/ciel)
        if (input.TypeItemsIdx is >= 3 and <= 5)
        {
            if (lockout && input.FallbackLockoutText.Length > 0 && Accept("náhradný text výluky", new TabTabValue(input.FallbackLockoutText, null), "nastavenie INISSu"))
                return Finish(input, final!.Value, steps, error);
            if (!lockout && deflected && input.FallbackDeflectText.Length > 0 && Accept("náhradný text odklonu", new TabTabValue(input.FallbackDeflectText, null), "nastavenie INISSu"))
                return Finish(input, final!.Value, steps, error);
        }

        // 8. TTexts
        if (input.TTextsValue is { } tt && Accept("TTexts", tt, ""))
            return Finish(input, final!.Value, steps, error);

        // 9. vlastna hodnota
        var own = pending is { } pp ? pp.Fill(input.OwnValue) : input.OwnValue;
        steps.Add(new TabTabComposeStep("vlastná hodnota", own, pending is null ? "" : "vyplnilo @"));
        return Finish(input, own, steps, error);
    }

    /// <summary>
    ///     Uplatni jednoduche pravidla podla DIVTYPE na hotovy text.
    /// </summary>
    private static TabTabComposeResult Finish(TabTabColumnInput input, TabTabValue value, List<TabTabComposeStep> steps, string? error)
    {
        var result = ApplyDivType(input, value, steps);
        return new TabTabComposeResult(result, steps) { Error = error };
    }

    /// <summary>
    ///     Prekodovanie hotoveho textu tabulkami TAB1/TAB2 podla DIVTYPE (0-4).
    /// </summary>
    public static TabTabValue ApplyDivType(TabTabColumnInput input, TabTabValue value, List<TabTabComposeStep>? steps = null)
    {
        var tab1 = input.Tab1 is null ? null : SimpleRuleTable.Build(input.Tab1);
        var tab2 = input.Tab2 is null ? null : SimpleRuleTable.Build(input.Tab2);

        TabTabValue result;
        string note;
        switch (input.DivType)
        {
            case 1:
                result = tab1 is not null && tab1.TryGet(value.Text, out var v1) ? Apply(v1, value) : new TabTabValue("", value.Font);
                note = tab1 is null ? "TAB1 chýba" : result.Text.Length == 0 && value.Text.Length > 0 ? "text nie je v TAB1 – stĺpec ostane prázdny" : "";
                break;

            case 2:
            {
                if (value.Text.StartsWith('#') && tab1 is not null && tab1.TryGet(value.Text, out var whole))
                {
                    result = Apply(whole, value);
                    note = "text začínajúci # sa hľadá v TAB1 ako celok";
                    break;
                }
                var parts = value.Text.Split([':', '.', '/', '-', ',', ' '], 2);
                var hours = parts[0];
                var minutes = parts.Length > 1 ? parts[1] : "";
                var tens = minutes.Length > 0 ? minutes[..1] : "";
                var units = minutes.Length > 1 ? minutes[1..] : "";
                if (tab1 is not null && tab2 is not null
                    && tab1.TryGet(hours, out var h) && tab2.TryGet(tens, out var t) && tab2.TryGet(units, out var u))
                {
                    result = new TabTabValue(Apply(h, new TabTabValue(hours, value.Font)).Text + Apply(t, new TabTabValue(tens, value.Font)).Text + Apply(u, new TabTabValue(units, value.Font)).Text,
                        h.Font ?? t.Font ?? u.Font ?? value.Font);
                    note = "hodiny z TAB1, minúty po čísliciach z TAB2";
                }
                else
                {
                    result = new TabTabValue("", value.Font);
                    note = "niektorá časť času nie je v tabuľkách – stĺpec ostane prázdny";
                }
                break;
            }

            case 3:
                if (tab1 is not null && tab1.TryGet(value.Text, out var v3))
                {
                    result = Apply(v3, value);
                    note = "";
                }
                else
                {
                    result = value;
                    note = "text nie je v TAB1 – pošle sa nezmenený";
                }
                break;

            case 4:
            {
                if (value.Text.StartsWith('#') && tab1 is not null && tab1.TryGet(value.Text, out var whole4))
                {
                    result = Apply(whole4, value);
                    note = "text začínajúci # sa hľadá v TAB1 ako celok";
                    break;
                }
                var sb = new StringBuilder();
                int? font = null;
                foreach (var ch in value.Text)
                {
                    if (tab1 is not null && tab1.TryGet(ch.ToString(), out var cv))
                    {
                        sb.Append(cv.Text.Replace("@", ch.ToString()));
                        font ??= cv.Font;
                    }
                    else
                    {
                        sb.Append(' ');
                    }
                }
                result = new TabTabValue(sb.ToString(), font ?? value.Font);
                note = "znak po znaku podľa TAB1";
                break;
            }

            default:
                result = value;
                note = input.DivType == 0 ? "bez prekódovania" : $"neznámy DIVTYPE {input.DivType}";
                break;
        }

        steps?.Add(new TabTabComposeStep($"DIVTYPE {input.DivType}", result, note));
        return result;

        static TabTabValue Apply(TabTabValue rule, TabTabValue original) =>
            new(rule.Text.Replace("@", original.Text), rule.Font ?? original.Font);
    }

    /// <summary>
    ///     Vyhodnoti pravidlo <c>#SWITCH</c>: text prvej splnenej podmienky; <see langword="null"/>, ak ziadna neplati.
    /// </summary>
    private static TabTabValue? EvaluateSwitch(TabTabLine rule, ExprEvaluator evaluator, TabTabColumnInput input, out string note, ref string? error)
    {
        var items = rule.Items;
        for (var i = 0; i + 1 < items.Count; i += 2)
        {
            if (!TryCondition(items[i], evaluator, ref error, out var truthy)) continue;
            if (!truthy) continue;

            note = $"riadok {rule.LineIndex + 1}, položka {i + 1}: {items[i].Text}";
            return ResolveText(items[i + 1], input);
        }

        note = $"riadok {rule.LineIndex + 1}: žiadna podmienka neplatí";
        return null;
    }

    /// <summary>
    ///     Vyhodnoti pravidlo <c>#MERGE</c>/<c>#MERGE2</c>: pripoji texty vsetkych splnenych podmienok.
    /// </summary>
    private static TabTabValue? EvaluateMerge(TabTabLine rule, bool byParts, ExprEvaluator evaluator, TabTabColumnInput input, out string note, ref string? error)
    {
        var items = rule.Items;
        var acc = "";
        int? font = null;
        var used = new List<int>();

        for (var i = 0; i + 2 < items.Count; i += 3)
        {
            if (!TryCondition(items[i], evaluator, ref error, out var truthy) || !truthy) continue;

            var sep = items[i + 1].Decoded?.Text ?? "";
            var text = ResolveText(items[i + 2], input);
            if (text.Text.Length == 0) continue;

            used.Add(i + 1);
            font ??= text.Font;
            if (acc.Length == 0)
            {
                acc = text.Text;
                continue;
            }

            if (byParts && (acc.Contains('#') || text.Text.Contains('#')))
            {
                var accParts = acc.Split('#');
                var newParts = text.Text.Split('#');
                var n = Math.Max(accParts.Length, newParts.Length);
                var joined = new string[n];
                for (var k = 0; k < n; k++)
                    joined[k] = accParts[k % accParts.Length] + sep + newParts[k % newParts.Length];
                acc = string.Join("#", joined);
            }
            else
            {
                acc += sep + text.Text;
            }
        }

        if (used.Count == 0)
        {
            note = $"riadok {rule.LineIndex + 1}: žiadna podmienka neplatí";
            return null;
        }

        note = $"riadok {rule.LineIndex + 1}, položky {string.Join(", ", used)}";
        return new TabTabValue(acc, font);
    }

    private static bool TryCondition(TabTabItem item, ExprEvaluator evaluator, ref string? error, out bool truthy)
    {
        truthy = false;
        var parse = ExprParser.Parse(item.Text);
        if (parse.Root is null) return false; // INISS polozku, ktora sa nepreklada, preskoci
        try
        {
            truthy = evaluator.Evaluate(parse.Root) != 0;
            return true;
        }
        catch (ExprEvaluationException e)
        {
            error ??= $"{item.Text}: {e.Message}";
            return false;
        }
    }

    /// <summary>
    ///     Text polozky: dekodovany, <c>%meno%</c> nahradene hodnotou ineho stlpca.
    /// </summary>
    private static TabTabValue ResolveText(TabTabItem item, TabTabColumnInput input)
    {
        var d = item.Decoded ?? TabTabText.Decode(item.Text);
        var value = new TabTabValue(d.Text, d.Font);
        if (d.Text.Length > 2 && d.Text[0] == '%' && d.Text[^1] == '%' && input.ColumnValue is not null)
        {
            var other = input.ColumnValue(d.Text[1..^1]);
            if (other is { } o) value = new TabTabValue(o.Text, d.Font ?? o.Font);
        }
        return value;
    }

    /// <summary>
    ///     Tabulka jednoduchych pravidiel sekcie: prava strana (dekodovana) → lava strana s pismom.
    /// </summary>
    private sealed class SimpleRuleTable
    {
        private readonly Dictionary<string, TabTabValue> _map;
        private readonly bool _ignoreCase;

        private SimpleRuleTable(TabTabSection section)
        {
            _ignoreCase = section.IgnoreCase;
            _map = new Dictionary<string, TabTabValue>(_ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
            foreach (var rule in section.Rules)
            {
                if (rule.Event != TabTabEventKind.None && rule.Event is not (TabTabEventKind.Vyluka or TabTabEventKind.Odklon or TabTabEventKind.PozOdj))
                    continue;
                var key = TabTabText.Decode(rule.Right).Text;
                var left = TabTabText.Decode(rule.Left);
                // INISS: mapa nazov → hodnota, pri opakovani plati posledne pravidlo
                _map[key] = new TabTabValue(left.Text, left.Font);
            }
        }

        public static SimpleRuleTable Build(TabTabSection section) => new(section);

        public bool TryGet(string text, out TabTabValue value) => _map.TryGetValue(text, out value);
    }
}
