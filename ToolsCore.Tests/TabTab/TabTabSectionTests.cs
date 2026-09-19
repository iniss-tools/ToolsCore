using System.Diagnostics.CodeAnalysis;
using ToolsCore.Expressions;
using ToolsCore.TabTab;
using ToolsCore.Tests.Expressions;

namespace ToolsCore.Tests.TabTab;

[TestClass]
[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
public class TabTabSectionTests
{
    [TestMethod]
    public void SimpleRules_LastEqualsIsSeparator()
    {
        var s = TabTabSection.Parse("R{81}=R\r\n{@}=-{@}\r\nSC{82}=SC{18}\r\n");
        var rules = s.Rules.ToList();
        Assert.AreEqual(3, rules.Count);
        Assert.AreEqual("R{81}", rules[0].Left);
        Assert.AreEqual("R", rules[0].Right);
        Assert.AreEqual(TabTabEventKind.None, rules[0].Event);
        Assert.AreEqual("-{@}", rules[1].Right);
        Assert.AreEqual(2, rules[2].LineIndex);
        Assert.AreEqual(0, rules[0].Span.Start);
        Assert.AreEqual(7, rules[0].Span.Length);
        Assert.AreEqual(6, rules[0].RightSpan.Start);
    }

    [TestMethod]
    public void Rule_WithDoubleEqualsInCondition_SplitsAtLastEquals()
    {
        var s = TabTabSection.Parse("(1==1), \"@\"{98} =#SWITCH");
        var r = s.Rules.Single();
        Assert.AreEqual("#SWITCH", r.Right);
        Assert.AreEqual(TabTabEventKind.Switch, r.Event);
        Assert.AreEqual(2, r.Items.Count);
        Assert.AreEqual("(1==1)", r.Items[0].Text);
        Assert.IsTrue(r.Items[0].IsCondition);
        Assert.AreEqual("\"@\"{98}", r.Items[1].Text);
        Assert.AreEqual("@", r.Items[1].Decoded!.Value.Text);
        Assert.AreEqual(98, r.Items[1].Decoded!.Value.Font);
    }

    [TestMethod]
    public void Continuation_JoinsLines_KeepsPositions()
    {
        const string text = "; komentar\r\n  Stav(Stav_Stoji), \"@\"{94}, \\\r\n  (Typ(Typ_R) || Typ(Typ_Ex)), \"@\"{89}, \\\r\n  1, \"@\"{90} = #SWITCH\r\n";
        var s = TabTabSection.Parse(text);
        Assert.AreEqual(2, s.Lines.Count);
        Assert.AreEqual(TabTabLineKind.Comment, s.Lines[0].Kind);
        var r = s.Lines[1];
        Assert.AreEqual(TabTabLineKind.Rule, r.Kind);
        Assert.AreEqual(1, r.LineIndex);
        Assert.AreEqual(6, r.Items.Count);
        Assert.AreEqual("(Typ(Typ_R) || Typ(Typ_Ex))", r.Items[2].Text);
        // pozicia polozky na druhom fyzickom riadku
        Assert.AreEqual(text.IndexOf("(Typ(Typ_R)", StringComparison.Ordinal), r.Items[2].Span.Start);
        Assert.AreEqual("1", r.Items[4].Text);
        Assert.AreEqual(text.LastIndexOf("1,", StringComparison.Ordinal), r.Items[4].Span.Start);
        Assert.AreEqual(text.IndexOf("#SWITCH", StringComparison.Ordinal), r.RightSpan.Start);
        Assert.AreEqual(text.Length - 2, r.Span.End);
    }

    [TestMethod]
    public void Comments_Options_Escapes()
    {
        var s = TabTabSection.Parse("IgnoreCase, Foo\r\nA=B ; inline komentar\r\na\\=b=c\r\n[X]\r\n[bad\r\n\r\n");
        Assert.AreEqual(TabTabLineKind.Options, s.Lines[0].Kind);
        CollectionAssert.AreEqual(new[] { "IgnoreCase", "Foo" }, s.Lines[0].Options.ToArray());
        Assert.IsTrue(s.IgnoreCase);
        Assert.AreEqual("B", s.Lines[1].Right);
        Assert.AreEqual("A", s.Lines[1].Left);
        Assert.AreEqual("a\\=b", s.Lines[2].Left);
        Assert.AreEqual("c", s.Lines[2].Right);
        Assert.AreEqual(TabTabLineKind.SectionHeader, s.Lines[3].Kind);
        Assert.AreEqual("X", s.Lines[3].Text);
        Assert.AreEqual(TabTabLineKind.BadSectionHeader, s.Lines[4].Kind);
        Assert.AreEqual(TabTabLineKind.Empty, s.Lines[5].Kind);
        Assert.AreEqual(6, s.Lines.Count);
    }

    [TestMethod]
    public void Events_CaseSensitive_And_PozOdj()
    {
        var s = TabTabSection.Parse("Autobus =#VYLUKA\r\nx=#switch\r\nB2 = #POZODJ_3\r\nOdklon, \"ODKLON\" = #SWITCH\r\n(1),\"\",\"a\" = #MERGE2");
        var r = s.Rules.ToList();
        Assert.AreEqual(TabTabEventKind.Vyluka, r[0].Event);
        Assert.AreEqual(TabTabEventKind.Unknown, r[1].Event);
        Assert.AreEqual(TabTabEventKind.PozOdj, r[2].Event);
        Assert.AreEqual("3", r[2].PozOdjTrack);
        Assert.AreEqual(TabTabEventKind.Switch, r[3].Event);
        Assert.AreEqual(TabTabEventKind.Merge2, r[4].Event);
        Assert.AreEqual(3, r[4].Items.Count);
        Assert.IsTrue(r[4].Items[1].IsSeparator);
    }

    [TestMethod]
    public void Decode_Text()
    {
        Assert.AreEqual(new TabTabText("@", 146), TabTabText.Decode("\"@\"{146}"));
        Assert.AreEqual(new TabTabText("-", TabTabText.DefaultFont), TabTabText.Decode("-{@}"));
        Assert.AreEqual(new TabTabText("R", 81), TabTabText.Decode("  R{81}  "));
        Assert.AreEqual(new TabTabText("Mešká#@ min.", null), TabTabText.Decode("\"Mešká#@ min.\""));
        Assert.AreEqual(new TabTabText("a \"b\"", null), TabTabText.Decode("\"a \"\"b\"\"\""));
        Assert.AreEqual(new TabTabText("x=y", null), TabTabText.Decode("x\\=y"));
        Assert.AreEqual(new TabTabText("a{1}b", null), TabTabText.Decode("a{1}b"));
        Assert.AreEqual(new TabTabText("trail", null), TabTabText.Decode("trail   "));
        Assert.AreEqual(new TabTabText("in quotes  ", null), TabTabText.Decode("\"in quotes  \""));
        Assert.AreEqual(new TabTabText("", 34369), TabTabText.Decode("{34369}"));
    }

    [TestMethod]
    public void Validator_InissErrors()
    {
        var symbols = new TestSymbols { Tracks = ["1", "2"] };
        var opts = new TabTabValidationOptions { Symbols = symbols, ColumnNames = ["Linka", "Smer"] };

        var r = TabTabValidator.Validate("Typ(Typ_R), \"a\", 1 = #SWITCH", opts);
        Assert.AreEqual(1, r.ErrorCount);
        Assert.AreEqual(TabTabDiagnosticCode.InissItemsCountUneven, r.Diagnostics[0].Code);

        r = TabTabValidator.Validate("Typ(Typ_R), \"a\" = #MERGE", opts);
        Assert.AreEqual(TabTabDiagnosticCode.InissItemsCountNotMultipleOf3, r.Diagnostics.Single(d => d.IsError).Code);

        r = TabTabValidator.Validate("x = #FOO", opts);
        Assert.AreEqual("unknown magic item #FOO", r.Diagnostics.Single().Message);

        r = TabTabValidator.Validate("x = #switch", opts);
        Assert.AreEqual(TabTabDiagnosticCode.EventCase, r.Diagnostics.Single().Code);

        r = TabTabValidator.Validate("B2 = #POZODJ_9", opts);
        Assert.AreEqual(TabTabDiagnosticCode.InissUnknownTrack, r.Diagnostics.Single().Code);
        Assert.AreEqual(0, TabTabValidator.Validate("B2 = #POZODJ_1", opts).Diagnostics.Count);

        r = TabTabValidator.Validate("Typ(Typ_R) && Foo, \"a\", \\\r\n Typ(Typ_Os, \"b\" = #SWITCH", opts);
        Assert.AreEqual(2, r.ErrorCount);
        Assert.AreEqual("#SWITCH/1: Neznámy symbol Foo", r.Diagnostics[0].Message);
        Assert.AreEqual(14, r.Diagnostics[0].Start);
        Assert.AreEqual(3, r.Diagnostics[0].Length);
        Assert.AreEqual("#SWITCH/3: Očakávam )", r.Diagnostics[1].Message);
        Assert.AreEqual(1, r.Diagnostics[1].LineIndex);
    }

    [TestMethod]
    public void Validator_Warnings()
    {
        var opts = new TabTabValidationOptions { ColumnNames = ["Linka"] };

        var r = TabTabValidator.Validate("1, \"a\", Typ(Typ_R), \"b\" = #SWITCH", opts);
        Assert.AreEqual(TabTabDiagnosticCode.UnreachableItem, r.Diagnostics.Single().Code);
        Assert.AreEqual(ExprSeverity.Warning, r.Diagnostics.Single().Severity);

        r = TabTabValidator.Validate("Typ(Typ_R), %Linka%, 1, %Smer% = #SWITCH", opts);
        Assert.AreEqual(TabTabDiagnosticCode.UnknownColumn, r.Diagnostics.Single().Code);
        StringAssert.Contains(r.Diagnostics.Single().Message, "Smer");

        r = TabTabValidator.Validate("Typ(Typ_R) && ODKLON || MISTNI, \"a\" = #SWITCH", opts);
        Assert.AreEqual(TabTabDiagnosticCode.Condition, r.Diagnostics.Single().Code);
        Assert.AreEqual(ExprDiagnosticCode.MixedLogicalOperators, r.Diagnostics.Single().ExprCode);

        r = TabTabValidator.Validate("[Druha]\r\n[zla\r\nfoo\r\nx=", opts);
        CollectionAssert.AreEqual(
            new[] { TabTabDiagnosticCode.SectionHeaderInside, TabTabDiagnosticCode.BadSectionHeader, TabTabDiagnosticCode.UnknownOption, TabTabDiagnosticCode.EmptyRight },
            r.Diagnostics.Select(d => d.Code).ToArray());

        Assert.AreEqual(0, TabTabValidator.Validate("R{81}=R\r\n; x\r\n\r\nOdklon, \"ODKLON\" = #SWITCH\r\n1,\"-\" =#SWITCH", opts).Diagnostics.Count);
    }

    [TestMethod]
    public void Validator_Fixes_InSectionCoordinates()
    {
        const string text = "R{81}=R\r\nTyp(Typ_R) && ODKLON || MISTNI, \"a\", \\\r\n  PRIZNAK(Prizn_M) == 1, \"b\" = #SWITCH";
        var r = TabTabValidator.Validate(text);
        var fixes = r.Diagnostics.Where(d => d.Fix is not null).ToList();
        Assert.AreEqual(2, fixes.Count, string.Join("; ", r.Diagnostics));

        var t = text;
        foreach (var f in fixes.OrderByDescending(d => d.Start))
            t = f.Fix!.Apply(t);
        Assert.AreEqual("R{81}=R\r\n(Typ(Typ_R) && ODKLON) || MISTNI, \"a\", \\\r\n  PRIZNAK(Prizn_M), \"b\" = #SWITCH", t);

        var again = TabTabValidator.Validate(t);
        Assert.AreEqual(0, again.Diagnostics.Count, string.Join("; ", again.Diagnostics));
        Assert.IsTrue(r.Diagnostics.All(d => d.Suggestion is not null || d.Severity == ExprSeverity.Info));

        // udalost malymi pismenami: INISS polozky vobec necita, oprava prepise udalost
        const string lower = "1, \"a\" = #switch";
        var e = TabTabValidator.Validate(lower).Diagnostics.Single();
        Assert.AreEqual(TabTabDiagnosticCode.EventCase, e.Code);
        Assert.AreEqual("1, \"a\" = #SWITCH", e.Fix!.Apply(lower));
    }
}
