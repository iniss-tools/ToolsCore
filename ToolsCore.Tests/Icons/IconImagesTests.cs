using ToolsCore.Icons;

namespace ToolsCore.Tests.Icons;

/// <summary>
/// Zoznamy obrazkov s ikonami z banky - kluce su nazvy ikon.
/// </summary>
[TestClass]
public class IconImagesTests
{
    [TestMethod]
    public void Fill_KluceVPoradiAOdznak()
    {
        using var list = new ImageList();
        list.Images.Add("stary", new Bitmap(8, 8));

        IconImages.Fill(list, 20, false, ["save", "train+add", "neexistuje", "save"]);

        Assert.AreEqual(new Size(20, 20), list.ImageSize);
        CollectionAssert.AreEqual(new[] { "save", "train+add" }, list.Images.Keys.Cast<string>().ToArray());
    }
}
