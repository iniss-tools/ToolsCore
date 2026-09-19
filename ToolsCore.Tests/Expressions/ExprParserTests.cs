using System.Diagnostics.CodeAnalysis;
using ToolsCore.Expressions;

namespace ToolsCore.Tests.Expressions;

[TestClass]
[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
public class ExprParserTests
{
    private static ExprNode Parse(string text, ExprContext ctx = ExprContext.Condition, IExprSymbolProvider? symbols = null)
    {
        var r = ExprParser.Parse(text, ctx, symbols);
        Assert.IsTrue(r.Success, $"'{text}': {r.Error}");
        return r.Root!;
    }

    private static ExprDiagnostic Fail(string text, ExprContext ctx = ExprContext.Condition)
    {
        var r = ExprParser.Parse(text, ctx);
        Assert.IsFalse(r.Success, $"'{text}' sa nemal prelozit");
        return r.Error!;
    }

    /// <summary>Zapise strom v prefixovom tvare - na kontrolu asociativity a priority.</summary>
    private static string Dump(ExprNode n) => n switch
    {
        ExprNumberNode num => num.Value.ToString(),
        ExprStringNode s => $"\"{s.Value}\"",
        ExprParenNode p => Dump(p.Inner),
        ExprUnaryNode u => $"({ExprMessages.Operator(u.Operator)} {Dump(u.Operand)})",
        ExprBinaryNode b => $"({ExprMessages.Operator(b.Operator)} {Dump(b.Left)} {Dump(b.Right)})",
        ExprConditionalNode c => $"(?: {Dump(c.Condition)} {Dump(c.WhenTrue)} {Dump(c.WhenFalse)})",
        ExprFunctionNode f => f.Argument is null ? f.Canonical.ToString() : $"{f.Canonical}[{Dump(f.Argument)}]",
        _ => throw new InvalidOperationException()
    };

    // ------------------------------------------------------------------ gramatika

    [TestMethod]
    public void Connectives_SameLevel_RightAssociative()
    {
        Assert.AreEqual("(&& 1 (|| 2 3))", Dump(Parse("1 && 2 || 3")));
        Assert.AreEqual("(|| 1 (&& 2 3))", Dump(Parse("1 || 2 && 3")));
        Assert.AreEqual("(& 1 (|| 2 3))", Dump(Parse("1 & 2 || 3")));
        Assert.AreEqual("(AND 1 (OR 2 3))", Dump(Parse("1 AND 2 OR 3")));
        Assert.AreEqual("(|| (&& 1 2) 3)", Dump(Parse("(1 && 2) || 3")));
    }

    [TestMethod]
    public void Arithmetic_RightAssociative()
    {
        Assert.AreEqual("(- 10 (- 3 2))", Dump(Parse("10 - 3 - 2")));
        Assert.AreEqual("(/ 8 (/ 4 2))", Dump(Parse("8 / 4 / 2")));
        Assert.AreEqual("(+ 1 (* 2 3))", Dump(Parse("1 + 2 * 3")));
        Assert.AreEqual("(+ (* 1 2) 3)", Dump(Parse("1 * 2 + 3")));
    }

    [TestMethod]
    public void Negation_AppliesToWholeComparison()
    {
        Assert.AreEqual("(! (> ZPOZDENI 5))", Dump(Parse("!ZPOZDENI > 5")));
        Assert.AreEqual("(NOT (== 1 2))", Dump(Parse("NOT 1 == 2")));
        Assert.AreEqual("(~ 5)", Dump(Parse("~5")));
        Assert.AreEqual("(! (! 1))", Dump(Parse("!(!1)")));
    }

    [TestMethod]
    public void DoubleNegation_WithoutParens_IsError()
    {
        var e = Fail("!!1");
        Assert.AreEqual(ExprDiagnosticCode.InissUnexpected, e.Code);
        Assert.AreEqual("Neočakávaný symbol !", e.Message);
        Assert.AreEqual(1, e.Start);
    }

    [TestMethod]
    public void Sign_AppliesToWholeSum()
    {
        Assert.AreEqual("(- (+ 1 2))", Dump(Parse("-1 + 2")));
        Assert.AreEqual("(+ (- 1 2))", Dump(Parse("+1 - 2")));
        Assert.AreEqual("(== (- 1) (- 1))", Dump(Parse("-1 == -1")));
    }

    [TestMethod]
    public void Comparison_CannotChain()
    {
        // po porovnani nasleduje dalsie == - ziadna uroven ho neprijme, takze sa ocakava koniec
        var e = Fail("1 == 1 == 1");
        Assert.AreEqual("Očakávam koniec", e.Message);
        Assert.AreEqual(7, e.Start);
    }

    [TestMethod]
    public void Conditional_Operator()
    {
        Assert.AreEqual("(?: (== POZICE 1) 1 (?: 2 3 4))", Dump(Parse("POZICE == 1 ? 1 : 2 ? 3 : 4")));
        Assert.AreEqual("Očakávam :", Fail("1 ? 2").Message);
    }

    [TestMethod]
    public void Odd_RequiresParens()
    {
        Assert.AreEqual("(ODD 3)", Dump(Parse("ODD(3)")));
        Assert.AreEqual("Očakávam (", Fail("ODD 3").Message);
    }

    [TestMethod]
    public void Parens_Errors()
    {
        Assert.AreEqual("Očakávam )", Fail("(1 + 2").Message);
        Assert.AreEqual("Očakávam koniec", Fail("(1) 2").Message);
        Assert.AreEqual("Neočakávaný symbol )", Fail("()").Message);
        Assert.AreEqual("Neočakávaný symbol koniec", Fail("").Message);
        Assert.AreEqual("Neočakávaný symbol koniec", Fail("1 +").Message);
        Assert.AreEqual("Neočakávaný symbol *", Fail("* 1").Message);
    }

    // ------------------------------------------------------------------ funkcie

    [TestMethod]
    public void Function_NoArgs()
    {
        Assert.AreEqual("ZPOZDENI", Dump(Parse("ZPOZDENI")));
        Assert.AreEqual("ZPOZDENI", Dump(Parse("zpozdeni")));
        Assert.AreEqual("ZPOZDENI", Dump(Parse("DELAY")));
        // po funkcii bez argumentu je zatvorka neocakavana
        Assert.AreEqual("Očakávam koniec", Fail("ZPOZDENI()").Message);
    }

    [TestMethod]
    public void Function_OptionalArg()
    {
        Assert.AreEqual("TYP", Dump(Parse("TYP")));
        Assert.AreEqual("TYP[26]", Dump(Parse("Typ(Typ_R)")));
        Assert.AreEqual("TYP[26]", Dump(Parse("TYPE(Type_R)")));
        Assert.AreEqual("PRIZNAK[4096]", Dump(Parse("Priznak(Prizn_M)")));
        Assert.AreEqual("(== POZICE 3)", Dump(Parse("POZICE == Poz_K")));
        Assert.AreEqual("ZAJMSTANICE[5614616]", Dump(Parse("HOMESTATION(5614616)")));
    }

    [TestMethod]
    public void Function_TrainNumber_StringOrNumber()
    {
        Assert.AreEqual("CVLAKU", Dump(Parse("CVLAKU")));
        Assert.AreEqual("CVLAKU[\"1234\"]", Dump(Parse("CVLAKU(\"1234\")")));
        Assert.AreEqual("CVLAKU[1234]", Dump(Parse("TRAINNUM(1234)")));
        Assert.AreEqual("(== (/ CVLAKU 100) 25)", Dump(Parse("CVlaku/100==25")));
    }

    [TestMethod]
    public void Function_RequiredNumber()
    {
        Assert.AreEqual("ZESMERU[8000284]", Dump(Parse("ZESMERU(8000284)")));
        Assert.AreEqual("Očakávam (", Fail("ZESMERU").Message);
        Assert.AreEqual("Neočakávaný symbol reťazec v úvodzovkách", Fail("ZESMERU(\"x\")").Message);
    }

    [TestMethod]
    public void Function_RequiredString()
    {
        Assert.AreEqual("KOLEJODJ[\"SM-B\"]", Dump(Parse("KOLEJODJ(\"SM-B\")")));
        Assert.AreEqual("OPERATOR[\"České dráhy, a.s.\"]", Dump(Parse("OPERATOR(\"České dráhy, a.s.\")")));
        Assert.AreEqual("KOLEJPRIJ[\"\"]", Dump(Parse("KOLEJPRIJ(\"\")")));
        Assert.AreEqual("Očakávam (", Fail("OPERATOR").Message);
        var e = Fail("OPERATOR(1)");
        Assert.AreEqual(ExprDiagnosticCode.InissExpected, e.Code);
        Assert.AreEqual("Očakávam reťazec v úvodzovkách", e.Message);
    }

    [TestMethod]
    public void String_OutsideFunction_IsUnexpected()
    {
        Assert.AreEqual("Neočakávaný symbol reťazec v úvodzovkách", Fail("\"abc\"").Message);
        Assert.AreEqual("Neočakávaný symbol reťazec v úvodzovkách", Fail("1 == \"a\"").Message);
    }

    // ------------------------------------------------------------------ lexika

    [TestMethod]
    public void Numbers_Strtol()
    {
        Assert.AreEqual("10", Dump(Parse("10")));
        Assert.AreEqual("31", Dump(Parse("0x1F")));
        Assert.AreEqual("8", Dump(Parse("010")));
        Assert.AreEqual("0", Dump(Parse("0")));
        Assert.AreEqual("2147483647", Dump(Parse("2147483647")));
        Assert.AreEqual("Očakával som číslo", Fail("12AB").Message);
        Assert.AreEqual("Očakával som číslo", Fail("09").Message);
        Assert.AreEqual("Očakával som číslo", Fail("0x").Message);
        Assert.AreEqual("Očakával som číslo", Fail("2147483648").Message);
        Assert.AreEqual("Očakával som číslo", Fail("10AND").Message);
    }

    [TestMethod]
    public void DateAndTimeLiterals()
    {
        Assert.AreEqual("0", Dump(Parse("#30.12.1899#")));
        Assert.AreEqual("45292", Dump(Parse("#1.1.2024#")));
        Assert.AreEqual("81000", Dump(Parse("#22:30#")));
        Assert.AreEqual("3661", Dump(Parse("#1:1:1#")));
        Assert.AreEqual("Neznámy formát dátumu", Fail("#1.1#").Message);
        Assert.AreEqual("Neznámy formát dátumu", Fail("#31.2.2024#").Message);
        Assert.AreEqual("Neznámy formát času", Fail("#1:2:3:4#").Message);
        Assert.AreEqual("Neznámy formát času", Fail("#a:b#").Message);
        Assert.AreEqual("Neukončený reťazec", Fail("#12:30").Message);
    }

    [TestMethod]
    public void Strings_Unterminated()
    {
        var e = Fail("KOLEJODJ(\"abc");
        Assert.AreEqual(ExprDiagnosticCode.InissUnterminatedString, e.Code);
        Assert.AreEqual(9, e.Start);
    }

    [TestMethod]
    public void UnknownSymbols()
    {
        Assert.AreEqual("Neznámy symbol FOO", Fail("FOO").Message);
        Assert.AreEqual("Neznámy symbol =", Fail("1 = 1").Message);
        Assert.AreEqual("Neznámy symbol ,", Fail("1, 2").Message);
        Assert.AreEqual("Neznámy symbol '", Fail("OPERATOR('x')").Message);
        Assert.AreEqual("Neznámy symbol Typ_Foo", Fail("Typ(Typ_Foo)").Message);
        Assert.AreEqual("Neznámy symbol Typ_", Fail("Typ(Typ_)").Message);
        var e = Fail("Typ(Typ_R) && Bar");
        Assert.AreEqual(14, e.Start);
        Assert.AreEqual(3, e.Length);
    }

    [TestMethod]
    public void Constants_CaseInsensitive_And_English()
    {
        Assert.AreEqual("4096", Dump(Parse("PRIZN_M")));
        Assert.AreEqual("768", Dump(Parse("Flag_Lockout")));
        Assert.AreEqual("128", Dump(Parse("state_standing")));
        Assert.AreEqual("3", Dump(Parse("Pos_E")));
    }

    [TestMethod]
    public void IltisConstants_OnlyInWaitContext()
    {
        Assert.AreEqual("Neznámy symbol VVC", Fail("VVC").Message);
        Assert.AreEqual("Neznámy symbol OVC", Fail("OVC").Message);
        Assert.AreEqual(unchecked((int)0x80000000).ToString(), Dump(Parse("VVC", ExprContext.StateDgmWait)));
        Assert.AreEqual("134217728", Dump(Parse("Odj", ExprContext.StateDgmWait)));
        Assert.AreEqual("(| 1073741824 268435456)", Dump(Parse("OVC | Vj", ExprContext.StateDgmWait)));
    }

    [TestMethod]
    public void TrainTypes_BuiltIn()
    {
        Assert.AreEqual("0", Dump(Parse("Typ_Os")));
        Assert.AreEqual("7", Dump(Parse("Typ_Lan")));
        Assert.AreEqual("17", Dump(Parse("Typ_Os1")));
        Assert.AreEqual("29", Dump(Parse("Typ_ER")));
        Assert.AreEqual("47", Dump(Parse("Typ_R9")));
        Assert.AreEqual("54", Dump(Parse("Typ_TGV")));
        Assert.AreEqual("72", Dump(Parse("Typ_X9")));
        Assert.AreEqual("74", Dump(Parse("Typ_Nákl")));
        Assert.AreEqual("74", Dump(Parse("Typ_nakl"))); // bez diakritiky a velkosti pismen
        Assert.AreEqual("94", Dump(Parse("Type_Sl9")));
    }

    [TestMethod]
    public void TrainTypes_TrTypesKeysFirst()
    {
        var symbols = new TestSymbols
        {
            TrainTypeKeys = new Dictionary<string, int> { ["RJ"] = 39, ["RR"] = 40, ["Os"] = 0 }
        };
        Assert.AreEqual("39", Dump(Parse("Typ_RJ", symbols: symbols)));
        Assert.AreEqual("40", Dump(Parse("Typ_rr", symbols: symbols)));
        Assert.AreEqual("26", Dump(Parse("Typ_R", symbols: symbols))); // zabudovany, v TrTypes nie je
        Assert.IsFalse(ExprParser.Parse("Typ_RJ").Success); // bez TrTypes nezname
    }

    [TestMethod]
    public void Tokens_HavePositions()
    {
        var r = ExprParser.Parse("Typ(Typ_R) || ODKLON");
        var kinds = r.Tokens.Select(t => t.Kind).ToArray();
        CollectionAssert.AreEqual(new[]
        {
            ExprTokenKind.Function, ExprTokenKind.LParen, ExprTokenKind.Number, ExprTokenKind.RParen,
            ExprTokenKind.Or, ExprTokenKind.Function, ExprTokenKind.End
        }, kinds);
        Assert.AreEqual(4, r.Tokens[2].Start);
        Assert.AreEqual(5, r.Tokens[2].Length);
        Assert.AreEqual(ExprNumberSource.TrainType, r.Tokens[2].NumberSource);
        Assert.AreEqual(0, r.Root!.Start);
        Assert.AreEqual(20, r.Root.Length);
    }
}

internal sealed class TestSymbols : IExprSymbolProvider
{
    public IReadOnlyDictionary<string, int>? TrainTypeKeys { get; init; }
    public HashSet<int>? Stations { get; init; }
    public HashSet<string>? Tracks { get; init; }
    public HashSet<string>? Operators { get; init; }

    public bool? StationExists(int id) => Stations?.Contains(id);
    public bool? TrackExists(string name) => Tracks?.Contains(name);
    public bool? OperatorExists(string name) => Operators?.Contains(name);
}
