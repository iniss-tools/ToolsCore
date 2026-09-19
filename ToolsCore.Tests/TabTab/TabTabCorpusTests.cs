using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using ToolsCore.Expressions;
using ToolsCore.TabTab;
using ToolsCore.Tests.Expressions;

namespace ToolsCore.Tests.TabTab;

/// <summary>
///     Realne subory TabTab.txt - INISS ich nacita bez chyb v logu, takze ani validator nesmie hlasit chyby.
/// </summary>
[TestClass]
[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
public class TabTabCorpusTests
{
    private const string LiveRoot = @"D:\INISSroot";

    private static readonly Regex Header = new(@"^\s*\[([^\]]*)\]", RegexOptions.Compiled);

    [TestMethod]
    public void LiveData_SectionsHaveNoInissErrors()
    {
        if (!Directory.Exists(LiveRoot))
            Assert.Inconclusive($"{LiveRoot} nie je dostupny");

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var enc = Encoding.GetEncoding(1250);

        var symbols = new TestSymbols { TrainTypeKeys = AllTypes() };
        var opts = new TabTabValidationOptions { Symbols = symbols };

        var errors = new List<string>();
        var sections = 0;
        var rules = 0;

        foreach (var file in Directory.EnumerateFiles(LiveRoot, "tabtab.txt", new EnumerationOptions { RecurseSubdirectories = true, MatchCasing = MatchCasing.CaseInsensitive }))
        {
            foreach (var (name, text) in SplitSections(File.ReadAllText(file, enc)))
            {
                sections++;
                var r = TabTabValidator.Validate(text, opts);
                rules += r.Section.Rules.Count();
                foreach (var d in r.Diagnostics.Where(d => d.IsError))
                    errors.Add($"{file} [{name}] {d}");
            }
        }

        Assert.IsTrue(sections > 100 && rules > 1000, $"sekcii {sections}, pravidiel {rules}");
        Assert.AreEqual(0, errors.Count, string.Join("\n", errors));
    }

    private static Dictionary<string, int> AllTypes()
    {
        var d = new Dictionary<string, int> { ["RJ"] = 39, ["RR"] = 40 };
        for (var i = 0; i < ExprTrainTypes.Count; i++)
            if (ExprTrainTypes.NameOf(i) is { } n)
                d[n] = i;
        return d;
    }

    /// <summary>
    ///     Rozdeli subor na sekcie tak, ako ich drzi GVDEditor (text medzi hlavickami).
    /// </summary>
    private static IEnumerable<(string Name, string Text)> SplitSections(string file)
    {
        string? name = null;
        var sb = new StringBuilder();
        foreach (var line in file.Split('\n'))
        {
            var m = Header.Match(line);
            if (m.Success)
            {
                if (name is not null) yield return (name, sb.ToString());
                name = m.Groups[1].Value;
                sb.Clear();
            }
            else if (name is not null)
            {
                sb.Append(line).Append('\n');
            }
        }
        if (name is not null) yield return (name, sb.ToString());
    }
}
