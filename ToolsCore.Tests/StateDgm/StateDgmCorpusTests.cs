using System.Diagnostics.CodeAnalysis;
using System.Text;
using ToolsCore.StateDgm;

namespace ToolsCore.Tests.StateDgm;

/// <summary>
///     Realne subory StateDgm.txt: kazdy sa musi nacitat, previest na typovany model, zapisat a po opatovnom
///     nacitani dat rovnaky vyznam (rovnake skupiny v rovnakom poradi, rovnake kluce a hodnoty).
/// </summary>
[TestClass]
[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
public class StateDgmCorpusTests
{
    private const string LiveRoot = @"D:\INISSroot";

    /// <summary>Kluce, ktore zapisovac dopocitava, a aliasy, ktore normalizuje - do porovnania nejdu.</summary>
    private static readonly HashSet<string> Ignored =
    [
        StateDgmKeys.NUM_DESIGNS, StateDgmKeys.NUM_TIME_POINTS, StateDgmKeys.NUM_CATEGORIES, StateDgmKeys.NUM_STATES,
        StateDgmKeys.NUM_EVENTS, StateDgmKeys.NUM_CONTROLS, StateDgmKeys.NUM_STARTERS
    ];

    [TestMethod]
    public void LiveData_RoundTrip()
    {
        if (!Directory.Exists(LiveRoot))
            Assert.Inconclusive($"{LiveRoot} nie je dostupny");

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var enc = Encoding.GetEncoding(1250);

        var files = 0;
        var states = 0;
        var problems = new List<string>();
        foreach (var file in Directory.EnumerateFiles(LiveRoot, "statedgm.txt", new EnumerationOptions { RecurseSubdirectories = true, MatchCasing = MatchCasing.CaseInsensitive }))
        {
            files++;
            var text = File.ReadAllText(file, enc);
            try
            {
                var tree = StateDgmReader.Read(text);
                var d = StateDgmConverter.FromTree(tree);
                states += d.Categories.Sum(c => c.States.Count);
                Assert.IsTrue(d.Categories.Count > 0, $"{file}: bez kategórií");
                Assert.IsTrue(d.Designs.Count > 0, $"{file}: bez vzhľadov");

                var written = d.ToText();
                var again = StateDgmDiagram.Parse(written);
                var dump1 = Dump(tree.Root);
                var dump2 = Dump(StateDgmReader.Read(written).Root);
                if (dump1 != dump2)
                {
                    File.WriteAllText(Path.Combine(Path.GetTempPath(), "statedgm_dump1.txt"), dump1);
                    File.WriteAllText(Path.Combine(Path.GetTempPath(), "statedgm_dump2.txt"), dump2);
                    problems.Add($"{file}: význam po zápise nesedí (dumpy v %TEMP%\\statedgm_dump*.txt)");
                }

                // druhy zapis musi byt totozny s prvym (kanonicky tvar je stabilny)
                Assert.AreEqual(written, again.ToText(), $"{file}: druhý zápis sa líši");
                Assert.AreEqual(0, again.Warnings.Count, $"{file}: po zápise ostali upozornenia: {string.Join("; ", again.Warnings)}");
            }
            catch (StateDgmParseException e)
            {
                problems.Add($"{file}({e.Line + 1}): {e.Message}");
            }
        }

        Assert.IsTrue(files > 50 && states > 1000, $"súborov {files}, stavov {states}");
        Assert.AreEqual(0, problems.Count, string.Join("\n", problems));
    }

    [TestMethod]
    public void LiveData_KosiceAndBardejovDetails()
    {
        var bj = Path.Combine(LiveRoot, @"INISSes\INISS 3.39\DATA\Bardejov.2018\StateDgm.TXT");
        var ke = Path.Combine(LiveRoot, @"INISSes\INISSBj\DATA\Košice.2025\statedgm.txt");
        if (!File.Exists(bj) || !File.Exists(ke))
            Assert.Inconclusive("chýbajú testovacie dáta");
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var d = StateDgmDiagram.Load(bj);
        Assert.AreEqual(31, d.Designs.Count);
        Assert.AreEqual(6, d.Categories.Count);
        Assert.AreEqual(0, d.TimePoints.Count);
        Assert.IsNull(d.IndCat);
        Assert.AreEqual(0, d.Warnings.Count, string.Join("; ", d.Warnings));
        var start = d.Categories[0].States[0];
        Assert.AreEqual("#Start", start.Key);
        Assert.AreEqual(StateDgmAttr.Stoji, start.Attr);
        Assert.AreEqual(2, start.AutoMode!.Number);
        Assert.AreEqual(9, start.Events.Count);
        Assert.AreEqual(9, start.Controls.Count);
        Assert.IsTrue(start.DoState!.OnDepartureTable);
        Assert.IsFalse(start.DoState.OnArrivalTable);
        Assert.AreEqual("SDDlgKolej", start.Events[0].Dialog);
        Assert.AreEqual("Přijíždí", start.Events[3].NextState);
        Assert.AreEqual(0, start.Extras.Count + d.HeaderExtras.Count + d.RootExtras.Count, "všetko známe");

        var k = StateDgmDiagram.Load(ke);
        var waits = k.Categories.SelectMany(c => c.States).Where(s => s.Wait != null).ToList();
        Assert.IsTrue(waits.Count >= 8);
        Assert.IsTrue(waits.All(s => s.Wait!.IsExpression));
        CollectionAssert.Contains(waits.Select(s => s.Wait!.Expression).ToList(), "OVC");
    }

    [TestMethod]
    public void LiveData_PetrzalkaUnnumberedGroups()
    {
        var pz = Path.Combine(LiveRoot, @"DATAs\Petrzalka2020-Data\Petrzalka.2020\STATEDGM.txt");
        if (!File.Exists(pz))
            Assert.Inconclusive("chýbajú testovacie dáta");
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var d = StateDgmDiagram.Load(pz);
        Assert.AreEqual(6, d.Categories.Count);
        Assert.AreEqual(4, d.TimePoints.Count, "G:\"TimePoint\" bez čísla a bez NumTimePoints");
        var start = d.Categories[0].States[0];
        Assert.AreEqual(11, start.Events.Count);
        Assert.AreEqual(1, start.Starters.Count);
        Assert.AreEqual(-360, start.Starters[0].TimeOffset);
        Assert.AreEqual(600, start.Starters[0].TimeOffsetStep);
        // G:"TimePoint" vnutri stavu je casovy bod stavu (#StartTime = cas vstupu do stavu)
        var withLocal = d.Categories.SelectMany(c => c.States).Where(s => s.TimePoints.Count > 0).ToList();
        Assert.IsTrue(withLocal.Count > 0);
        Assert.IsTrue(withLocal.All(s => s.TimePoints[0].Key == StateDgmKeys.START_TIME && s.Extras.Count == 0));
        var text = d.ToText();
        StringAssert.Contains(text, "G:\"TimePoint1\" {S:\"Key\"=\"#StartTime\"}");
    }

    /// <summary>
    ///     Vyznamovy vypis stromu: skupiny podla zakladneho mena v poradi, hodnoty zoradene podla kluca, bez klucov Num….
    /// </summary>
    private static string Dump(StateDgmGroup g)
    {
        var sb = new StringBuilder();
        Dump(g, sb, 0);
        return sb.ToString();
    }

    private static void Dump(StateDgmGroup g, StringBuilder sb, int depth)
    {
        var pad = new string(' ', depth * 2);
        var baseName = StateDgmGroup.BaseName(g.Name);
        sb.Append(pad).Append('[').Append(baseName).Append(']').Append('\n');
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var v in g.Values.Where(v => !Ignored.Contains(v.Key)))
        {
            var n = Normalize(v);
            if (n == null) continue;
            var eq = n.IndexOf('=');
            values[n[..eq]] = n[(eq + 1)..];
        }

        // predvolene hodnoty INISSu - zapisovac ich zapise vzdy, povodny subor ich mohol vynechat
        foreach (var (k, dv) in Defaults(baseName))
            values.TryAdd(k, dv);
        values.Remove(StateDgmKeys.NAME, out var name);
        if (!string.IsNullOrEmpty(name)) values[StateDgmKeys.NAME] = name;

        foreach (var (k, v) in values.OrderBy(x => x.Key, StringComparer.Ordinal))
            sb.Append(pad).Append("  ").Append(k).Append('=').Append(v).Append('\n');
        // poradie skupin rozneho druhu nie je vyznamove (nezname skupiny idu pri zapise na koniec),
        // poradie skupin rovnakeho druhu ano - stabilne triedenie podla zakladneho mena
        foreach (var sub in g.Groups.OrderBy(x => StateDgmGroup.BaseName(x.Name), StringComparer.Ordinal)) Dump(sub, sb, depth + 1);
    }

    private static IEnumerable<(string, string)> Defaults(string baseName)
    {
        switch (baseName)
        {
            case StateDgmKeys.CATEGORIE:
                yield return (StateDgmKeys.COMMENT, "");
                yield return (StateDgmKeys.ICON, "0");
                break;
            case StateDgmKeys.STATE:
                yield return (StateDgmKeys.ICON, "0");
                yield return (StateDgmKeys.ATTR, "0");
                yield return (StateDgmKeys.DEFAULT_CONTROL, "0");
                break;
            case StateDgmKeys.DO_STATE:
            case StateDgmKeys.UNDO_STATE:
                yield return (StateDgmKeys.CLASS, StateDgmKeys.CLASS_TABLE_SET);
                yield return (StateDgmKeys.ON_DEP_TABLE, "0");
                yield return (StateDgmKeys.ON_ARR_TABLE, "0");
                yield return (StateDgmKeys.ON_PLATFORM_TABLE, "0");
                yield return (StateDgmKeys.SHOW_POSITION, "0");
                yield return (StateDgmKeys.SHOW_TRACK, "1");
                break;
            case StateDgmKeys.DESIGN:
                yield return (StateDgmKeys.DEF_PUSH_BTN, "0");
                yield return (StateDgmKeys.CLASS, StateDgmKeys.CLASS_DESIGN_BTN);
                break;
            case StateDgmKeys.TIME_POINT:
                yield return (StateDgmKeys.TIME_POINT_KEY1, "");
                yield return (StateDgmKeys.TIME_POINT_KEY2, "");
                yield return (StateDgmKeys.TIME_POINT_OFFSET1, "0");
                yield return (StateDgmKeys.TIME_POINT_OFFSET2, "0");
                yield return (StateDgmKeys.OPERATOR, StateDgmKeys.OPERATOR_MIN);
                break;
            case StateDgmKeys.STARTER:
                yield return (StateDgmKeys.CLASS, StateDgmKeys.CLASS_STARTER);
                yield return (StateDgmKeys.TIME_OFFSET, "0");
                yield return (StateDgmKeys.START_LATER_TOO, "0");
                break;
            case StateDgmKeys.CONTROL:
                yield return (StateDgmKeys.CTRL_ID, "0");
                break;
        }
    }

    private static string? Normalize(StateDgmValue v)
    {
        if (v.IsRemoval) return $"{v.Key}=#";
        // ciselny WaitPath sa normalizuje na Wait=VVC
        if (v.Key == StateDgmKeys.WAIT_PATH && v.Kind == StateDgmValueKind.Int)
            return v.Number == 0 ? null : $"{StateDgmKeys.WAIT}=VVC";
        var key = v.Key switch
        {
            StateDgmKeys.ON_DEP_TABLE_OLD => StateDgmKeys.ON_DEP_TABLE,
            StateDgmKeys.ON_ARR_TABLE_OLD => StateDgmKeys.ON_ARR_TABLE,
            StateDgmKeys.ON_PLATFORM_TABLE_OLD => StateDgmKeys.ON_PLATFORM_TABLE,
            StateDgmKeys.SHOW_POSITION_OLD => StateDgmKeys.SHOW_POSITION,
            StateDgmKeys.SHOW_TRACK_OLD => StateDgmKeys.SHOW_TRACK,
            _ => v.Key
        };
        return v.Kind switch
        {
            StateDgmValueKind.String => $"{key}={v.Text}",
            StateDgmValueKind.Int => $"{key}={v.Number}",
            _ => $"{key}={(v.Flag ? 1 : 0)}"
        };
    }
}
