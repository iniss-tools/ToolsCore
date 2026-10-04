using ToolsCore.Iniss.Elen;

namespace ToolsCore.Tests.Elen;

/// <summary>
/// Subor pisiem tabul ELEN (.bin).
/// </summary>
[TestClass]
public class ElenFontFileTests
{
    private static ElenFont Font(string name, int maxWidth, int height, bool proportional, byte fill = 0)
    {
        var bytesPerRow = ElenFont.BytesPerRowFor(maxWidth);
        var glyphs = new ElenGlyph[ElenFont.GlyphCount];
        for (var i = 0; i < glyphs.Length; i++)
        {
            var data = new byte[height * bytesPerRow];
            for (var b = 0; b < data.Length; b++)
                data[b] = (byte)(fill + i + b);
            glyphs[i] = new ElenGlyph(proportional ? i % (maxWidth + 1) : maxWidth, height, bytesPerRow, data);
        }

        return new ElenFont(name, maxWidth, height, proportional, glyphs);
    }

    private static byte[] Bytes(ElenFontFile file)
    {
        using var stream = new MemoryStream();
        file.Write(stream);
        return stream.ToArray();
    }

    [TestMethod]
    public void Zapis_PoNacitani_RovnakeBajty()
    {
        var file = new ElenFontFile("10_1250_test", [Font("FIX_10_9", 6, 10, false), Font("PROP_10_9", 12, 10, true, 7)]);
        var bytes = Bytes(file);

        var read = ElenFontFile.Read(new MemoryStream(bytes));

        Assert.AreEqual("10_1250_test", read.Name);
        Assert.HasCount(2, read.Fonts);
        Assert.AreEqual("PROP_10_9", read.Fonts[1].Name);
        Assert.AreEqual(12, read.Fonts[1].MaxWidth);
        Assert.AreEqual(2, read.Fonts[1].BytesPerRow);
        Assert.IsTrue(read.Fonts[1].IsProportional);
        CollectionAssert.AreEqual(bytes, Bytes(read));
    }

    [TestMethod]
    public void Hlavicka_PosunyAVolneMiesta()
    {
        var bytes = Bytes(new ElenFontFile("f", [Font("A", 6, 10, false), Font("B", 6, 10, false)]));

        // prve pismo hned za hlavickou (64), druhe za nim, ostatne miesta volne (64); na konci bajt 'a'
        const int fontSize = 20 + 3 + 256 * (1 + 10);
        Assert.AreEqual(64, bytes[0] << 8 | bytes[1]);
        Assert.AreEqual(64 + fontSize, bytes[2] << 8 | bytes[3]);
        Assert.AreEqual(64, bytes[4] << 8 | bytes[5]);
        Assert.AreEqual(64 + 2 * fontSize + 1, bytes.Length);
        Assert.AreEqual((byte)'a', bytes[^1]);
    }

    [TestMethod]
    public void Nacitanie_JedinePismo_AjPrazdnySubor()
    {
        var single = Bytes(new ElenFontFile("f", [Font("A", 6, 8, false)]));
        var empty = Bytes(new ElenFontFile("f", []));

        Assert.HasCount(1, ElenFontFile.Read(new MemoryStream(single)).Fonts);
        Assert.IsEmpty(ElenFontFile.Read(new MemoryStream(empty)).Fonts);
    }

    [TestMethod]
    public void Nacitanie_DuplicitnyNazovPisma_Preskoci()
    {
        var bytes = Bytes(new ElenFontFile("f", [Font("A", 6, 8, false), Font("A", 6, 8, false, 3), Font("C", 6, 8, false)]));

        var read = ElenFontFile.Read(new MemoryStream(bytes));

        CollectionAssert.AreEqual(new[] { "A", "C" }, read.Fonts.Select(f => f.Name).ToArray());
    }

    [TestMethod]
    public void Prisny_SirkaZnakuNeproporcionalneho_Chyba()
    {
        var font = Font("A", 6, 8, false);
        var glyphs = font.Glyphs.ToArray();
        glyphs[65] = new ElenGlyph(5, 8, 1, new byte[8]);
        var bytes = Bytes(new ElenFontFile("f", [new ElenFont("A", 6, 8, false, glyphs)]));

        Assert.AreEqual(5, ElenFontFile.Read(new MemoryStream(bytes)).Fonts[0].Glyphs[65].Width);
        Assert.ThrowsExactly<FormatException>(() => ElenFontFile.Read(new MemoryStream(bytes), true));
    }

    [TestMethod]
    public void SkratenySubor_ZvysneZnakyPrazdneAleboChyba()
    {
        var bytes = Bytes(new ElenFontFile("f", [Font("A", 6, 8, false, 1), Font("B", 6, 8, false)]));
        // koniec v strede pisma A - za znakom 100
        var cut = bytes[..(64 + 23 + 100 * 9 + 4)];

        var read = ElenFontFile.Read(new MemoryStream(cut));

        Assert.HasCount(1, read.Fonts);
        Assert.AreEqual(6, read.Fonts[0].Glyphs[99].Width);
        Assert.AreEqual(0, read.Fonts[0].Glyphs[200].Width);
        Assert.ThrowsExactly<EndOfStreamException>(() => ElenFontFile.Read(new MemoryStream(cut), true));
    }

    [TestMethod]
    public void Znak_BodyOdNajvyssiehoBitu()
    {
        // 9 bodov na sirku = 2 bajty na riadok; riadok 1: 1000_0000 0100_0000
        var glyph = new ElenGlyph(9, 2, 2, [0x00, 0x00, 0x80, 0x40]);

        Assert.IsTrue(glyph.IsSet(0, 1));
        Assert.IsTrue(glyph.IsSet(9, 1));
        Assert.IsFalse(glyph.IsSet(1, 1));
        Assert.IsFalse(glyph.IsSet(0, 0));
        Assert.IsFalse(glyph.IsSet(0, 2));
        Assert.AreEqual(0x40, glyph[1, 1]);
    }

    [TestMethod]
    public void Zapis_PrilisVelaPisiem_Chyba()
    {
        var fonts = Enumerable.Range(0, 17).Select(i => Font($"F{i}", 6, 1, false)).ToArray();

        Assert.ThrowsExactly<FormatException>(() => Bytes(new ElenFontFile("f", fonts)));
    }
}
