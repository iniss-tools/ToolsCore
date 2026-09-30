using ToolsCore.Iniss.Tools;

namespace ToolsCore.Tests.Tools;

/// <summary>
/// Kodovanie nahravok .EWA bez disku.
/// </summary>
[TestClass]
public class EwaCodecTests
{
    /// <summary>
    /// Najmensi platny WAV: RIFF, fmt (PCM 8 kHz mono 8 bit) a data.
    /// </summary>
    private static byte[] Wav(int samples = 64)
    {
        using var ms = new MemoryStream();
        using var w = new BinaryWriter(ms);
        w.Write("RIFF"u8.ToArray());
        w.Write(36 + samples);
        w.Write("WAVE"u8.ToArray());
        w.Write("fmt "u8.ToArray());
        w.Write(16);
        w.Write((short)1);
        w.Write((short)1);
        w.Write(8000);
        w.Write(8000);
        w.Write((short)1);
        w.Write((short)8);
        w.Write("data"u8.ToArray());
        w.Write(samples);
        for (var i = 0; i < samples; i++)
            w.Write((byte)(i * 7));
        w.Flush();
        return ms.ToArray();
    }

    private static byte[] Encode(byte[] wav, byte? key = null, bool check = false)
    {
        using var output = new MemoryStream();
        EwaCodec.Encode(new MemoryStream(wav), output, check, key);
        return output.ToArray();
    }

    private static byte[] Decode(byte[] ewa, bool check = false)
    {
        using var output = new MemoryStream();
        EwaCodec.Decode(new MemoryStream(ewa), output, check);
        return output.ToArray();
    }

    [TestMethod]
    public void EncodeDecode_KazdyKluc_VratiPovodnyWav()
    {
        var wav = Wav(300);
        foreach (byte key in new byte[] { 1, 82, 0xAB, 0xFF })
        {
            var ewa = Encode(wav, key);
            CollectionAssert.AreNotEqual(wav, ewa, $"kluc {key}");
            CollectionAssert.AreEqual(wav, Decode(ewa, check: true), $"kluc {key}");
        }
    }

    [TestMethod]
    public void Encode_ZnamyKluc_ZodpovedaFormatuInissu()
    {
        var wav = Wav();
        var ewa = Encode(wav, 0x5A);

        // bajt na pozicii i = (wav[i] + 0x11 * i) XOR kluc
        for (var i = 0; i < wav.Length; i++)
            Assert.AreEqual((byte)(((wav[i] + 0x11 * i) & 0xff) ^ 0x5A), ewa[i], $"bajt {i}");
    }

    [TestMethod]
    public void Encode_NahodnyKluc_NikdyNieJeNula()
    {
        var wav = Wav(8);
        for (var i = 0; i < 300; i++)
            Assert.AreNotEqual((byte)'R', Encode(wav)[0]);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Encode(wav, 0));
    }

    [TestMethod]
    public void Decode_NekodovanyWav_PonechaBezZmeny()
    {
        var wav = Wav();

        CollectionAssert.AreEqual(wav, Decode(wav, check: true));
    }

    [TestMethod]
    public void Check_NeplatnyWav_FormatException()
    {
        var bad = "RIFF____NOPE"u8.ToArray();

        Assert.ThrowsExactly<FormatException>(() => EwaCodec.CheckWav(new MemoryStream(bad)));
        Assert.ThrowsExactly<FormatException>(() => Encode(bad, check: true));
        var ewa = Encode(bad, 7);
        Assert.ThrowsExactly<FormatException>(() => Decode(ewa, check: true));
        Assert.ThrowsExactly<FormatException>(() => EwaCodec.CheckWav(new MemoryStream(Wav().Take(30).ToArray())));
    }
}
