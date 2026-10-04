namespace ToolsCore.Iniss.Tools;

/// <summary>
/// Kod Kamenickych (KEYBCS2) - stara ceska kodova stranka tabul ELEN, ELENOLD a ELEN16Kam.
/// 0x00-0x7F ako ASCII, 0x80-0xAF ceske a slovenske znaky, 0xB0-0xFF ako CP437.
/// Znaky, ktore v nej nie su, sa zakoduju ako '?'.
/// </summary>
public sealed class KamenickyEncoding : Encoding
{
    private const char Replacement = '?';

    // 0x80-0xFF
    private const string Upper =
        "ČüéďäĎŤčěĚĹÍľĺÄÁ" +
        "ÉžŽôöÓůÚýÖÜŠĽÝŘť" +
        "áíóúňŇŮÔšřŕŔ¼§«»" +
        "░▒▓│┤╡╢╖╕╣║╗╝╜╛┐" +
        "└┴┬├─┼╞╟╚╔╩╦╠═╬╧" +
        "╨╤╥╙╘╒╓╫╪┘┌█▄▌▐▀" +
        "αßΓπΣσµτΦΘΩδ∞φε∩" +
        "≡±≥≤⌠⌡÷≈°∙·√ⁿ²■ ";

    private static readonly Dictionary<char, byte> ToByte = BuildMap();

    /// <inheritdoc />
    public override string EncodingName => "Kamenický (KEYBCS2)";

    /// <inheritdoc />
    public override string WebName => "x-kamenicky";

    /// <inheritdoc />
    public override bool IsSingleByte => true;

    /// <inheritdoc />
    public override int GetByteCount(char[] chars, int index, int count)
    {
        ArgumentNullException.ThrowIfNull(chars);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        return count;
    }

    /// <inheritdoc />
    public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
    {
        ArgumentNullException.ThrowIfNull(chars);
        ArgumentNullException.ThrowIfNull(bytes);
        for (var i = 0; i < charCount; i++)
            bytes[byteIndex + i] = Encode(chars[charIndex + i]);
        return charCount;
    }

    /// <inheritdoc />
    public override int GetCharCount(byte[] bytes, int index, int count)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        return count;
    }

    /// <inheritdoc />
    public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        ArgumentNullException.ThrowIfNull(chars);
        for (var i = 0; i < byteCount; i++)
            chars[charIndex + i] = Decode(bytes[byteIndex + i]);
        return byteCount;
    }

    /// <inheritdoc />
    public override int GetMaxByteCount(int charCount) => charCount;

    /// <inheritdoc />
    public override int GetMaxCharCount(int byteCount) => byteCount;

    /// <summary>Znak bajtu <paramref name="b" />.</summary>
    public static char Decode(byte b) => b < 0x80 ? (char)b : Upper[b - 0x80];

    /// <summary>Bajt znaku <paramref name="c" />; '?' pre znak mimo kodovej stranky.</summary>
    public static byte Encode(char c) => c < 0x80 ? (byte)c : ToByte.GetValueOrDefault(c, (byte)Replacement);

    private static Dictionary<char, byte> BuildMap()
    {
        var map = new Dictionary<char, byte>(Upper.Length);
        for (var i = 0; i < Upper.Length; i++)
            map[Upper[i]] = (byte)(0x80 + i);
        return map;
    }
}
