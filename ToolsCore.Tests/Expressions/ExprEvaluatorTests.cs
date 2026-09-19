using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToolsCore.Expressions;

namespace ToolsCore.Tests.Expressions;

[TestClass]
public class ExprEvaluatorTests
{
    private sealed class Train : IExprTrainContext
    {
        public string TrainNumber { get; init; } = "1234";
        public int Position { get; init; } = 2;
        public int TrainTypeIndex { get; init; } = 26; // R
        public uint Flags { get; init; } = 0x1000 | 0x03; // Prizn_M + nizsi bajt
        public uint State { get; init; } = 0x80; // Stav_Stoji
        public bool IsDeflected { get; init; }
        public int ArrivalDelaySeconds { get; init; } = 5 * 60 + 59;
        public int DepartureDelaySeconds { get; init; } = 7 * 60;
        public int StayTimeSeconds { get; init; } = 600;
        public int PlannedStayTimeSeconds { get; init; } = 120;
        public int HomeStationId { get; init; } = 5614616;
        public int BaseStationId { get; init; } = 5453414;
        public int EndStationId { get; init; } = 5457176;
        public bool IsLocal { get; init; } = true;
        public bool IsFromStation(int stationId) => stationId == 8000284;
        public bool IsToStation(int stationId) => stationId == 5457176;
        public string ArrivalTrack { get; init; } = "1";
        public string DepartureTrack { get; init; } = "";
        public DateOnly ArrivalDate { get; init; } = new(2024, 1, 1);
        public DateOnly DepartureDate { get; init; } = new(2024, 1, 2);
        public TimeOnly ArrivalTime { get; init; } = new(12, 30, 15);
        public TimeOnly DepartureTime { get; init; } = new(23, 59, 0);
        public string OperatorName { get; init; } = "České dráhy, a.s.";
        public string ArrivalLine { get; init; } = "S20";
        public string DepartureLine { get; init; } = "S20";
        public string TrainName { get; init; } = "Pendolino";
    }

    private static int Eval(string text, IExprTrainContext? train = null, ExprEvalSite site = ExprEvalSite.Default,
        ExprContext ctx = ExprContext.Condition)
    {
        var r = ExprParser.Parse(text, ctx);
        Assert.IsTrue(r.Success, $"'{text}': {r.Error}");
        return new ExprEvaluator(train ?? new Train(), site, new DateTime(2024, 3, 5, 6, 7, 8)).Evaluate(r.Root!);
    }

    [TestMethod]
    public void Arithmetic_And_Logic()
    {
        Assert.AreEqual(9, Eval("10 - 3 - 2"));
        Assert.AreEqual(4, Eval("8 / 4 / 2"));
        Assert.AreEqual(7, Eval("1 + 2 * 3"));
        Assert.AreEqual(0, Eval("0 && 1 || 1")); // = 0 && (1 || 1)
        Assert.AreEqual(0, Eval("0 && (1 || 1)"));
        Assert.AreEqual(1, Eval("(0 && 1) || 1"));
        Assert.AreEqual(1, Eval("5 AND 3"));
        Assert.AreEqual(1, Eval("5 & 3"));
        Assert.AreEqual(6, Eval("5 ^ 3"));
        Assert.AreEqual(-6, Eval("~5"));
        Assert.AreEqual(1, Eval("!0"));
        Assert.AreEqual(0, Eval("NOT 7"));
        Assert.AreEqual(1, Eval("ODD(3)"));
        Assert.AreEqual(-1, Eval("ODD(-3)"));
        Assert.AreEqual(0, Eval("ODD(4)"));
        Assert.AreEqual(-3, Eval("-1 + 2 * 1"));
        Assert.AreEqual(2, Eval("-7 % 5 + 0 ? 2 : 3"));
        Assert.AreEqual(1, Eval("2 > 1 ? 1 : 0"));
        Assert.AreEqual(1, Eval("-1 < 1"));
    }

    [TestMethod]
    public void DivisionByZero_Throws()
    {
        Assert.ThrowsExactly<ExprEvaluationException>(() => Eval("1 / 0"));
        Assert.ThrowsExactly<ExprEvaluationException>(() => Eval("1 % (2 - 2)"));
    }

    [TestMethod]
    public void TrainNumber()
    {
        Assert.AreEqual(1234, Eval("CVLAKU"));
        Assert.AreEqual(1, Eval("CVLAKU(1234)"));
        Assert.AreEqual(1, Eval("CVLAKU(\"1234\")"));
        Assert.AreEqual(0, Eval("CVLAKU(\"01234\")"));
        Assert.AreEqual(12, Eval("CVlaku/100"));
        Assert.AreEqual(1, Eval("CVlaku>=1137 && CVlaku<=1249"));
        Assert.AreEqual(12, Eval("CVLAKU", new Train { TrainNumber = "12ab" }));
        Assert.AreEqual(0, Eval("CVLAKU", new Train { TrainNumber = "Os 12" }));
    }

    [TestMethod]
    public void Position_Type_Flags_State()
    {
        Assert.AreEqual(2, Eval("POZICE"));
        Assert.AreEqual(1, Eval("POZICE(Poz_P)"));
        Assert.AreEqual(0, Eval("POSITION == Pos_B"));
        Assert.AreEqual(26, Eval("TYP"));
        Assert.AreEqual(1, Eval("Typ(Typ_R)"));
        Assert.AreEqual(0, Eval("Typ(Typ_Os)"));
        Assert.AreEqual(0x1000, Eval("PRIZNAK"));           // bez najnizsieho bajtu
        Assert.AreEqual(0x1000, Eval("PRIZNAK(Prizn_M)"));  // maska, nie 1
        Assert.AreEqual(0, Eval("PRIZNAK(Prizn_M) == 1"));
        Assert.AreEqual(1, Eval("PRIZNAK(Prizn_M) != 0"));
        Assert.AreEqual(0, Eval("FLAG(Flag_Lockout)"));
        Assert.AreEqual(0x80, Eval("STAV"));
        Assert.AreEqual(1, Eval("STAV(Stav_Stoji)"));
        Assert.AreEqual(0, Eval("STAV(Stav_Stoji | Stav_Odbaven)")); // vsetky bity
        Assert.AreEqual(0, Eval("NAKLTYP"));
        Assert.AreEqual(1, Eval("CARGO", new Train { TrainTypeIndex = 74 }));
        Assert.AreEqual(1, Eval("NAKLTYP", new Train { TrainTypeIndex = 94 }));
        Assert.AreEqual(0, Eval("NAKLTYP", new Train { TrainTypeIndex = 72 }));
    }

    [TestMethod]
    public void Delays_Minutes_And_Context()
    {
        Assert.AreEqual(5, Eval("ZPOZDENIPRIJ"));
        Assert.AreEqual(7, Eval("DELAYDEP"));
        Assert.AreEqual(7, Eval("ZPOZDENI"));
        Assert.AreEqual(5, Eval("ZPOZDENI", site: ExprEvalSite.ArrivalTable));
        Assert.AreEqual(7, Eval("ZPOZDENI", site: ExprEvalSite.DepartureTable));
        Assert.AreEqual(10, Eval("DOBAPOBYTU"));
        Assert.AreEqual(2, Eval("PLANSTAYTIME"));
        Assert.AreEqual(300, Eval("ZPOZDENIPRIJ*60"));
    }

    [TestMethod]
    public void Lockout_Context()
    {
        var arr = new Train { Flags = 0x100 };
        var dep = new Train { Flags = 0x200 };
        Assert.AreEqual(1, Eval("VYLUKAZDE", arr));
        Assert.AreEqual(1, Eval("VYLUKAZDE", dep));
        Assert.AreEqual(1, Eval("LOCKOUT", arr, ExprEvalSite.ArrivalTable));
        Assert.AreEqual(0, Eval("VYLUKAZDE", dep, ExprEvalSite.ArrivalTable));
        Assert.AreEqual(0, Eval("VYLUKAZDE", arr, ExprEvalSite.DepartureTable));
        Assert.AreEqual(0, Eval("VYLUKAZDE"));
    }

    [TestMethod]
    public void Stations_Directions_Local()
    {
        Assert.AreEqual(5614616, Eval("ZAJMSTANICE"));
        Assert.AreEqual(1, Eval("HOMESTATION(5614616)"));
        Assert.AreEqual(1, Eval("VYCHSTANICE(5453414)"));
        Assert.AreEqual(1, Eval("ENDSTATION(5457176)"));
        Assert.AreEqual(0, Eval("CILSTANICE(1)"));
        Assert.AreEqual(1, Eval("ZESMERU(8000284)"));
        Assert.AreEqual(0, Eval("ZESMERU(1)"));
        Assert.AreEqual(1, Eval("DOSMERU(5457176)"));
        Assert.AreEqual(1, Eval("MISTNI"));
        Assert.AreEqual(0, Eval("CIZI"));
        Assert.AreEqual(1, Eval("OUTSIDE", new Train { IsLocal = false }));
        Assert.AreEqual(1, Eval("ODKLON", new Train { IsDeflected = true }));
    }

    [TestMethod]
    public void Strings_Tracks_Operator_Lines_Name()
    {
        Assert.AreEqual(1, Eval("KOLEJPRIJ(\"1\")"));
        Assert.AreEqual(0, Eval("KOLEJPRIJ(\"01\")"));
        Assert.AreEqual(1, Eval("KOLEJODJ(\"\")"));
        Assert.AreEqual(1, Eval("!KOLEJPRIJ(\"\")"));
        Assert.AreEqual(1, Eval("OPERATOR(\"České dráhy, a.s.\")"));
        Assert.AreEqual(1, Eval("OPERATOR(\"ČESKÉ DRÁHY, A.S.\")"));   // bez ohladu na velkost pismen
        Assert.AreEqual(0, Eval("OPERATOR(\"Ceske drahy, a.s.\")"));   // diakritika sa rozlisuje
        Assert.AreEqual(1, Eval("LINKAPRIJ(\"S20\")"));
        Assert.AreEqual(0, Eval("LINKAODJ(\"s20\")"));                 // presne
        Assert.AreEqual(0, Eval("ZMENALINKY"));
        Assert.AreEqual(1, Eval("ZMENALINKY", new Train { DepartureLine = "S21" }));
        Assert.AreEqual(1, Eval("NAZVLAKU(\"pendolino\")"));
    }

    [TestMethod]
    public void Dates_And_Times()
    {
        Assert.AreEqual(45292, Eval("DATUMPRIJ"));
        Assert.AreEqual(1, Eval("DATUMPRIJ == #1.1.2024#"));
        Assert.AreEqual(1, Eval("DATEDEP == DATUMPRIJ + 1"));
        Assert.AreEqual(12 * 3600 + 30 * 60 + 15, Eval("CASPRIJ"));
        Assert.AreEqual(1, Eval("CASODJ >= #22:00# || CASODJ < #5:00#"));
        Assert.AreEqual(1, Eval("DATE == #5.3.2024#"));
        Assert.AreEqual(6 * 3600 + 7 * 60 + 8, Eval("TIME"));
    }

    [TestMethod]
    public void IndCat()
    {
        Assert.AreEqual(1, ExprEvaluator.IndCat6(1, 0));
        Assert.AreEqual(4, ExprEvaluator.IndCat6(1, 0x200));
        Assert.AreEqual(1, ExprEvaluator.IndCat6(1, 0x100));
        Assert.AreEqual(2, ExprEvaluator.IndCat6(2, 0));
        Assert.AreEqual(5, ExprEvaluator.IndCat6(2, 0x100));
        Assert.AreEqual(5, ExprEvaluator.IndCat6(2, 0x200));
        Assert.AreEqual(5, ExprEvaluator.IndCat6(2, 0x300));
        Assert.AreEqual(3, ExprEvaluator.IndCat6(3, 0));
        Assert.AreEqual(6, ExprEvaluator.IndCat6(3, 0x100));
        Assert.AreEqual(3, ExprEvaluator.IndCat6(3, 0x200));
        Assert.AreEqual(-1, ExprEvaluator.IndCat6(0, 0));

        Assert.AreEqual(4, ExprEvaluator.IndCat8(1, 0x200));
        Assert.AreEqual(2, ExprEvaluator.IndCat8(2, 0));
        Assert.AreEqual(5, ExprEvaluator.IndCat8(2, 0x100));
        Assert.AreEqual(6, ExprEvaluator.IndCat8(2, 0x300));
        Assert.AreEqual(7, ExprEvaluator.IndCat8(2, 0x200));
        Assert.AreEqual(8, ExprEvaluator.IndCat8(3, 0x100));
        Assert.AreEqual(3, ExprEvaluator.IndCat8(3, 0x200));

        Assert.AreEqual(5, Eval("INDCAT6", new Train { Position = 2, Flags = 0x200 }));
        Assert.AreEqual(7, Eval("INDCAT8", new Train { Position = 2, Flags = 0x200 }));
    }

    [TestMethod]
    public void ConstantFolding()
    {
        Assert.IsTrue(ExprEvaluator.TryFoldConstant(ExprParser.Parse("(1==1) && Prizn_M").Root!, out var v));
        Assert.AreEqual(1, v);
        Assert.IsFalse(ExprEvaluator.TryFoldConstant(ExprParser.Parse("1 + ZPOZDENI").Root!, out _));
        Assert.IsFalse(ExprEvaluator.TryFoldConstant(ExprParser.Parse("1 / 0").Root!, out _));
    }

    [TestMethod]
    public void WaitConstants()
    {
        Assert.AreEqual(0x40000000 | 0x10000000, Eval("OVC | Vj", ctx: ExprContext.StateDgmWait));
    }
}
