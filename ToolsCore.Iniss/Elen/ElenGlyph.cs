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
