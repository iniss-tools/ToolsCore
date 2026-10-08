using System.Diagnostics.CodeAnalysis;
using ToolsCore.Iniss.Registry;
using ToolsCore.Iniss.Tables;

namespace ToolsCore.Tests.Tables;

/// <summary>
/// Priradenie fyzickych tabul k linkam (sekciam Driver*) ako v INISSe 3.39.
/// </summary>
[TestClass]
[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
public class DriverLinesTests
{
    private static InissTable Table(int index, string key, TableManufacturer manufacturer, int port, int id = 1) =>
        new(index, "Test.2025", new TablePhysical
        {
            Key = key, Name = key, ID = id, CommunicationPort = port, Rem = "", SaveXML = "", ReverseArrows = "", Comment = "",
            TableCatalog = new TableCatalog { Key = "K", Name = "K", Comment = "", Manufacturer = manufacturer }
        });

    private static ResolvedConfig Config(params (string Section, int Class, string Port)[] drivers)
    {
        var machine = new RegBranch();
        foreach (var (section, cls, port) in drivers)
            machine.Set(section, "TableClass", RegRawValue.Dword(cls)).Set(section, "TablePort", RegRawValue.String(port));
        return RegResolver.Resolve(new InissConfigSource { AppName = "T", Version = new RegVersion(3, 39), Machine = machine });
    }

    [TestMethod]
    public void BezCislaLinky_AutomatickyNaLinkuVlastnejRodiny()
    {
        var config = Config(("Driver", 128, "2=TCP://h:1"), ("Driver0", 4, "COM3"), ("Driver1", 4, "COM4"));
        var tables = new[] { Table(0, "ELEN16", TableManufacturer.Elen16, 0) };

        var map = DriverLines.Build(config, tables);

        // vzdialeny INISS ma zhodu len 1 - automaticky sa nepouzije; z dvoch ELEN liniek vyhra prva
        Assert.IsTrue(LineTableOf(map, "Driver0").Single().Automatic);
        Assert.IsEmpty(LineTableOf(map, "Driver1"));
        Assert.IsEmpty(map.Unserved);
    }

    [TestMethod]
    public void CisloLinky_LenAkProtokolVyrobcuPrijima()
    {
        var config = Config(("Driver", 4, "COM3"), ("Driver0", 128, "5=TCP://h:1"));
        var tables = new[]
        {
            Table(0, "LCD1 na ELEN", TableManufacturer.Lcd1, 3),
            Table(1, "LCD1 cez vzdialeny", TableManufacturer.Lcd1, 5),
            Table(2, "bez linky", TableManufacturer.Elen, 9)
        };

        var map = DriverLines.Build(config, tables);

        Assert.IsEmpty(LineTableOf(map, "Driver"));
        Assert.HasCount(1, LineTableOf(map, "Driver0"));
        CollectionAssert.AreEquivalent(new[] { "LCD1 na ELEN", "bez linky" }, map.Unserved.Select(u => u.Table.Table.Key).ToArray());
        Assert.IsTrue(map.Lines.Single(l => l.Section == "Driver").Problems.Any(p => p.Severity == RegSeverity.Warning));
    }

    [TestMethod]
    public void DuplicitneCisloLinky_DalsiaLinkaSaIgnoruje()
    {
        var config = Config(("Driver", 4, "COM3"), ("Driver0", 4, "3=TCP://h:1"));
        var tables = new[] { Table(0, "ELEN", TableManufacturer.Elen, 3) };

        var map = DriverLines.Build(config, tables);

        Assert.IsTrue(map.Lines.Single(l => l.Section == "Driver").Active);
        var second = map.Lines.Single(l => l.Section == "Driver0");
        Assert.IsFalse(second.Active);
        Assert.AreEqual(RegSeverity.Error, second.Problems.Single().Severity);
        Assert.HasCount(1, LineTableOf(map, "Driver"));
    }

    [TestMethod]
    public void TabulaBezAdresy_SaNeposiela()
    {
        var map = DriverLines.Build(Config(("Driver", 4, "COM3")), [Table(0, "Internet", TableManufacturer.Elen, 0, id: -1)]);

        Assert.IsEmpty(LineTableOf(map, "Driver"));
        Assert.IsEmpty(map.Unserved);
    }

    [TestMethod]
    public void VolnaLinka_NajprvLinkaZDatBezOvladaca()
    {
        var config = Config(("Driver", 4, "COM1"));
        var tables = new[] { Table(0, "A", TableManufacturer.Elen, 4), Table(1, "B", TableManufacturer.Elen, 1) };

        Assert.AreEqual(4, DriverLines.FreeLine(DriverLines.Build(config, tables), tables));
        Assert.AreEqual(2, DriverLines.FreeLine(DriverLines.Build(config, []), []));
    }

    private static IReadOnlyList<LineTable> LineTableOf(DriverLineMap map, string section) => map.Lines.Single(l => l.Section == section).Tables;
}
