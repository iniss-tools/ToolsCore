using System.Diagnostics.CodeAnalysis;
using System.Text;
using ToolsCore.StateDgm;

namespace ToolsCore.Tests.StateDgm;

/// <summary>
///     Predlohy StateDgm.txt, ktore GVDEditor zapisuje do noveho grafikonu (SK, CZ, SK s automatikou ILTIS).
/// </summary>
[TestClass]
[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
public class StateDgmTemplateTests
{
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

    [TestMethod]
    public void Templates_LoadCleanAndConsistent()
    {
        var dir = ResourcesDir();
        if (dir == null) Assert.Inconclusive("priecinok GVDEditor/Resources sa nenasiel");
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var sk = StateDgmDiagram.Load(Path.Combine(dir, "statedgm.txt"));
        var cz = StateDgmDiagram.Load(Path.Combine(dir, "statedgmCZ.txt"));
        var iltis = StateDgmDiagram.Load(Path.Combine(dir, "statedgmILTIS.txt"));

        foreach (var (name, d) in new[] { ("SK", sk), ("CZ", cz), ("ILTIS", iltis) })
        {
            Assert.AreEqual(0, d.Warnings.Count, $"{name}: {string.Join("; ", d.Warnings)}");
            Assert.AreEqual(6, d.Categories.Count, name);
            Assert.AreEqual(31, d.Designs.Count, name);
            var diags = StateDgmValidator.Validate(d, new StateDgmValidationOptions { ReportKeys = ["Přijíždí", "Vjíždí", "Zastavil", "Pobytové", "Odjede"] });
            Assert.IsFalse(diags.Any(x => x.IsError), $"{name}: {string.Join("\n", diags.Where(x => x.IsError))}");
        }

        // CZ = SK s ceskymi nazvami pre obsluhu; struktura rovnaka
        CollectionAssert.AreEqual(sk.Categories.Select(c => c.States.Count).ToList(), cz.Categories.Select(c => c.States.Count).ToList());
        Assert.AreEqual("Vypiš", cz.Categories[0].States[1].Name);
        Assert.AreEqual("Vypíš", sk.Categories[0].States[1].Name);
        Assert.AreEqual("ZpožděníN", cz.Designs[0].Key);
        Assert.IsTrue(cz.Categories.SelectMany(c => c.States).SelectMany(s => s.Controls).All(c => cz.FindDesign(c.DesignKey) != null));

        // ILTIS = SK + automatika riadena udalostami
        CollectionAssert.AreEqual(sk.Categories.Select(c => c.States.Count).ToList(), iltis.Categories.Select(c => c.States.Count).ToList());
        var waits = iltis.Categories.SelectMany(c => c.States).Where(s => s.Wait != null).Select(s => s.Wait!.Expression).ToList();
        Assert.AreEqual(8, waits.Count);
        CollectionAssert.AreEquivalent(new[] { "OVC", "Odj", "VVC", "Vj", "OVC", "VVC", "Vj", "Odj" }, waits);
        Assert.AreEqual(15, iltis.Categories.SelectMany(c => c.States).Count(s => s.HasAutomation));
        Assert.IsTrue(iltis.Categories.SelectMany(c => c.States).Where(s => s.HasAutomation).All(s => s.AutoMode!.Number == 2 && s.AutoTimePoint != null));
    }
}
