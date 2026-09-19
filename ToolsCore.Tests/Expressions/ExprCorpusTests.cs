using ToolsCore.Expressions;

namespace ToolsCore.Tests.Expressions;

/// <summary>
///     Vyrazy z realnych suborov INISSu - vsetko, co INISS nacita, sa musi prelozit.
/// </summary>
[TestClass]
public class ExprCorpusTests
{
    /// <summary>Kluce z TrTypes.txt, ktore realne grafikony pouzivaju (Typ_RJ, Typ_RR …).</summary>
    private static readonly TestSymbols Symbols = new()
    {
        TrainTypeKeys = AllTypes()
    };

    private static Dictionary<string, int> AllTypes()
    {
        var d = new Dictionary<string, int> { ["RJ"] = 39, ["RR"] = 40 };
        for (var i = 0; i < ExprTrainTypes.Count; i++)
            if (ExprTrainTypes.NameOf(i) is { } n)
                d[n] = i;
        return d;
    }

    [TestMethod]
    public void EmbeddedCorpus_Compiles()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "TestData", "expressions.txt");
        var failures = new List<string>();
        var count = 0;
        foreach (var line in File.ReadLines(path))
        {
            if (line.Length == 0 || line[0] == '#') continue;
            var tab = line.IndexOf('\t');
            var ctx = line[..tab] == "W" ? ExprContext.StateDgmWait : ExprContext.Condition;
            var expr = line[(tab + 1)..];
            count++;
            var r = ExprParser.Parse(expr, ctx, Symbols);
            if (!r.Success) failures.Add($"{expr}\n    -> {r.Error}");
        }

        Assert.IsTrue(count > 100, "korpus je podozrivo maly");
        Assert.AreEqual(0, failures.Count, string.Join("\n", failures));
    }

    [TestMethod]
    public void EmbeddedCorpus_ValidatorHasNoWarnings()
    {
        // realne subory su odladene - validator by na nich nemal hlasit varovania (info moze)
        var path = Path.Combine(AppContext.BaseDirectory, "TestData", "expressions.txt");
        var warnings = new List<string>();
        foreach (var line in File.ReadLines(path))
        {
            if (line.Length == 0 || line[0] == '#') continue;
            var tab = line.IndexOf('\t');
            var expr = line[(tab + 1)..];
            var r = ExprValidator.Validate(expr, new ExprValidationOptions
            {
                Context = line[..tab] == "W" ? ExprContext.StateDgmWait : ExprContext.Condition,
                Symbols = Symbols,
                IsCondition = line[..tab] != "W",
                ReportContextDependent = false
            });
            foreach (var d in r.Diagnostics.Where(d => d.Severity == ExprSeverity.Warning))
                warnings.Add($"{expr}\n    -> {d}");
        }

        Assert.AreEqual(0, warnings.Count, string.Join("\n", warnings));
    }
}
