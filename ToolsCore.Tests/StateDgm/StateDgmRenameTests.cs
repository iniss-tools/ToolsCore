using System.Diagnostics.CodeAnalysis;
using System.Text;
using ToolsCore.StateDgm;

namespace ToolsCore.Tests.StateDgm;

/// <summary>
///     Prenos premenovaneho kluca do odkazov (stav, vzhlad, casovy bod, akcia) a odmietnutie nejednoznacnych premenovani.
/// </summary>
[TestClass]
[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
public class StateDgmRenameTests
{
    /// <summary>Dve kategorie so stavmi rovnakych klucov, vzhlad, bod hlavicky a bod stavu.</summary>
    private static StateDgmDiagram Sample()
    {
        var d = new StateDgmDiagram();
        d.Designs.Add(new StateDgmDesign { Key = "OdídeN" });
        d.Designs.Add(new StateDgmDesign { Key = "KoľajN" });
        d.TimePoints.Add(new StateDgmTimePoint { Key = "#Bod", TimePointKey1 = StateDgmKeys.BuiltInTimePoints[1], TimePointKey2 = StateDgmKeys.BuiltInTimePoints[3] });
        d.TimePoints.Add(new StateDgmTimePoint { Key = "#Bod2", TimePointKey1 = "#Bod", TimePointKey2 = StateDgmKeys.BuiltInTimePoints[3] });
        for (var k = 0; k < 2; k++)
        {
            var cat = new StateDgmCategory { Key = "#K" + k };
            foreach (var key in new[] { "#Start", "Vypis", "Odisiel" })
            {
                var s = new StateDgmState { Key = key };
                s.Events.Add(new StateDgmEvent { Key = "#GoVypis", NextState = "Vypis" });
                s.Events.Add(new StateDgmEvent { Key = "#GoOdisiel", NextState = "Odisiel" });
                s.Controls.Add(new StateDgmControl { CtrlId = 0, DesignKey = "KoľajN", EventKey = "#GoVypis" });
                s.Controls.Add(new StateDgmControl { CtrlId = 1, DesignKey = "OdídeN", EventKey = "#GoOdisiel" });
                s.Starters.Add(new StateDgmStarter { Key = "S", EventKey = "#GoOdisiel", TimePointKey = "#Bod", TimePointKeyLast = "#Bod2" });
                cat.States.Add(s);
            }

            d.Categories.Add(cat);
        }

        var v = d.Categories[0].States[1];
        v.TimePoints.Add(new StateDgmTimePoint { Key = StateDgmKeys.START_TIME });
        v.TimePoints.Add(new StateDgmTimePoint { Key = "#Lok", TimePointKey1 = StateDgmKeys.START_TIME, TimePointKey2 = "#Bod" });
        v.Starters.Add(new StateDgmStarter { Key = "S2", EventKey = "#GoVypis", TimePointKey = StateDgmKeys.START_TIME, TimePointKeyLast = "#Lok" });
        return d;
    }

    private static readonly StateDgmDiagnosticCode[] ReferenceCodes =
    [
        StateDgmDiagnosticCode.NextStateMissing, StateDgmDiagnosticCode.EventKeyMissing, StateDgmDiagnosticCode.DesignKeyMissing,
        StateDgmDiagnosticCode.TimePointKeyMissing, StateDgmDiagnosticCode.DuplicateKey
    ];

    /// <summary>Ziadny neplatny odkaz ani duplicitny kluc.</summary>
    private static bool NoErrors(StateDgmDiagram d, out string text)
    {
        var errors = StateDgmValidator.Validate(d, new StateDgmValidationOptions())
            .Where(x => ReferenceCodes.Contains(x.Code)).ToList();
        text = string.Join("\n", errors);
        return errors.Count == 0;
    }

    [TestMethod]
    public void Rename_SampleIsClean()
    {
        Assert.IsTrue(NoErrors(Sample(), out var t), t);
    }

    [TestMethod]
    public void Rename_StateUpdatesNextStateInCategoryOnly()
    {
        var d = Sample();
        var s = d.Categories[0].States[1];
        s.Key = "Vypísaný";
        Assert.IsTrue(StateDgmRename.TryRename(d, s, "Vypis", out var n));
        Assert.AreEqual(3, n);
        Assert.IsTrue(d.Categories[0].States.All(x => x.FindEvent("#GoVypis")!.NextState == "Vypísaný"));
        Assert.IsTrue(d.Categories[1].States.All(x => x.FindEvent("#GoVypis")!.NextState == "Vypis"), "iná kategória má vlastný stav Vypis");
        Assert.IsTrue(NoErrors(d, out var t), t);
    }

    [TestMethod]
    public void Rename_StateTypedStepByStep()
    {
        // editor prenasa pri kazdom znaku; medzikrok "Odisiel" je kluc ineho stavu → odkazy cakaju na dalsi znak
        var d = Sample();
        var s = d.Categories[0].States[1];
        var refKey = s.Key;
        foreach (var typed in new[] { "", "O", "Od", "Odisiel", "Odisiel2" })
        {
            s.Key = typed;
            if (StateDgmRename.TryRename(d, s, refKey, out _)) refKey = typed;
        }

        Assert.AreEqual("Odisiel2", refKey);
        Assert.IsTrue(d.Categories[0].States.All(x => x.FindEvent("#GoVypis")!.NextState == "Odisiel2"));
        Assert.IsTrue(d.Categories[0].States.All(x => x.FindEvent("#GoOdisiel")!.NextState == "Odisiel"), "cudzie odkazy sa nezlúčili");
        Assert.IsTrue(NoErrors(d, out var t), t);
    }

    [TestMethod]
    public void Rename_RefusesAmbiguous()
    {
        var d = Sample();
        var s = d.Categories[0].States[1];

        s.Key = "Odisiel"; // kluc ineho stavu
        Assert.IsFalse(StateDgmRename.TryRename(d, s, "Vypis", out _));
        s.Key = "";
        Assert.IsFalse(StateDgmRename.TryRename(d, s, "Vypis", out _));
        Assert.IsTrue(d.Categories[0].States.All(x => x.FindEvent("#GoVypis")!.NextState == "Vypis"));

        // na novy kluc sa uz nieco odkazuje (neexistujuci stav)
        d.Categories[0].States[0].Events.Add(new StateDgmEvent { Key = "#Zly", NextState = "Nikde" });
        s.Key = "Nikde";
        Assert.IsFalse(StateDgmRename.TryRename(d, s, "Vypis", out _));

        // stary kluc ma aj iny stav - odkazy patria aj jemu
        d.Categories[0].States[2].Key = "Vypis";
        s.Key = "Iny";
        Assert.IsFalse(StateDgmRename.TryRename(d, s, "Vypis", out _));
        Assert.IsTrue(d.Categories[0].States.All(x => x.FindEvent("#GoVypis")!.NextState == "Vypis"));

        // prvok, ktory v diagrame nie je
        Assert.IsFalse(StateDgmRename.TryRename(d, new StateDgmState { Key = "X" }, "Vypis", out _));
    }

    [TestMethod]
    public void Rename_DesignUpdatesAllControls()
    {
        var d = Sample();
        var ds = d.Designs[1];
        ds.Key = "KolajN";
        Assert.IsTrue(StateDgmRename.TryRename(d, ds, "KoľajN", out var n));
        Assert.AreEqual(6, n);
        Assert.IsTrue(d.Categories.SelectMany(c => c.States).All(s => s.Controls[0].DesignKey == "KolajN"));
        Assert.IsTrue(NoErrors(d, out var t), t);

        ds.Key = "OdídeN";
        Assert.IsFalse(StateDgmRename.TryRename(d, ds, "KolajN", out _));
        Assert.IsTrue(d.Categories.SelectMany(c => c.States).All(s => s.Controls[0].DesignKey == "KolajN"));
    }

    [TestMethod]
    public void Rename_HeaderTimePointUpdatesWholeDiagram()
    {
        var d = Sample();
        var tp = d.TimePoints[0];
        tp.Key = "#Hlavný";
        Assert.IsTrue(StateDgmRename.TryRename(d, tp, "#Bod", out var n));
        // #Bod2 + #Lok v stave + 6 starterov
        Assert.AreEqual(8, n);
        Assert.AreEqual("#Hlavný", d.TimePoints[1].TimePointKey1);
        Assert.AreEqual("#Hlavný", d.Categories[0].States[1].TimePoints[1].TimePointKey2);
        Assert.IsTrue(d.Categories.SelectMany(c => c.States).All(s => s.Starters[0].TimePointKey == "#Hlavný"));
        Assert.IsTrue(NoErrors(d, out var t), t);

        // zabudovany kluc a kluc bodu stavu su obsadene
        tp.Key = StateDgmKeys.BuiltInTimePoints[0];
        Assert.IsFalse(StateDgmRename.TryRename(d, tp, "#Hlavný", out _));
        tp.Key = "#Lok";
        Assert.IsFalse(StateDgmRename.TryRename(d, tp, "#Hlavný", out _));
    }

    [TestMethod]
    public void Rename_HeaderTimePointSkipsShadowingState()
    {
        var d = Sample();
        var own = d.Categories[1].States[0];
        own.TimePoints.Add(new StateDgmTimePoint { Key = "#Bod", TimePointKey1 = StateDgmKeys.BuiltInTimePoints[0], TimePointKey2 = StateDgmKeys.BuiltInTimePoints[1] });
        var tp = d.TimePoints[0];
        tp.Key = "#Novy";
        Assert.IsTrue(StateDgmRename.TryRename(d, tp, "#Bod", out _));
        Assert.AreEqual("#Bod", own.Starters[0].TimePointKey, "stav s vlastným #Bod sa odkazuje naň");
        Assert.AreEqual("#Novy", d.Categories[1].States[1].Starters[0].TimePointKey);
    }

    [TestMethod]
    public void Rename_StateTimePointUpdatesOwnStateOnly()
    {
        var d = Sample();
        var v = d.Categories[0].States[1];
        var tp = v.TimePoints[1];
        tp.Key = "#Lokálny";
        Assert.IsTrue(StateDgmRename.TryRename(d, tp, "#Lok", out var n));
        Assert.AreEqual(1, n);
        Assert.AreEqual("#Lokálny", v.Starters[1].TimePointKeyLast);
        Assert.IsTrue(NoErrors(d, out var t), t);

        // bod stavu nesmie dostat kluc bodu hlavicky (zakryl by ho)
        tp.Key = "#Bod2";
        Assert.IsFalse(StateDgmRename.TryRename(d, tp, "#Lokálny", out _));
        Assert.AreEqual("#Lokálny", v.Starters[1].TimePointKeyLast);

        var start = v.TimePoints[0];
        start.Key = "#Vstup";
        Assert.IsTrue(StateDgmRename.TryRename(d, start, StateDgmKeys.START_TIME, out n));
        Assert.AreEqual(2, n);
        Assert.AreEqual("#Vstup", v.TimePoints[1].TimePointKey1);
        Assert.AreEqual("#Vstup", v.Starters[1].TimePointKey);
    }

    [TestMethod]
    public void Rename_EventUpdatesControlsAndStarters()
    {
        var d = Sample();
        var v = d.Categories[0].States[1];
        var ev = v.FindEvent("#GoOdisiel")!;
        v.Controls.Add(new StateDgmControl { CtrlId = 2, DesignKey = "KoľajN", EventKey = "#GoOdisiel" });
        ev.Key = "#Odchod";
        Assert.IsTrue(StateDgmRename.TryRename(d, ev, "#GoOdisiel", out var n));
        Assert.AreEqual(3, n);
        Assert.AreEqual("#Odchod", v.Controls[1].EventKey);
        Assert.AreEqual("#Odchod", v.Controls[2].EventKey);
        Assert.AreEqual("#Odchod", v.Starters[0].EventKey);
        Assert.AreEqual("#GoOdisiel", d.Categories[0].States[0].Controls[1].EventKey, "iný stav sa nemení");
        Assert.IsTrue(NoErrors(d, out var t), t);

        ev.Key = "#GoVypis";
        Assert.IsFalse(StateDgmRename.TryRename(d, ev, "#Odchod", out _));
    }

    [TestMethod]
    public void Rename_SlovakTemplateStaysClean()
    {
        var dir = ResourcesDir();
        if (dir == null) Assert.Inconclusive("priecinok GVDEditor/Resources sa nenasiel");
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var d = StateDgmDiagram.Load(Path.Combine(dir, "statedgm.txt"));
        Assert.IsTrue(NoErrors(d, out var t0), t0);

        foreach (var cat in d.Categories)
            for (var i = 1; i < cat.States.Count; i++)
            {
                var s = cat.States[i];
                var old = s.Key;
                s.Key = old + "_x";
                Assert.IsTrue(StateDgmRename.TryRename(d, s, old, out _), old);
            }

        foreach (var ds in d.Designs)
        {
            var old = ds.Key;
            ds.Key = old + "_x";
            Assert.IsTrue(StateDgmRename.TryRename(d, ds, old, out _), old);
        }

        foreach (var s in d.Categories.SelectMany(c => c.States))
        foreach (var ev in s.Events)
        {
            var old = ev.Key;
            ev.Key = old + "_x";
            Assert.IsTrue(StateDgmRename.TryRename(d, ev, old, out _), old);
        }

        Assert.IsTrue(NoErrors(d, out var t), t);
    }

    private static string? ResourcesDir()
    {
        var dir = AppContext.BaseDirectory;
        for (var i = 0; i < 8 && dir != null; i++)
        {
            var candidate = Path.Combine(dir, "GVDEditor", "GVDEditor", "Resources");
            if (File.Exists(Path.Combine(candidate, "statedgm.txt"))) return candidate;
            dir = Path.GetDirectoryName(dir);
        }

        return null;
    }
}
