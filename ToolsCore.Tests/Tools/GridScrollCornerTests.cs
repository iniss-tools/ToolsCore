using System.Globalization;
using ToolsCore.Tools;

namespace ToolsCore.Tests.Tools;

/// <summary>
/// Roh medzi posuvnikmi tabulky: pri tmavych posuvnikoch ma farbu ich drahy, nie systemovu svetlu.
/// </summary>
[STATestClass]
public class GridScrollCornerTests
{
    // tabulka s viac stlpcami a riadkami, nez sa zmesti - ma oba posuvniky
    private static DataGridView Grid()
    {
        var grid = new DataGridView { Size = new Size(220, 140), RowHeadersVisible = false, AllowUserToAddRows = false };
        for (var i = 0; i < 8; i++)
            grid.Columns.Add("c" + i.ToString(CultureInfo.InvariantCulture), "Stlpec");
        grid.Rows.Add(30);
        grid.CreateControl();
        grid.PerformLayout();
        return grid;
    }

    private static Color CornerColor(DataGridView grid)
    {
        var corner = GridScrollCorner.Corner(grid);
        Assert.IsNotNull(corner, "tabulka ma mat oba posuvniky");
        using var bitmap = new Bitmap(grid.Width, grid.Height);
        grid.DrawToBitmap(bitmap, new Rectangle(Point.Empty, grid.Size));
        return bitmap.GetPixel(corner.Value.X + corner.Value.Width / 2, corner.Value.Y + corner.Value.Height / 2);
    }

    [TestMethod]
    public void Roh_TmavePosuvnikyMajuRohFarbyDrahy()
    {
        using var grid = Grid();

        GridScrollCorner.Apply(grid, true);

        Assert.AreEqual(GridScrollCorner.DarkTrack.ToArgb(), CornerColor(grid).ToArgb());
    }

    [TestMethod]
    public void Roh_PoPrepnutiNaSvetlePosuvnikySystemovaFarba()
    {
        using var grid = Grid();

        GridScrollCorner.Apply(grid, true);
        GridScrollCorner.Apply(grid, false);

        Assert.AreEqual(SystemColors.Control.ToArgb(), CornerColor(grid).ToArgb());
    }
}
