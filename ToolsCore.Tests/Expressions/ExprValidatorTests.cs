using System.Diagnostics.CodeAnalysis;
using ToolsCore.Expressions;

namespace ToolsCore.Tests.Expressions;

[TestClass]
[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
public class ExprValidatorTests
{
    private static readonly TestSymbols Symbols = new()
    {
        TrainTypeKeys = new Dictionary<string, int> { ["Os"] = 0, ["R"] = 26, ["RJ"] = 39 },
        Stations = [5614616, 8000284],
        Tracks = ["1", "2", "SM-B"],
        Operators = ["České dráhy, a.s.", "RegioJet a.s."]
    };

    private static IReadOnlyList<ExprDiagnostic> Check(string text, bool withSymbols = true, bool isCondition = false)
    {
        var r = ExprValidator.Validate(text, new ExprValidationOptions
        {
            Symbols = withSymbols ? Symbols : null,
            IsCondition = isCondition,
            ReportContextDependent = false
        });
        Assert.IsTrue(r.Compiles, $"'{text}': {r.Parse.Error}");
        return r.Diagnostics;
    }

    private static ExprDiagnostic Single(string text, ExprDiagnosticCode code, bool withSymbols = true)
    {
        var d = Check(text, withSymbols);
        Assert.HasCount(1, d, $"'{text}': {string.Join("; ", d)}");
        Assert.AreEqual(code, d[0].Code, d[0].Message);
        return d[0];
    }

    [TestMethod]
    public void Clean_Expressions_HaveNoDiagnostics()
    {
        Assert.IsEmpty(Check("Typ(Typ_R) || Typ(Typ_Os)"));
        Assert.IsEmpty(Check("1"));
        Assert.IsEmpty(Check("(Typ(Typ_R) && ZPOZDENIPRIJ > 5) || ODKLON"));
        Assert.IsEmpty(Check("Typ(Typ_R) && Typ(Typ_Os) && ODKLON")); // rovnake spojky - poradie nevadi
        Assert.IsEmpty(Check("PRIZNAK(Prizn_M) != 0"));
        Assert.IsEmpty(Check("KOLEJODJ(\"SM-B\") || KOLEJODJ(\"\")"));
        Assert.IsEmpty(Check("ZESMERU(8000284) && OPERATOR(\"RegioJet a.s.\")"));
        Assert.IsEmpty(Check("1 + 2 + 3 * 4 * 5")); // + a * su asociativne - poradie nevadi
        Assert.IsEmpty(Check("ZPOZDENIPRIJ * 60 + 30"));
    }

    [TestMethod]
    public void Arithmetic_OrderSensitive()
    {
        Single("10 - 3 - 2", ExprDiagnosticCode.RightAssociativeArithmetic);
        Single("10 - 3 + 2 * 4", ExprDiagnosticCode.RightAssociativeArithmetic); // 10 - (3 + 8)
        Single("8 / 4 / 2", ExprDiagnosticCode.RightAssociativeArithmetic);
        Single("8 / 4 * 2", ExprDiagnosticCode.RightAssociativeArithmetic);      // 8 / (4 * 2)
        Single("8 * 4 / 2", ExprDiagnosticCode.RightAssociativeArithmetic);      // 8 * (4 / 2) - pri celych cislach zalezi
        Assert.IsEmpty(Check("(10 - 3) - 2"));
    }

    [TestMethod]
    public void Mixed_Connectives()
    {
        var d = Single("Typ(Typ_R) && ZPOZDENIPRIJ > 5 || ODKLON", ExprDiagnosticCode.MixedLogicalOperators);
        Assert.Contains("Typ(Typ_R) && (ZPOZDENIPRIJ > 5 || ODKLON)", d.Message);
        Assert.AreEqual(ExprSeverity.Warning, d.Severity);
        Single("1 AND 2 OR 3", ExprDiagnosticCode.MixedLogicalOperators);
        Single("1 & 2 && 3", ExprDiagnosticCode.MixedLogicalOperators);
        Assert.IsEmpty(Check("1 && 2 AND 3"));
        Assert.IsEmpty(Check("1 || (2 && 3)"));
    }

    [TestMethod]
    public void Mask_Compared_To_Number()
    {
        Single("PRIZNAK(Prizn_M) == 1", ExprDiagnosticCode.MaskComparedToNumber);
        Single("1 == FLAG(Flag_Lockout)", ExprDiagnosticCode.MaskComparedToNumber);
        Assert.IsEmpty(Check("PRIZNAK(Prizn_M) == 0"));
        Assert.IsEmpty(Check("STAV(Stav_Stoji) == 1"));
    }

    [TestMethod]
    public void Negation_And_Sign()
    {
        var d = Single("!ZPOZDENIPRIJ > 5", ExprDiagnosticCode.NegationOfComparison);
        Assert.AreEqual(ExprSeverity.Info, d.Severity);
        Single("-ZPOZDENIPRIJ + 5", ExprDiagnosticCode.SignOfSum);
        Assert.IsEmpty(Check("!(ZPOZDENIPRIJ > 5)"));
        Assert.IsEmpty(Check("-5"));
    }

    [TestMethod]
    public void Octal_And_DivisionByZero()
    {
        var d = Single("ZPOZDENIPRIJ > 010", ExprDiagnosticCode.OctalLiteral);
        Assert.Contains("010 = 8", d.Message);
        Assert.IsEmpty(Check("ZPOZDENIPRIJ > 0x10"));
        Assert.IsEmpty(Check("ZPOZDENIPRIJ > 0"));
        Single("CVLAKU / 0", ExprDiagnosticCode.DivisionByZero);
        Single("CVLAKU % (1 - 1)", ExprDiagnosticCode.DivisionByZero);
    }

    [TestMethod]
    public void TrainTypes_Against_TrTypes()
    {
        var d = Single("Typ(Typ_Ex)", ExprDiagnosticCode.TrainTypeNotInTrTypes);
        Assert.Contains("Ex", d.Message);
        Assert.IsEmpty(Check("Typ(Typ_RJ)"));
        Assert.IsEmpty(Check("Typ(Typ_Ex)", withSymbols: false));
        var i = Single("Typ(Typ_ex)", ExprDiagnosticCode.TrainTypeInexactMatch, withSymbols: false);
        Assert.AreEqual(ExprSeverity.Info, i.Severity);
    }

    [TestMethod]
    public void Unknown_Station_Track_Operator()
    {
        var d = Single("ZESMERU(1234567)", ExprDiagnosticCode.UnknownStation);
        Assert.AreEqual(8, d.Start);
        Assert.AreEqual(7, d.Length);
        Single("CILSTANICE(1)", ExprDiagnosticCode.UnknownStation);
        Assert.IsEmpty(Check("CILSTANICE(1 + 0)")); // vyraz sa nekontroluje
        Single("KOLEJPRIJ(\"3\")", ExprDiagnosticCode.UnknownTrack);
        Single("OPERATOR(\"RegioJet\")", ExprDiagnosticCode.UnknownOperator);
        Assert.IsEmpty(Check("ZESMERU(1234567)", withSymbols: false));
    }

    [TestMethod]
    public void Constant_Condition()
    {
        Assert.IsEmpty(Check("1", isCondition: true));
        var d = Check("(1==1)", isCondition: true);
        Assert.HasCount(1, d);
        Assert.AreEqual(ExprDiagnosticCode.ConstantCondition, d[0].Code);
        Assert.AreEqual("Podmienka je vždy splnená", d[0].Message);
        Assert.AreEqual("Podmienka nie je nikdy splnená", Check("0", isCondition: true)[0].Message);
        Assert.IsEmpty(Check("Prizn_M"));
        Assert.IsEmpty(Check("Typ(Typ_R)", isCondition: true));
    }

    [TestMethod]
    public void ContextDependent_Info()
    {
        var r = ExprValidator.Validate("ZPOZDENI > 5 || VYLUKAZDE");
        Assert.HasCount(2, r.Diagnostics);
        Assert.IsTrue(r.Diagnostics.All(d => d is { Code: ExprDiagnosticCode.ContextDependentFunction, Severity: ExprSeverity.Info }));
    }

    [TestMethod]
    public void ParseError_IsReturnedAsOnlyDiagnostic()
    {
        var r = ExprValidator.Validate("Typ(Typ_R) && Foo");
        Assert.IsFalse(r.Compiles);
        Assert.IsTrue(r.HasErrors);
        Assert.HasCount(1, r.Diagnostics);
        Assert.AreEqual("Neznámy symbol Foo", r.Diagnostics[0].Message);
    }

    private static string Fixed(string text)
    {
        var d = Check(text).Single(x => x.Fix is not null);
        return d.Fix!.Apply(text);
    }

    [TestMethod]
    public void Fixes_ProduceExpectedText()
    {
        Assert.AreEqual("(Typ(Typ_R) && ZPOZDENIPRIJ > 5) || ODKLON", Fixed("Typ(Typ_R) && ZPOZDENIPRIJ > 5 || ODKLON"));
        Assert.AreEqual("(10 - 3) - 2", Fixed("10 - 3 - 2"));
        Assert.AreEqual("(-ZPOZDENIPRIJ) + 5", Fixed("-ZPOZDENIPRIJ + 5"));
        Assert.AreEqual("PRIZNAK(Prizn_M)", Fixed("PRIZNAK(Prizn_M) == 1"));
        Assert.AreEqual("!PRIZNAK(Prizn_M)", Fixed("PRIZNAK(Prizn_M) != 1"));
        Assert.AreEqual("ZPOZDENIPRIJ > 10", Fixed("ZPOZDENIPRIJ > 010"));
        Assert.AreEqual("(!ZPOZDENIPRIJ) > 5", Fixed("!ZPOZDENIPRIJ > 5"));
        Assert.AreEqual("Typ(Typ_Ex)", Check("Typ(Typ_ex)", withSymbols: false).Single().Fix!.Apply("Typ(Typ_ex)"));
        Assert.AreEqual("Typ(Type_RJ)", Check("Typ(Type_rj)").Single().Fix!.Apply("Typ(Type_rj)"));
        // 1 || 2 && 3: INISS = 1 || (2 && 3), zvycajna priorita rovnako - bez varovania
        Assert.AreEqual(0, Check("1 || 2 && 3").Count);
        Assert.IsNotNull(Check("ZESMERU(1234567)").Single().Suggestion);
    }
}
