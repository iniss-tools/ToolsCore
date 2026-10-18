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

    [TestMethod]
    public void For_MriezkaPodlaPozadiaBuniekAOdznak()
    {
        using var grid = new DataGridView();
        grid.DefaultCellStyle.BackColor = Color.FromArgb(43, 45, 48);
        using var label = new Label { BackColor = Color.White };

        Assert.AreSame(AppIcons.Get("status-error", null, 16, true), IconImages.For(grid, "status-error"));
        Assert.AreSame(AppIcons.Get("train", "add", 16, false), IconImages.For(label, "train+add"));
        Assert.AreEqual(new Size(32, 32), IconImages.For(label, "status-info", IconImages.DialogSize)!.Size);
    }

    [TestMethod]
    public void ForMessage_IkonyDialogu()
    {
        using var label = new Label { BackColor = Color.White };

        Assert.AreSame(AppIcons.Get("status-warning", null, 32, false), IconImages.ForMessage(MessageBoxIcon.Warning, label));
        Assert.AreSame(AppIcons.Get("status-question", null, 32, false), IconImages.ForMessage(MessageBoxIcon.Question, label));
        Assert.IsNull(IconImages.ForMessage(MessageBoxIcon.None, label));
    }

    [TestMethod]
    public void ErrorIcon_IkonaZBankyZVyrovnavaciejPamate()
    {
        using var label = new Label { BackColor = Color.White };

        var icon = IconImages.ErrorIcon(label);

        Assert.AreEqual(new Size(16, 16), icon.Size);
        Assert.AreSame(icon, IconImages.ErrorIcon(label));
    }
}
