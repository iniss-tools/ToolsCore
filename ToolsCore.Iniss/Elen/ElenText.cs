using ToolsCore.Iniss.Tables;
using ToolsCore.Iniss.Tools;

namespace ToolsCore.Iniss.Elen;

/// <summary>
/// Text na tabuli ELEN po bodoch tak, ako ho kresli panel: znaky pisma vedla seba, kazdy posunie text o svoju
/// sirku, kod bajtu sa neprevadza. Popis v docs <c>protokoly/elen.mdx</c> (Pisma a farby, Zarovnanie a sirka textu).
/// </summary>
public static class ElenText
{
    /// <summary>
    /// Kodovanie textu tabule podla vyrobcu: kod Kamenickych pri ELEN, ELENOLD a ELEN16Kam, inak Windows-1250
    /// (ELEN10, ELEN16, ELEKON a zoznam pisiem bez tabule).
    /// </summary>
    public static Encoding EncodingFor(TableManufacturer? manufacturer) =>
        manufacturer == TableManufacturer.Elen || manufacturer == TableManufacturer.Elenold || manufacturer == TableManufacturer.Elen16Kam
            ? Encodings.Kamenicky
            : Encodings.Win1250;

    /// <summary>
    /// Kod znaku, ktorym panel kresli znak textu pri pisme s vysokymi cislicami: medzera 0x14, cislice 0x15-0x1E,
    /// dvojbodka 0x1F; ostatne znaky ostanu.
    /// </summary>
    public static byte TallDigitCode(byte code) => code switch
    {
        >= (byte)'0' and <= (byte)'9' => (byte)(0x15 + code - '0'),
        (byte)' ' => 0x14,
        (byte)':' => 0x1F,
        _ => code
    };

    /// <summary>
    /// Pismo suboru, ktore vyberie cislo pisma (<see cref="ElenFontCode.FontIndex" />); null = v subore nie je.
    /// </summary>
    public static ElenFont? FontFor(ElenFontFile file, ElenFontCode code)
    {
        ArgumentNullException.ThrowIfNull(file);
        var index = code.FontIndex;
        return index < file.Fonts.Count ? file.Fonts[index] : null;
    }

    /// <summary>
    /// Vykresli kody textu pismom <paramref name="font" /> do riadku vysky pisma.
    /// </summary>
    /// <param name="font">pismo</param>
    /// <param name="codes">kody znakov (text v kodovani tabule)</param>
    /// <param name="tallDigits">medzera, cislice a dvojbodka vysokymi cislicami pisma</param>
    public static ElenTextRaster Render(ElenFont font, ReadOnlySpan<byte> codes, bool tallDigits = false)
    {
        ArgumentNullException.ThrowIfNull(font);
        var glyphs = new ElenTextGlyph[codes.Length];
        var width = 0;
        for (var i = 0; i < codes.Length; i++)
        {
            var code = tallDigits ? TallDigitCode(codes[i]) : codes[i];
            var glyphWidth = font.Glyphs[code].Width;
            glyphs[i] = new ElenTextGlyph(i, code, width, glyphWidth);
            width += glyphWidth;
        }

        var height = font.Height;
        var bits = new bool[width * height];
        foreach (var item in glyphs)
        {
            var glyph = font.Glyphs[item.Code];
            // body za sirkou znaku sa nekreslia ako v simulatore tabul (v znamych pismach su zhasnute)
            var columns = Math.Min(item.Width, glyph.BytesPerRow * 8);
            for (var y = 0; y < Math.Min(height, glyph.Height); y++)
            {
                for (var x = 0; x < columns; x++)
                {
                    if (glyph.IsSet(x, y))
                        bits[y * width + item.X + x] = true;
                }
            }
        }

        return new ElenTextRaster(width, height, bits, glyphs);
    }
}
