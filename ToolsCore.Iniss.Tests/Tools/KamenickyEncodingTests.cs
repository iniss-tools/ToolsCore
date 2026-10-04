using ToolsCore.Iniss.Tools;

namespace ToolsCore.Tests.Tools;

/// <summary>
/// Kod Kamenickych (KEYBCS2) tabul ELEN.
/// </summary>
[TestClass]
public class KamenickyEncodingTests
{
    [TestMethod]
    [DataRow('Č', 0x80)]
    [DataRow('ž', 0x91)]
    [DataRow('Ž', 0x92)]
    [DataRow('ô', 0x93)]
    [DataRow('ů', 0x96)]
    [DataRow('š', 0xA8)]
    [DataRow('»', 0xAF)]
    [DataRow('░', 0xB0)]
    [DataRow(' ', 0xFF)]
    public void Znak_Bajt(char c, int b)
    {
        Assert.AreEqual((byte)b, KamenickyEncoding.Encode(c));
        Assert.AreEqual(c, KamenickyEncoding.Decode((byte)b));
    }

    [TestMethod]
    public void VsetkyBajty_TamASpat()
    {
        var bytes = Enumerable.Range(0, 256).Select(b => (byte)b).ToArray();

        CollectionAssert.AreEqual(bytes, Encodings.Kamenicky.GetBytes(Encodings.Kamenicky.GetString(bytes)));
    }

    [TestMethod]
    public void Text_SlovenskyAZnakMimo()
    {
        Assert.AreEqual("Žilina Ľubochňa", Encodings.Kamenicky.GetString(Encodings.Kamenicky.GetBytes("Žilina Ľubochňa")));
        Assert.AreEqual("?", Encodings.Kamenicky.GetString(Encodings.Kamenicky.GetBytes("€")));
    }
}
