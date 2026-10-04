namespace ToolsCore.Iniss.Elen;

/// <summary>
/// Znak pisma ELEN - bitova mapa po riadkoch, v kazdom riadku <see cref="ElenFont.BytesPerRow" /> bajtov,
/// najvyssi bit prveho bajtu je lavy bod.
/// </summary>
public sealed class ElenGlyph
{
    private readonly byte[] _data;

    /// <param name="width">sirka znaku v bodoch (0 = znak v pisme nie je)</param>
    /// <param name="height">vyska v bodoch (vyska pisma)</param>
    /// <param name="bytesPerRow">bajtov na riadok</param>
    /// <param name="data">bitova mapa, <paramref name="height" /> × <paramref name="bytesPerRow" /> bajtov</param>
    public ElenGlyph(int width, int height, int bytesPerRow, byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (data.Length != height * bytesPerRow)
            throw new ArgumentException($"Glyph data must have {height * bytesPerRow} bytes, not {data.Length}.", nameof(data));

        Width = width;
        Height = height;
        BytesPerRow = bytesPerRow;
        _data = data;
    }

    /// <summary>Sirka znaku v bodoch.</summary>
    public int Width { get; }

    /// <summary>Vyska znaku v bodoch.</summary>
    public int Height { get; }

    /// <summary>Bajtov na riadok bitovej mapy.</summary>
    public int BytesPerRow { get; }

    /// <summary>Bitova mapa tak, ako je v subore.</summary>
    public ReadOnlySpan<byte> Data => _data;

    /// <summary>Bajt bitovej mapy v riadku <paramref name="row" /> a stlpci bajtov <paramref name="column" />.</summary>
    public byte this[int row, int column] => _data[row * BytesPerRow + column];

    /// <summary>Ci svieti bod <paramref name="x" />, <paramref name="y" /> (0, 0 = lavy horny).</summary>
    public bool IsSet(int x, int y) =>
        x >= 0 && y >= 0 && y < Height && x < BytesPerRow * 8 && (_data[y * BytesPerRow + (x >> 3)] & (0x80 >> (x & 7))) != 0;
}

/// <summary>
/// Pismo ELEN - jedno z najviac 16 pisiem suboru .bin, vzdy 256 znakov.
/// </summary>
public sealed class ElenFont
{
    /// <summary>Pocet znakov v pisme.</summary>
    public const int GlyphCount = 256;

    /// <param name="name">nazov pisma (najviac 20 znakov ASCII)</param>
    /// <param name="maxWidth">najvacsia sirka znaku v bodoch</param>
    /// <param name="height">vyska v bodoch</param>
    /// <param name="isProportional">proporcionalne pismo</param>
    /// <param name="glyphs">256 znakov</param>
    public ElenFont(string name, int maxWidth, int height, bool isProportional, IReadOnlyList<ElenGlyph> glyphs)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(glyphs);
        if (glyphs.Count != GlyphCount)
            throw new ArgumentException($"Font must have {GlyphCount} glyphs, not {glyphs.Count}.", nameof(glyphs));

        Name = name;
        MaxWidth = maxWidth;
        Height = height;
        IsProportional = isProportional;
        Glyphs = glyphs;
    }

    /// <summary>Nazov pisma.</summary>
    public string Name { get; }

    /// <summary>Najvacsia sirka znaku v bodoch.</summary>
    public int MaxWidth { get; }

    /// <summary>Vyska v bodoch.</summary>
    public int Height { get; }

    /// <summary>Proporcionalne pismo.</summary>
    public bool IsProportional { get; }

    /// <summary>Bajtov na riadok bitovej mapy znaku.</summary>
    public int BytesPerRow => BytesPerRowFor(MaxWidth);

    /// <summary>Znaky podla kodu bajtu, ktory prisiel tabuli.</summary>
    public IReadOnlyList<ElenGlyph> Glyphs { get; }

    /// <summary>Bajtov na riadok bitovej mapy pre sirku <paramref name="maxWidth" />.</summary>
    public static int BytesPerRowFor(int maxWidth) => (maxWidth + 7) / 8;

    /// <inheritdoc />
    public override string ToString() => Name;
}

/// <summary>
/// Subor pisiem tabul ELEN (.bin) - rovnaky format pouzivaju tabule, ElenFontEditor aj ELEN Monitor.
/// </summary>
/// <remarks>
/// Hlavicka: 16 posunov pisiem (ushort big-endian; 64 = volne miesto, okrem prveho pisma), 32 B nazov suboru.
/// Pismo: 20 B nazov, sirka, vyska, proporcionalne (po 1 B), potom 256 × (sirka znaku + vyska × bajtov na riadok).
/// Za poslednym pismom nasleduje bajt 'a'.
/// </remarks>
public sealed class ElenFontFile
{
    /// <summary>Najviac pisiem v subore.</summary>
    public const int MaxFonts = 16;

    private const int HeaderSize = MaxFonts * 2 + FileNameSize;
    private const int FileNameSize = 32;
    private const int FontNameSize = 20;
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
    /// <param name="strict">prisna kontrola - nazov v hlavicke musi byt meno suboru, sirky znakov musia sediet s pismom</param>
    public static ElenFontFile Load(string path, bool strict = false)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        if (!File.Exists(path))
            throw new FileNotFoundException($"File {path} was not found.", path);

        using var stream = File.OpenRead(path);
        var file = Read(stream, strict);

        var expected = Path.GetFileNameWithoutExtension(path);
        if (strict && file.Name != expected)
            throw new FormatException($"File {path} have different name ({expected}) than the one defined inside the file: {file.Name}.");

        return file;
    }

    /// <summary>
    /// Precita subor pisiem z prudu, ktory sa da posuvat.
    /// </summary>
    /// <param name="stream">prud</param>
    /// <param name="strict">prisna kontrola sirok znakov</param>
    public static ElenFontFile Read(Stream stream, bool strict = false)
    {
        ArgumentNullException.ThrowIfNull(stream);
        using var reader = new BinaryReader(stream, Encoding.ASCII, true);

        var positions = new ushort[MaxFonts];
        for (var i = 0; i < positions.Length; i++)
            positions[i] = (ushort)((reader.ReadByte() << 8) | reader.ReadByte());

        var name = ReadText(reader.ReadBytes(FileNameSize));
        var fonts = new List<ElenFont>();

        for (var i = 0; i < positions.Length; i++)
        {
            // 64 = hned za hlavickou: prve pismo, inak volne miesto; prve miesto je volne, len ak za hlavickou
            // nie je nic okrem koncoveho bajtu
            if (positions[i] == HeaderSize && (i != 0 || stream.Length <= HeaderSize + 1))
                break;

            stream.Position = positions[i];
            var fontName = ReadText(reader.ReadBytes(FontNameSize));
            if (fonts.Any(f => f.Name == fontName))
                continue;

            int maxWidth = reader.ReadByte();
            int height = reader.ReadByte();
            var proportional = reader.ReadBoolean();
            var bytesPerRow = ElenFont.BytesPerRowFor(maxWidth);

            var glyphs = new ElenGlyph[ElenFont.GlyphCount];
            var truncated = false;
            for (var j = 0; j < glyphs.Length; j++)
            {
                // skrateny subor: v prisnom rezime chyba, inak zvysne znaky prazdne (ako ich vidi panel)
                if (truncated || stream.Position >= stream.Length)
                {
                    if (strict)
                        throw new EndOfStreamException($"Font {fontName} ends inside symbol {j}.");
                    truncated = true;
                    glyphs[j] = new ElenGlyph(0, height, bytesPerRow, new byte[height * bytesPerRow]);
                    continue;
                }

                int width = reader.ReadByte();
                if (strict && !proportional && width != maxWidth && width != 0)
                    throw new FormatException($"Font {fontName} is not proportional but index symbol {j} has different width ({width}).");
                if (strict && width > maxWidth)
                    throw new FormatException($"Font {fontName} have max width {maxWidth} but index symbol {j} has bigger width ({width}).");

                var data = reader.ReadBytes(height * bytesPerRow);
                if (data.Length != height * bytesPerRow)
                {
                    if (strict)
                        throw new EndOfStreamException($"Font {fontName} ends inside symbol {j}.");
                    truncated = true;
                    Array.Resize(ref data, height * bytesPerRow);
                }

                glyphs[j] = new ElenGlyph(width, height, bytesPerRow, data);
            }

            fonts.Add(new ElenFont(fontName, maxWidth, height, proportional, glyphs));
            if (truncated)
                break;
        }

        return new ElenFontFile(name, fonts);
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
            throw new FormatException($"Count of fonts must be smaller than {MaxFonts + 1} (max is {MaxFonts}).");

        // posuny pisiem sa daju vypocitat vopred - zapis nepotrebuje posuvat prud
        var positions = new ushort[MaxFonts];
        Array.Fill(positions, (ushort)HeaderSize);
        var position = HeaderSize;
        for (var i = 0; i < Fonts.Count; i++)
        {
            if (position > ushort.MaxValue)
                throw new FormatException($"Font {Fonts[i].Name} starts beyond 64 KB.");
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
                    throw new FormatException($"Symbol of font {font.Name} has different size than the font.");
                writer.Write((byte)glyph.Width);
                writer.Write(glyph.Data);
            }
        }

        writer.Write(Terminator);
    }

    private static string ReadText(byte[] bytes)
    {
        var end = Array.IndexOf(bytes, (byte)0);
        return Encoding.ASCII.GetString(bytes, 0, end < 0 ? bytes.Length : end).TrimEnd();
    }

    private static byte[] WriteText(string text, int size)
    {
        ArgumentException.ThrowIfNullOrEmpty(text);
        if (text.Length > size)
            throw new ArgumentException($"String length must be shorter than {size}.", nameof(text));

        var bytes = new byte[size];
        Encoding.ASCII.GetBytes(text, bytes);
        return bytes;
    }
}
