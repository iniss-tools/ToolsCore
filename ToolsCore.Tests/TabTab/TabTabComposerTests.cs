using System.Diagnostics.CodeAnalysis;
using ToolsCore.Expressions;
using ToolsCore.TabTab;

namespace ToolsCore.Tests.TabTab;

[TestClass]
[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
public class TabTabComposerTests
{
    private sealed class Train : IExprTrainContext
    {
        public string TrainNumber { get; init; } = "1234";
        public int Position { get; init; } = 2;
        public int TrainTypeIndex { get; init; } = 0; // Os
        public uint Flags { get; init; }
        public uint State { get; init; }
        public bool IsDeflected { get; init; }
        public int ArrivalDelaySeconds { get; init; }
        public int DepartureDelaySeconds { get; init; }
        public int StayTimeSeconds => 0;
        public int PlannedStayTimeSeconds => 0;
        public int HomeStationId => 1;
        public int BaseStationId => 2;
        public int EndStationId => 3;
        public bool IsLocal => true;
        public bool IsFromStation(int stationId) => false;
        public bool IsToStation(int stationId) => false;
        public string ArrivalTrack { get; init; } = "";
        public string DepartureTrack { get; init; } = "";
        public DateOnly ArrivalDate => new(2024, 1, 1);
        public DateOnly DepartureDate => new(2024, 1, 1);
        public TimeOnly ArrivalTime => new(8, 0);
        public TimeOnly DepartureTime => new(8, 5);
        public string OperatorName => "ZSSK";
        public string ArrivalLine { get; init; } = "";
        public string DepartureLine { get; init; } = "";
        public string TrainName => "";
    }

    private static TabTabValue Compose(string tab1, Train train, string own = "Os", int divType = 3, string? tab2 = null,
        Func<string, TabTabValue?>? columns = null, ExprEvalSite site = ExprEvalSite.Default, bool byParts = true, int idx = 15)
    {
        var r = TabTabComposer.Compose(new TabTabColumnInput
        {
            Train = train,
            Site = site,
            OwnValue = new TabTabValue(own, null),
            DivType = divType,
            TypeItemsIdx = idx,
            Tab1 = TabTabSection.Parse(tab1),
            Tab2 = tab2 is null ? null : TabTabSection.Parse(tab2),
            ColumnValue = columns,
            MergeByParts = byParts
        });
        Assert.IsNull(r.Error, r.Error);
        return r.Value;
    }

    [TestMethod]
    public void Switch_FirstTrueWins_And_Placeholder()
    {
        const string tab = "Stav(Stav_Stoji), \"@\"{94}, \\\r\n(Typ(Typ_R) || Typ(Typ_Ex)), \"@\"{89}, \\\r\n1, \"@\"{90} = #SWITCH";
        Assert.AreEqual(new TabTabValue("Os", 90), Compose(tab, new Train()));
        Assert.AreEqual(new TabTabValue("R", 89), Compose(tab, new Train { TrainTypeIndex = 26 }, own: "R"));
        Assert.AreEqual(new TabTabValue("R", 94), Compose(tab, new Train { TrainTypeIndex = 26, State = 0x80 }, own: "R"));
    }

    [TestMethod]
    public void Switch_TextWithoutPlaceholder_Closes()
    {
        const string tab = "Zpozdeni, \"Mešká#@ min.\" =#SWITCH\r\nOdklon, \"ODKLON\" = #SWITCH";
        Assert.AreEqual(new TabTabValue("Mešká#7 min.", null), Compose(tab, new Train { DepartureDelaySeconds = 7 * 60 }, own: "7"));
        Assert.AreEqual(new TabTabValue("ODKLON", null), Compose(tab, new Train { IsDeflected = true }, own: "Kúty"));
        Assert.AreEqual(new TabTabValue("Kúty", null), Compose(tab, new Train(), own: "Kúty"));
    }

    [TestMethod]
    public void Vyluka_Odklon_PozOdj_BySite()
    {
        const string tab = "Autobus =#VYLUKA\r\nOdkl =#ODKLON\r\nB2 = #POZODJ_3";
        Assert.AreEqual("Autobus", Compose(tab, new Train { Flags = 0x100 }, own: "1").Text);
        Assert.AreEqual("Autobus", Compose(tab, new Train { Flags = 0x100 }, own: "1", site: ExprEvalSite.ArrivalTable).Text);
        Assert.AreEqual("1", Compose(tab, new Train { Flags = 0x200 }, own: "1", site: ExprEvalSite.ArrivalTable).Text);
        Assert.AreEqual("Odkl", Compose(tab, new Train { IsDeflected = true }, own: "1").Text);
        Assert.AreEqual("B2", Compose(tab, new Train { DepartureTrack = "3" }, own: "1").Text);
        Assert.AreEqual("1", Compose(tab, new Train { DepartureTrack = "4" }, own: "1").Text);
    }

    [TestMethod]
    public void Merge_ByParts_And_Merge2()
    {
        const string tab = "1,,\"5\",\\\r\n1,\" \",\"min#MIN\" = #MERGE";
        Assert.AreEqual("5 min#5 MIN", Compose(tab, new Train(), own: "").Text);
        Assert.AreEqual("5 min#MIN", Compose(tab, new Train(), own: "", byParts: false).Text);
        Assert.AreEqual("5 min#MIN", Compose(tab.Replace("#MERGE", "#MERGE2"), new Train(), own: "").Text);
    }

    [TestMethod]
    public void Merge_WithPlaceholder_And_ColumnRef()
    {
        const string tab = "(Typ(Typ_Zr)),,\"@\"{34371},\\\r\n(!(Typ(Typ_EC) || Typ(Typ_R))),,\"@\"{34370},\\\r\n(!LinkaOdj(\"\")),\"#\",%Linka% = #MERGE";
        TabTabValue? Columns(string name) => name == "Linka" ? new TabTabValue("S20", null) : null;

        Assert.AreEqual(new TabTabValue("Os#S20", 34370), Compose(tab, new Train { DepartureLine = "S20" }, own: "Os", columns: Columns));
        Assert.AreEqual(new TabTabValue("Os", 34370), Compose(tab, new Train(), own: "Os", columns: Columns));
        Assert.AreEqual(new TabTabValue("R", null), Compose(tab, new Train { TrainTypeIndex = 26 }, own: "R", columns: Columns));
    }

    [TestMethod]
    public void DivType_SimpleRules()
    {
        const string tab = "R{81}=R\r\nSC{82}=SC{18}\r\n{@}=-{@}\r\nx=Os";
        Assert.AreEqual(new TabTabValue("R", 81), Compose(tab, new Train(), own: "R", divType: 3));
        Assert.AreEqual(new TabTabValue("Ex", null), Compose(tab, new Train(), own: "Ex", divType: 3));
        Assert.AreEqual(new TabTabValue("", null), Compose(tab, new Train(), own: "Ex", divType: 1));
        Assert.AreEqual(new TabTabValue("x", null), Compose(tab, new Train(), own: "Os", divType: 1));
        Assert.AreEqual(new TabTabValue("Ex", null), Compose(tab, new Train(), own: "Ex", divType: 0));
        // DIVTYPE 0 nepouzije ani udalosti
        Assert.AreEqual("1", Compose("Autobus =#VYLUKA", new Train { Flags = 0x300 }, own: "1", divType: 0).Text);
    }

    [TestMethod]
    public void DivType_ByChars_And_Time()
    {
        const string chars = "A1=A\r\nB2=B\r\n#VYL =#VYLUKA";
        Assert.AreEqual("A1B2 A1", Compose(chars, new Train(), own: "ABxA", divType: 4).Text);

        const string hours = "h12=12\r\nh08=08";
        const string digits = "d3=3\r\nd0=0\r\nd5=5";
        Assert.AreEqual("h12d3d0", Compose(hours, new Train(), own: "12:30", divType: 2, tab2: digits).Text);
        Assert.AreEqual("", Compose(hours, new Train(), own: "12:47", divType: 2, tab2: digits).Text);
        Assert.AreEqual("h08d0d5", Compose(hours, new Train(), own: "08.05", divType: 2, tab2: digits).Text);
    }

    [TestMethod]
    public void IgnoreCase_Section()
    {
        const string tab = "IgnoreCase\r\nOsobný=os";
        Assert.AreEqual("Osobný", Compose(tab, new Train(), own: "Os", divType: 3).Text);
        Assert.AreEqual("Os", Compose("Osobný=os", new Train(), own: "Os", divType: 3).Text);
    }

    [TestMethod]
    public void Steps_AreRecorded()
    {
        var r = TabTabComposer.Compose(new TabTabColumnInput
        {
            Train = new Train(),
            OwnValue = new TabTabValue("Os", null),
            DivType = 3,
            TypeItemsIdx = 15,
            Tab1 = TabTabSection.Parse("1, \"@\"{90} = #SWITCH\r\nOs{81}=Os")
        });
        Assert.AreEqual(new TabTabValue("Os", 81), r.Value);
        Assert.IsTrue(r.Steps.Any(s => s.Source == "#SWITCH"));
        Assert.IsTrue(r.Steps.Any(s => s.Source == "vlastná hodnota"));
        Assert.AreEqual("DIVTYPE 3", r.Steps[^1].Source);
    }
}
