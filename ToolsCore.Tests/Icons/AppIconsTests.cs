using ToolsCore.Icons;

namespace ToolsCore.Tests.Icons;

/// <summary>
/// Banka ikon SVG: vykreslenie v presnej velkosti, tmavy variant a odznak.
/// </summary>
[TestClass]
public class AppIconsTests
{
    [TestMethod]
    public void Names_ObsahujuIkonyAOdznakyBezVariantov()
    {
        CollectionAssert.Contains(AppIcons.Names.ToList(), "save");
        CollectionAssert.Contains(AppIcons.Names.ToList(), "train");
        CollectionAssert.Contains(AppIcons.Badges.ToList(), "add");
        Assert.IsFalse(AppIcons.Names.Any(n => n.EndsWith("_dark", StringComparison.Ordinal)));
    }

    [TestMethod]
    [DataRow(16)]
    [DataRow(20)]
    [DataRow(30)]
    public void Get_VykresliIkonuVPresnejVelkosti(int size)
    {
        var icon = AppIcons.Get("save", null, size, false)!;

        Assert.AreEqual(new Size(size, size), icon.Size);
        Assert.IsTrue(Pixels(icon).Any(c => c.A > 128));
    }

    [TestMethod]
    [DataRow(16, 16, 1)]
    [DataRow(20, 20, 1)]
    [DataRow(25, 24, 1)]
    [DataRow(30, 28, 1)]
    [DataRow(32, 32, 1)]
    [DataRow(40, 20, 2)]
    [DataRow(48, 24, 2)]
    [DataRow(64, 32, 2)]
    [DataRow(12, 16, 0)]
    public void Choose_NajvacsiaKresbaAleboJejNasobok(int size, int design, int scale) =>
        Assert.AreEqual((design, scale), AppIcons.Choose([16, 20, 24, 28, 32], size));

    [TestMethod]
    public void Choose_ChybajucaKresba_NasobokMensej() => Assert.AreEqual((16, 2), AppIcons.Choose([16, 20, 24], 32));

    [TestMethod]
    public void Get_MensiaKresba_VystredenaNaCeleBody()
    {
        // 25 bodov = kresba 24 bez zvacsenia, posledny stlpec a riadok ostanu prazdne
        var icon = AppIcons.Get("save", null, 25, false)!;
        var drawn = AppIcons.Get("save", null, 24, false)!;

        Assert.AreEqual(new Size(25, 25), icon.Size);
        for (var i = 0; i < 25; i++)
            Assert.IsTrue(icon.GetPixel(24, i).A == 0 && icon.GetPixel(i, 24).A == 0);
        Assert.AreEqual(drawn.GetPixel(4, 12), icon.GetPixel(4, 12));
    }

    [TestMethod]
    public void Get_TmavyVariant_JeSvetlejsi()
    {
        var light = AppIcons.Get("settings", null, 16, false)!;
        var dark = AppIcons.Get("settings", null, 16, true)!;

        Assert.IsTrue(Brightness(dark) > Brightness(light) + 50);
    }

    [TestMethod]
    public void Get_DvojfarebnaIkona_ObrysAVyplnSNizkymKrytim()
    {
        // priecinok: zlaty obrys a vnutro v tej istej farbe s krytim 0.3
        var pixels = Pixels(AppIcons.Get("folder", null, 16, false)!);

        Assert.IsTrue(pixels.Any(c => c.A == 255 && c.R > c.B + 60));
        Assert.IsTrue(pixels.Count(c => c.A is > 60 and < 100) > 20);
    }

    [TestMethod]
    public void Get_Odznak_KresliSaVPravomDolnomRohuSMedzerou()
    {
        var plain = AppIcons.Get("train", null, 16, false)!;
        var badged = AppIcons.Get("train", "delete", 16, false)!;

        // stred odznaku je cerveny kriz
        var center = badged.GetPixel(12, 12);
        Assert.IsTrue(center.R > 180 && center.G < 120, center.ToString());
        // vlak sa v medzere okolo odznaku vymaze, mimo rohu ostane
        var cleared = 0;
        for (var y = 7; y < 16; y++)
        for (var x = 7; x < 16; x++)
            if (plain.GetPixel(x, y).A == 255 && badged.GetPixel(x, y).A == 0)
                cleared++;
        Assert.IsTrue(cleared > 0);
        Assert.AreEqual(plain.GetPixel(4, 4), badged.GetPixel(4, 4));
    }

    [TestMethod]
    public void Get_RovnakeParametre_VratiObrazokZVyrovnavaciejPamate()
    {
        Assert.AreSame(AppIcons.Get("folder", "add", 20, false), AppIcons.Get("folder", "add", 20, false));
        Assert.AreNotSame(AppIcons.Get("folder", null, 20, false), AppIcons.Get("folder", null, 20, true));
    }

    [TestMethod]
    public void Get_NeznamaIkonaAleboOdznak()
    {
        Assert.IsNull(AppIcons.Get("neexistuje", null, 16, false));
        // neznamy odznak sa vynecha
        CollectionAssert.AreEqual(Pixels(AppIcons.Get("save", null, 16, false)!), Pixels(AppIcons.Get("save", "neexistuje", 16, false)!));
    }

    [TestMethod]
    [DataRow(255, 255, 255, false)]
    [DataRow(240, 240, 240, false)]
    [DataRow(43, 45, 48, true)]
    [DataRow(0, 0, 0, true)]
    public void IsDarkBackground_PodlaJasu(int r, int g, int b, bool dark) => Assert.AreEqual(dark, AppIcons.IsDarkBackground(Color.FromArgb(r, g, b)));

    [TestMethod]
    public void IsDarkBackground_PriehladnaFarba_NieJeTmava() => Assert.IsFalse(AppIcons.IsDarkBackground(Color.Transparent));

    private static Color[] Pixels(Bitmap bitmap)
    {
        var pixels = new Color[bitmap.Width * bitmap.Height];
        for (var y = 0; y < bitmap.Height; y++)
        for (var x = 0; x < bitmap.Width; x++)
            pixels[y * bitmap.Width + x] = bitmap.GetPixel(x, y);
        return pixels;
    }

    private static double Brightness(Bitmap bitmap) => Pixels(bitmap).Where(c => c.A > 128).Average(c => (c.R + c.G + c.B) / 3.0);
}
