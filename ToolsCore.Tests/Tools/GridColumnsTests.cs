using ToolsCore.Tools;
using ToolsCore.XML;

namespace ToolsCore.Tests.Tools;

/// <summary>
/// Zapamatane stlpce tabulky: poradie, viditelnost a sirka z konfiguracie, predvolene skryte stlpce.
/// </summary>
[TestClass]
public class GridColumnsTests
{
    [TestMethod]
    public void Attach_UlozeneUsporiadanie_PouzijePoradieViditelnostASirku()
    {
        using var grid = CreateGrid();
        var config = new ConfigBase();
        config.SetGrid("Test", new GridLayout
        {
            Id = "Test",
            Columns =
            [
                new GridColumnLayout { Name = "cC", Order = 0 },
                new GridColumnLayout { Name = "cA", Order = 1, Width = 120 },
                new GridColumnLayout { Name = "cB", Order = 2, Visible = false },
                new GridColumnLayout { Name = "cNeexistuje", Order = 3 }
            ]
        });
        var saves = 0;

        var columns = GridColumns.Attach(grid, "Test", () => config, () => saves++);

        Assert.AreEqual(0, grid.Columns["cC"]!.DisplayIndex);
        Assert.AreEqual(1, grid.Columns["cA"]!.DisplayIndex);
        Assert.IsFalse(grid.Columns["cB"]!.Visible);
        Assert.AreEqual(grid.LogicalToDeviceUnits(120), grid.Columns["cA"]!.Width);
        Assert.IsTrue(columns.HasUserWidth(grid.Columns["cA"]!));
        Assert.IsFalse(columns.HasUserWidth(grid.Columns["cC"]!));
        Assert.IsTrue(grid.AllowUserToOrderColumns);
        Assert.AreEqual(0, saves, "pouzitie ulozeneho usporiadania nic nezapisuje");
    }

    [TestMethod]
    public void Attach_BezUlozenia_PredvoleneSkryteAPoslednyVyplna()
    {
        using var grid = CreateGrid();

        GridColumns.Attach(grid, "Test", () => new ConfigBase(), () => { }, "cB");

        Assert.IsFalse(grid.Columns["cB"]!.Visible);
        Assert.AreEqual(DataGridViewAutoSizeColumnMode.Fill, grid.Columns["cC"]!.AutoSizeMode);
        Assert.AreEqual(DataGridViewAutoSizeColumnMode.None, grid.Columns["cA"]!.AutoSizeMode,
            "automaticka sirka z navrhu sa nastavi raz, aby sa stlpec dal rozsirit tahanim");
    }

    [TestMethod]
    public void Attach_VsetkyStlpceSkryte_UkazePredvolene()
    {
        using var grid = CreateGrid();
        var config = new ConfigBase();
        config.SetGrid("Test", new GridLayout
        {
            Id = "Test",
            Columns = new[] { "cA", "cB", "cC" }.Select((n, i) => new GridColumnLayout { Name = n, Order = i, Visible = false }).ToList()
        });

        GridColumns.Attach(grid, "Test", () => config, () => { });

        Assert.IsTrue(grid.Columns.Cast<DataGridViewColumn>().All(c => c.Visible));
    }

    private static DataGridView CreateGrid()
    {
        var grid = new DataGridView();
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "cA", HeaderText = "A", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "cB", HeaderText = "B" });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "cC", HeaderText = "C" });
        return grid;
    }
}
