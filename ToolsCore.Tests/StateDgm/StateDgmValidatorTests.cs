using System.Diagnostics.CodeAnalysis;
using System.Text;
using ToolsCore.Expressions;
using ToolsCore.StateDgm;
using ToolsCore.Tests.Expressions;

namespace ToolsCore.Tests.StateDgm;

/// <summary>
///     Kontrola diagramu - realne subory bez chyb, umele chyby najdene.
/// </summary>
[TestClass]
[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
public class StateDgmValidatorTests
{
    private static readonly string[] ReportKeys = ["Přijíždí", "Vjíždí", "Zastavil", "Pobytové", "Odjede", "Zpoždění odjezd", "Zpoždění příjezd"];

    [TestMethod]
    public void Validator_FindsProblems()
    {
        var d = new StateDgmDiagram();
        d.Designs.Add(new StateDgmDesign { Key = "KoľajN", Bitmaps = "3-0,1,2" });
        d.Designs.Add(new StateDgmDesign { Key = "Zlý", Bitmaps = "abc" });
        d.TimePoints.Add(new StateDgmTimePoint { Key = "#T1", TimePointKey1 = "#Neexistuje", Operator = "avg" });
        d.IndCat = "INDCAT8";

        var cat = new StateDgmCategory { Key = "#K", Name = "Kat", Icon = 4 };
        d.Categories.Add(cat);
        var start = new StateDgmState { Key = "#Start", AutoMode = StateDgmDynamic.FromNumber(3), AutoTimePoint = StateDgmDynamic.FromNumber(1), DefaultControl = 5 };
        start.Events.Add(new StateDgmEvent { Key = "#Go", NextState = "Nikde", Class = "SDEventUniPos", ReportKey = "Cudzie" });
        start.Events.Add(new StateDgmEvent { Key = "#Go", Class = "SDEventChangeState" });
        start.Events.Add(new StateDgmEvent { Key = "#Dlg", Class = "SDEventWithDialog", Dialog = "SDDlgX" });
        start.Events.Add(new StateDgmEvent { Key = "#Attr", Class = "SDEventVlakAttr" });
        start.Events.Add(new StateDgmEvent { Key = "#Ok", NextState = "Druhy", Class = "SDEventUniPos", ReportKey = "Odjede" });
        start.Controls.Add(new StateDgmControl { CtrlId = 0, DesignKey = "Nie", EventKey = "#Nie" });
        start.Controls.Add(new StateDgmControl { CtrlId = 0, DesignKey = "KoľajN", EventKey = "#Ok" });
        start.Starters.Add(new StateDgmStarter { Key = "S", EventKey = "#Ok", TimePointKey = "#Zly" });
        start.AutoCondition = "Typ(Typ_R";
        cat.States.Add(start);
        cat.States.Add(new StateDgmState { Key = "Druhy", Icon = 9 });
        cat.States.Add(new StateDgmState { Key = "Sirota", Attr = StateDgmAttr.Shadow });

        var diags = StateDgmValidator.Validate(d, new StateDgmValidationOptions { ReportKeys = ReportKeys, Symbols = new TestSymbols { TrainTypeKeys = AllTypes() } });
        var codes = diags.Select(x => x.Code).ToHashSet();

        foreach (var expected in new[]
                 {
                     StateDgmDiagnosticCode.BitmapsFormat, StateDgmDiagnosticCode.TimePointKeyMissing, StateDgmDiagnosticCode.TimePointOperator,
                     StateDgmDiagnosticCode.IndCatCategoryCount, StateDgmDiagnosticCode.IconRange, StateDgmDiagnosticCode.AutoModeRange,
                     StateDgmDiagnosticCode.DefaultControlRange, StateDgmDiagnosticCode.NextStateMissing, StateDgmDiagnosticCode.DuplicateKey,
                     StateDgmDiagnosticCode.ReportKeyUnknown, StateDgmDiagnosticCode.ChangeStateNoNext, StateDgmDiagnosticCode.DialogInvalid,
                     StateDgmDiagnosticCode.VlakAttrNoDelay, StateDgmDiagnosticCode.DesignKeyMissing, StateDgmDiagnosticCode.EventKeyMissing,
                     StateDgmDiagnosticCode.DuplicateCtrlId, StateDgmDiagnosticCode.Expression, StateDgmDiagnosticCode.StateUnreachable,
                     StateDgmDiagnosticCode.StateDeadEnd
                 })
            Assert.IsTrue(codes.Contains(expected), $"chýba {expected}:\n{string.Join("\n", diags)}");

        var expr = diags.Single(x => x.Code == StateDgmDiagnosticCode.Expression);
        Assert.AreEqual(StateDgmKeys.AUTO_CONDITION, expr.ExprKey);
        Assert.IsTrue(expr.IsError);
        Assert.IsNotNull(expr.Expr);
        Assert.AreEqual(new StateDgmLocation(StateDgmElementKind.State, 0, 0), expr.Location);

        var dead = diags.Single(x => x.Code == StateDgmDiagnosticCode.StateDeadEnd);
        Assert.AreEqual(1, dead.Location.State, "Druhy je slepý; Sirota je Shadow (koncový) a nedosiahnuteľný");
        Assert.AreEqual(2, diags.Single(x => x.Code == StateDgmDiagnosticCode.StateUnreachable).Location.State);
    }

    [TestMethod]
    public void Validator_CleanDiagram()
    {
        var d = new StateDgmDiagram();
        d.Designs.Add(new StateDgmDesign { Key = "OdídeN", Bitmaps = "36-7,8,9" });
        var cat = new StateDgmCategory { Key = "#K", Name = "Kat" };
        d.Categories.Add(cat);
        var start = new StateDgmState { Key = "#Start", AutoMode = StateDgmDynamic.FromExpression("Typ(Typ_R) ? 2 : 0"), AutoTimePoint = StateDgmDynamic.FromNumber(2), AutoTimePointAdd = StateDgmDynamic.FromNumber(-600), Wait = StateDgmDynamic.FromWait(StateDgmWaitEvent.OVC) };
        start.Events.Add(new StateDgmEvent { Key = "#Go", NextState = "Koniec", Class = "SDEventUniPos", ReportKey = "Odjede" });
        start.Controls.Add(new StateDgmControl { CtrlId = 0, DesignKey = "OdídeN", EventKey = "#Go" });
        cat.States.Add(start);
        cat.States.Add(new StateDgmState { Key = "Koniec", Attr = StateDgmAttr.Shadow | StateDgmAttr.Odbaven });
        for (var i = 0; i < 5; i++) d.Categories.Add(new StateDgmCategory { Key = $"#K{i}", Name = $"K{i}", States = { new StateDgmState { Key = "#Start", Attr = StateDgmAttr.Shadow } } });

        var diags = StateDgmValidator.Validate(d, new StateDgmValidationOptions { ReportKeys = ReportKeys, Symbols = new TestSymbols { TrainTypeKeys = AllTypes() } });
        Assert.AreEqual(0, diags.Count, string.Join("\n", diags));

        diags = StateDgmValidator.Validate(d, new StateDgmValidationOptions { HasIltis = false });
        Assert.AreEqual(StateDgmDiagnosticCode.WaitWithoutIltis, diags.Single().Code);
    }

    private static Dictionary<string, int> AllTypes()
    {
        var d = new Dictionary<string, int>();
        for (var i = 0; i < ExprTrainTypes.Count; i++)
            if (ExprTrainTypes.NameOf(i) is { } n)
                d[n] = i;
        return d;
    }
}
