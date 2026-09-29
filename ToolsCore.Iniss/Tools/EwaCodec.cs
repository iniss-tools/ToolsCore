using ToolsCore.Iniss.Properties;

namespace ToolsCore.Iniss.Tools;

/// <summary>
/// Kodovanie nahravok INISSu .EWA: WAV, v ktorom je kazdy bajt posunuty o 0x11 * pozicia a zakodovany XOR klucom.
/// Kluc sa zisti z prveho bajtu (WAV zacina 'R' z hlavicky RIFF); kluc 0 = nekodovany WAV, INISS ho cita priamo.
/// </summary>
public static class EwaCodec
{
    private const byte FIRST_WAV_BYTE = (byte)'R';

    /// <summary>
    /// Dekoduje .EWA na .WAV.
    /// </summary>
    /// <param name="ewa">vstup .EWA - citany od aktualnej pozicie po koniec</param>
    /// <param name="wav">vystup .WAV</param>
    /// <param name="check">skontrolovat, ci je vystup platny WAV (vystup musi byt citatelny a posuvatelny)</param>
    /// <exception cref="FormatException">vystup nie je platny WAV (len pri <paramref name="check" />)</exception>
    public static void Decode(Stream ewa, Stream wav, bool check = false)
    {
        ArgumentNullException.ThrowIfNull(ewa);
        ArgumentNullException.ThrowIfNull(wav);

        var data = ReadAll(ewa);
        if (data.Length > 0)
        {
            var key = (byte)(data[0] ^ FIRST_WAV_BYTE);
            // kluc 0 (prvy bajt 'R') - nekodovany WAV
            if (key != 0)
                for (var pos = 0; pos < data.Length; pos++)
                    data[pos] = (byte)(((data[pos] ^ key) - 0x11L * pos) & 0xff);
        }

        var start = wav.CanSeek ? wav.Position : 0;
        wav.Write(data, 0, data.Length);

        if (check)
        {
            wav.Flush();
            var end = wav.Position;
            wav.Position = start;
            CheckWav(wav);
            wav.Position = end;
        }
    }

    /// <summary>
    /// Zakoduje .WAV do .EWA nahodnym klucom (nikdy 0 - INISS by subor spoznal ako nekodovany WAV).
    /// </summary>
    /// <param name="wav">vstup .WAV - citany od aktualnej pozicie po koniec</param>
    /// <param name="ewa">vystup .EWA</param>
    /// <param name="check">skontrolovat, ci je vstup platny WAV (vstup musi byt posuvatelny)</param>
    /// <param name="key">kluc 1-255; <see langword="null" /> = nahodny</param>
    /// <exception cref="FormatException">vstup nie je platny WAV (len pri <paramref name="check" />)</exception>
    public static void Encode(Stream wav, Stream ewa, bool check = false, byte? key = null)
    {
        ArgumentNullException.ThrowIfNull(wav);
        ArgumentNullException.ThrowIfNull(ewa);
        if (key == 0)
            throw new ArgumentOutOfRangeException(nameof(key), "Kľúč 0 znamená nekódovaný WAV.");

        var ewaKey = key ?? (byte)Random.Shared.Next(1, 0xff + 1);

        if (check)
        {
            var start = wav.Position;
            CheckWav(wav);
            wav.Position = start;
        }

        var data = ReadAll(wav);
        for (var pos = 0; pos < data.Length; pos++)
            data[pos] = (byte)(((data[pos] + 0x11L * pos) & 0xff) ^ ewaKey);

        ewa.Write(data, 0, data.Length);
    }

    /// <summary>
    /// Skontroluje hlavicku WAV (RIFF, dlzka, WAVE, blok fmt, pri WAVE_FORMAT_EXTENSIBLE subformat PCM).
    /// </summary>
    /// <param name="wav">WAV od aktualnej pozicie; pozicia sa posunie</param>
    /// <exception cref="FormatException">nie je platny WAV</exception>
    public static void CheckWav(Stream wav)
    {
        ArgumentNullException.ThrowIfNull(wav);
        using var reader = new BinaryReader(wav, Encoding.ASCII, true);
        var start = wav.Position;

        if (Read4ByteString(reader) != "RIFF")
            throw new FormatException(Resources.Ewa_NoRiff);
        if (reader.ReadInt32() != wav.Length - start - 8)
            throw new FormatException(Resources.Ewa_BadLength);
        if (Read4ByteString(reader) != "WAVE")
            throw new FormatException(Resources.Ewa_NotWave);

        var fmtLen = -1;
        while (wav.Length - wav.Position >= 8)
        {
            if (Read4ByteString(reader) == "fmt ")
            {
                fmtLen = reader.ReadInt32();
                break;
            }

            wav.Position += reader.ReadInt32();
        }

        if (fmtLen < 0)
            throw new FormatException(Resources.Ewa_NoFormat);
        if (fmtLen < 16)
            throw new FormatException(Resources.Ewa_FmtTooShort);

        var formatTag = reader.ReadInt16();
        reader.ReadInt16(); // kanaly
        reader.ReadInt32(); // vzorky za sekundu
        reader.ReadInt32(); // bajty za sekundu
        reader.ReadInt16(); // zarovnanie bloku
        reader.ReadInt16(); // bity na vzorku

        if (formatTag == -2)
        {
            reader.ReadInt16();
            reader.ReadInt16();
            reader.ReadInt32();
            if (new Guid("00000001-0000-0010-8000-00AA00389B71") != new Guid(reader.ReadBytes(16)))
                throw new FormatException(Resources.Ewa_UnknownSubformat);
        }
    }

    private static byte[] ReadAll(Stream stream)
    {
        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        return buffer.ToArray();
    }

    private static string Read4ByteString(BinaryReader reader)
    {
        var array = reader.ReadBytes(4);
        return Encoding.ASCII.GetString(array);
    }
}
