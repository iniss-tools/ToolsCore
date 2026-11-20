using ToolsCore.Iniss.Elen;
using ToolsCore.Iniss.Tables;

namespace ToolsCore.Tests.Elen;

/// <summary>
/// Text na tabuli ELEN po bodoch.
/// </summary>
[TestClass]
public class ElenTextTests
{
    /// <summary>
    /// Pismo vysky 2: znak 'A' sirky 3 (body v stlpci 0 a 2), 'B' sirky 2 (stlpec 1 dole), ostatne sirky 0;
    /// vysoka cislica 0x15 (za '0') sirky 4 s celym prvym stlpcom.
    /// </summary>
    private static ElenFont Font()
    {
        var glyphs = new ElenGlyph[ElenFont.GlyphCount];
        for (var i = 0; i < glyphs.Length; i++)
            glyphs[i] = new ElenGlyph(0, 2, 1, new byte[2]);
        glyphs['A'] = new ElenGlyph(3, 2, 1, [0b1010_0000, 0b1010_0000]);
        // bod za sirkou znaku (stlpec 3) sa nekresli
        glyphs['B'] = new ElenGlyph(2, 2, 1, [0b0001_0000, 0b0100_0000]);
        glyphs[0x15] = new ElenGlyph(4, 2, 1, [0b1000_0000, 0b1000_0000]);
        return new ElenFont("TEST", 8, 2, true, glyphs);
    }

    [TestMethod]
    public void Render_ZnakyVedlaSebaPodlaSirky()
    {
        var raster = ElenText.Render(Font(), "AB"u8);

        Assert.AreEqual(5, raster.Width);
        Assert.AreEqual(2, raster.Height);
        CollectionAssert.AreEqual(new[] { new ElenTextGlyph(0, (byte)'A', 0, 3), new ElenTextGlyph(1, (byte)'B', 3, 2) }, raster.Glyphs.ToArray());
        Assert.IsTrue(raster.IsSet(0, 0));
        Assert.IsTrue(raster.IsSet(2, 1));
        Assert.IsFalse(raster.IsSet(3, 0));
        Assert.IsTrue(raster.IsSet(4, 1));
        Assert.IsFalse(raster.IsSet(5, 1));
    }

    [TestMethod]
    public void Render_ZnakSoSirkou0_NezaberaMiestoAJeChybajuci()
    {
        var raster = ElenText.Render(Font(), "AxB"u8);

        Assert.AreEqual(5, raster.Width);
        Assert.IsTrue(raster.Glyphs[1].IsMissing);
        Assert.AreEqual(3, raster.Glyphs[1].X);
        Assert.AreEqual(2, raster.GlyphAt(3));
        Assert.AreEqual(0, raster.GlyphAt(2));
        Assert.AreEqual(-1, raster.GlyphAt(5));
    }

    [TestMethod]
    public void Render_VysokeCislice_NahradiaCisliceZnakmiOd0x14()
    {
        var raster = ElenText.Render(Font(), "0"u8, tallDigits: true);

        Assert.AreEqual(0x15, raster.Glyphs.Single().Code);
        Assert.AreEqual(4, raster.Width);
        Assert.IsTrue(raster.IsSet(0, 1));
    }

    [TestMethod]
    [DataRow((byte)' ', (byte)0x14)]
    [DataRow((byte)'0', (byte)0x15)]
    [DataRow((byte)'9', (byte)0x1E)]
    [DataRow((byte)':', (byte)0x1F)]
    [DataRow((byte)'A', (byte)'A')]
    public void TallDigitCode_MedzeraCisliceDvojbodka(byte code, byte expected)
    {
        Assert.AreEqual(expected, ElenText.TallDigitCode(code));
    }

    [TestMethod]
    [DataRow(0x42, 0)]
    [DataRow(0x52, 1)]
    [DataRow(0x62, 2)]
    [DataRow(0x72, 3)]
    [DataRow(0x8342, 3)]
    [DataRow(0x8952, 9)]
    public void FontIndex_RezAleboRozsirenePismo(int id, int index)
    {
        Assert.AreEqual(index, new ElenFontCode(id).FontIndex);
    }

    [TestMethod]
    public void FontFor_PismoMimoSuboru_Null()
    {
        var file = new ElenFontFile("F", [Font(), Font()]);

        Assert.AreSame(file.Fonts[1], ElenText.FontFor(file, new ElenFontCode(0x52)));
        Assert.IsNull(ElenText.FontFor(file, new ElenFontCode(0x62)));
    }

    [TestMethod]
    public void EncodingFor_KodKamenickychPodlaVyrobcu()
    {
        // Ž: Windows-1250 0x8E, kod Kamenickych 0x92
        Assert.AreEqual(0x92, ElenText.EncodingFor(TableManufacturer.Elen16Kam).GetBytes("Ž")[0]);
        Assert.AreEqual(0x92, ElenText.EncodingFor(TableManufacturer.Elen).GetBytes("Ž")[0]);
        Assert.AreEqual(0x8E, ElenText.EncodingFor(TableManufacturer.Elen16).GetBytes("Ž")[0]);
        Assert.AreEqual(0x8E, ElenText.EncodingFor(null).GetBytes("Ž")[0]);
    }
}
