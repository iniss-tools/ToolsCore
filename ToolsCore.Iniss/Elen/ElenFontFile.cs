using System.Globalization;
using ToolsCore.Iniss.Properties;

namespace ToolsCore.Iniss.Elen;

/// <summary>
/// Subor pisiem tabul ELEN (.bin) - rovnaky format pouzivaju tabule, INISS2, ElenFontEditor aj ELEN Monitor.
/// </summary>
/// <remarks>
/// Hlavicka: 16 posunov pisiem (ushort big-endian; 64 = volne miesto, okrem prveho pisma), 32 B nazov suboru.
/// Pismo: 20 B nazov, sirka, vyska, proporcionalne (po 1 B), potom 256 × (sirka znaku + vyska × bajtov na riadok).
/// Za poslednym pismom nasleduje bajt 'a'. Format popisuje docs <c>formaty-suborov/pisma-elen.mdx</c>.
/// </remarks>
public sealed class ElenFontFile
{
    /// <summary>Najviac pisiem v subore.</summary>
    public const int MaxFonts = 16;

    /// <summary>Najvacsi subor, ktory INISS2 nacita.</summary>
    public const int MaxFileSize = 131_071;

    private const int HeaderSize = MaxFonts * 2 + FileNameSize;
    private const int FileNameSize = 32;
    private const int FontNameSize = 20;
    private const int FontHeaderSize = FontNameSize + 3;
    private const byte Terminator = (byte)'a';

    /// <param name="name">nazov suboru zapisany v hlavicke (najviac 32 znakov ASCII)</param>
    /// <param name="fonts">pisma, najviac 16</param>
    public ElenFontFile(string name, IReadOnlyList<ElenFont> fonts)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(fonts);
        Name = name;
        Fonts = fonts;
    }

    /// <summary>Nazov suboru zapisany v hlavicke.</summary>
    public string Name { get; }

    /// <summary>Pisma v poradi suboru.</summary>
    public IReadOnlyList<ElenFont> Fonts { get; }

    /// <summary>
    /// Nacita subor pisiem.
    /// </summary>
    /// <param name="path">cesta k suboru .bin</param>
    /// <param name="strict">
    /// prisna kontrola ako INISS2 (dlzka suboru, nazov v hlavicke = meno suboru s rozlisenim velkosti pismen) a celistvost
    /// suboru - vid <see cref="Read" />
    /// </param>
    public static ElenFontFile Load(string path, bool strict = false)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        if (!File.Exists(path))
            throw new FileNotFoundException(Text(Resources.ElenFont_FileNotFound, path), path);

        using var stream = File.OpenRead(path);
        var file = Read(stream, strict);

        var expected = Path.GetFileNameWithoutExtension(path);
        if (strict && file.Name != expected)
            throw new FormatException(Text(Resources.ElenFont_NameMismatch, file.Name, expected));

        return file;
    }

    /// <summary>
    /// Precita subor pisiem z prudu, ktory sa da posuvat. Pisma zostanu na svojich miestach - poradie je cislo pisma,
    /// ktore tabuli posiela INISS, preto sa ziadne pismo nevynecha (ani rovnomenne).
    /// </summary>
    /// <remarks>
    /// Volne citanie nacita, co sa da: skrateny subor doplni prazdnymi znakmi (ako ich vidi panel). Prisne citanie
    /// prijme len subor, ktory INISS2 nacita a ktory je cely: dlzka 65 B az <see cref="MaxFileSize" />, aspon jedno
    /// pismo, pisma za sebou od konca hlavicky bez medzier a volnych miest medzi nimi, nazvy v ASCII, sirka pisma
    /// 1 az <see cref="ElenFont.MaxSupportedWidth" />, nenulova vyska, priznak proporcneho pisma 0 alebo 1 a za
    /// poslednym pismom len bajt 'a'. Sirky znakov sa nekontroluju - v suboroch vyrobcu byvaju vacsie ako sirka pisma
    /// (riadiace kody so sirkou 24) a neproporcne pismo ich nepouziva.
    /// </remarks>
    /// <param name="stream">prud</param>
    /// <param name="strict">prisna kontrola celistvosti suboru</param>
    public static ElenFontFile Read(Stream stream, bool strict = false)
    {
        ArgumentNullException.ThrowIfNull(stream);
        var length = stream.Length;
        if (strict && length <= HeaderSize)
            throw new FormatException(Text(Resources.ElenFont_TooShort, length));
        if (strict && length > MaxFileSize)
            throw new FormatException(Text(Resources.ElenFont_TooLong, length, MaxFileSize));

        using var reader = new BinaryReader(stream, Encoding.ASCII, true);

        var positions = new ushort[MaxFonts];
        for (var i = 0; i < positions.Length; i++)
            positions[i] = (ushort)((reader.ReadByte() << 8) | reader.ReadByte());

        var name = ReadText(reader.ReadBytes(FileNameSize));
        var fonts = new List<ElenFont>();
        // koniec hlavicky alebo predchadzajuceho pisma - tam ma zacinat dalsie pismo
        long end = HeaderSize;

        for (var i = 0; i < positions.Length; i++)
        {
            // 64 = hned za hlavickou: prve pismo, inak volne miesto; prve miesto je volne, len ak za hlavickou
            // nie je nic okrem koncoveho bajtu
            if (positions[i] == HeaderSize && (i != 0 || length <= HeaderSize + 1))
            {
                if (strict && Array.FindIndex(positions, i, p => p != HeaderSize) is var used and >= 0)
                    throw new FormatException(Text(Resources.ElenFont_SlotAfterFree, used + 1, i + 1));
                break;
            }

            if (strict && positions[i] != end)
                throw new FormatException(Text(Resources.ElenFont_BadStart, i + 1, positions[i], end));
            if (positions[i] + FontHeaderSize > length)
            {
                if (strict)
                    throw new EndOfStreamException(Text(Resources.ElenFont_StartBeyondEnd, i + 1));
                break;
            }

            stream.Position = positions[i];
            var nameBytes = reader.ReadBytes(FontNameSize);
            var fontName = ReadText(nameBytes);
            int maxWidth = reader.ReadByte();
            int height = reader.ReadByte();
            int proportional = reader.ReadByte();
            if (strict)
                CheckFont(i, nameBytes, fontName, maxWidth, height, proportional);

            var bytesPerRow = ElenFont.BytesPerRowFor(maxWidth);
            var glyphs = new ElenGlyph[ElenFont.GlyphCount];
            var truncated = false;
            for (var j = 0; j < glyphs.Length; j++)
            {
                // skrateny subor: v prisnom rezime chyba, inak zvysne znaky prazdne (ako ich vidi panel)
                if (truncated || stream.Position >= length)
                {
                    if (strict)
                        throw new EndOfStreamException(Text(Resources.ElenFont_EndsInsideSymbol, fontName, j));
                    truncated = true;
                    glyphs[j] = new ElenGlyph(0, height, bytesPerRow, new byte[height * bytesPerRow]);
                    continue;
                }

                int width = reader.ReadByte();
                var data = reader.ReadBytes(height * bytesPerRow);
                if (data.Length != height * bytesPerRow)
                {
                    if (strict)
                        throw new EndOfStreamException(Text(Resources.ElenFont_EndsInsideSymbol, fontName, j));
                    truncated = true;
                    Array.Resize(ref data, height * bytesPerRow);
                }

                glyphs[j] = new ElenGlyph(width, height, bytesPerRow, data);
            }

            fonts.Add(new ElenFont(fontName, maxWidth, height, proportional != 0, glyphs));
            end = stream.Position;
            if (truncated)
                break;
        }

        if (strict && fonts.Count == 0)
            throw new FormatException(Resources.ElenFont_NoFonts);
        if (strict && (length != end + 1 || ReadByteAt(stream, end) != Terminator))
            throw new FormatException(Text(Resources.ElenFont_NoTerminator, end));

        return new ElenFontFile(name, fonts);
    }

    /// <summary>
    /// Prisna kontrola hlavicky pisma: nazov v ASCII, sirka, s ktorou pocita INISS2, nenulova vyska a priznak 0 alebo 1.
    /// </summary>
    private static void CheckFont(int index, byte[] nameBytes, string name, int maxWidth, int height, int proportional)
    {
        var nameEnd = Array.IndexOf(nameBytes, (byte)0);
        if (name.Length == 0 || nameBytes.AsSpan(0, nameEnd < 0 ? nameBytes.Length : nameEnd).ContainsAnyExceptInRange((byte)' ', (byte)'~'))
            throw new FormatException(Text(Resources.ElenFont_InvalidName, index + 1));
        if (maxWidth is < 1 or > ElenFont.MaxSupportedWidth)
            throw new FormatException(Text(Resources.ElenFont_BadWidth, name, maxWidth, ElenFont.MaxSupportedWidth));
        if (height < 1)
            throw new FormatException(Text(Resources.ElenFont_ZeroHeight, name));
        if (proportional is not (0 or 1))
            throw new FormatException(Text(Resources.ElenFont_BadProportional, name, proportional));
    }

    private static int ReadByteAt(Stream stream, long position)
    {
        stream.Position = position;
        return stream.ReadByte();
    }

    /// <summary>
    /// Ulozi subor pisiem (prepise cely subor). Pri chybe obsahu ostane povodny subor nedotknuty.
    /// </summary>
    public void Save(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        using var buffer = new MemoryStream();
        Write(buffer);
        File.WriteAllBytes(path, buffer.ToArray());
    }

    /// <summary>
    /// Zapise subor pisiem do prudu.
    /// </summary>
    public void Write(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (Fonts.Count > MaxFonts)
            throw new FormatException(Text(Resources.ElenFont_TooManyFonts, MaxFonts));

        // posuny pisiem sa daju vypocitat vopred - zapis nepotrebuje posuvat prud
        var positions = new ushort[MaxFonts];
        Array.Fill(positions, (ushort)HeaderSize);
        var position = HeaderSize;
        for (var i = 0; i < Fonts.Count; i++)
        {
            if (position > ushort.MaxValue)
                throw new FormatException(Text(Resources.ElenFont_Beyond64K, Fonts[i].Name));
            positions[i] = (ushort)position;
            position += FontNameSize + 3 + ElenFont.GlyphCount * (1 + Fonts[i].Height * Fonts[i].BytesPerRow);
        }

        using var writer = new BinaryWriter(stream, Encoding.ASCII, true);
        foreach (var pos in positions)
        {
            writer.Write((byte)(pos >> 8));
            writer.Write((byte)pos);
        }

        writer.Write(WriteText(Name, FileNameSize));
        foreach (var font in Fonts)
        {
            writer.Write(WriteText(font.Name, FontNameSize));
            writer.Write((byte)font.MaxWidth);
            writer.Write((byte)font.Height);
            writer.Write(font.IsProportional);

            foreach (var glyph in font.Glyphs)
            {
                if (glyph.Height != font.Height || glyph.BytesPerRow != font.BytesPerRow)
                    throw new FormatException(Text(Resources.ElenFont_GlyphSize, font.Name));
                writer.Write((byte)glyph.Width);
                writer.Write(glyph.Data);
            }
        }

        writer.Write(Terminator);
    }

    /// <summary>Text chyby pre obsluhu v jazyku programu.</summary>
    private static string Text(string format, params object[] args) => string.Format(CultureInfo.CurrentCulture, format, args);

    private static string ReadText(byte[] bytes)
    {
        var end = Array.IndexOf(bytes, (byte)0);
        return Encoding.ASCII.GetString(bytes, 0, end < 0 ? bytes.Length : end).TrimEnd();
    }

    private static byte[] WriteText(string text, int size)
    {
        ArgumentException.ThrowIfNullOrEmpty(text);
        if (text.Length > size)
            throw new ArgumentException(Text(Resources.ElenFont_TextTooLong, text, size), nameof(text));

        var bytes = new byte[size];
        Encoding.ASCII.GetBytes(text, bytes);
        return bytes;
    }
}
