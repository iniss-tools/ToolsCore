using System.Diagnostics.CodeAnalysis;
using ToolsCore.Iniss.Grafikon;

namespace ToolsCore.Tests.Grafikon;

/// <summary>
/// Priznaky grafikonu v DirList.TXT: rozlozenie ako v INISSe (poradie, velkost pismen, K/M, posledna cislica) a zapis.
/// </summary>
[TestClass]
[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
public class DirListFlagsTests
{
    [TestMethod]
    [DataRow(null, "")]
    [DataRow("", "")]
    [DataRow("ZK3", "ZK3")]
    [DataRow("3kz", "ZK3", DisplayName = "poradie a velkost pismen nezalezia")]
    [DataRow("Z, O", "ZO", DisplayName = "oddelovace sa ignoruju")]
    [DataRow("MK", "K", DisplayName = "pri K aj M plati K")]
    [DataRow("M", "M")]
    [DataRow("27", "7", DisplayName = "z viacerych cislic plati posledna")]
    [DataRow("0X", "", DisplayName = "nula ani ine znaky nie su priznaky")]
    public void Parse_AkoINISS(string? text, string expected)
    {
        Assert.AreEqual(expected, DirListFlags.Parse(text).ToString());
    }

    [TestMethod]
    public void Parse_RozlozeneHodnoty()
    {
        var flags = DirListFlags.Parse("OM9");

        Assert.IsFalse(flags.Spread);
        Assert.IsTrue(flags.DepartureTrack);
        Assert.AreEqual(DirListTrainCreation.CreateWithoutCategori, flags.TrainCreation);
        Assert.AreEqual(9, flags.Switch);
        Assert.IsFalse(flags.IsEmpty);
        Assert.IsTrue(DirListFlags.Parse(" ").IsEmpty);
    }

    [TestMethod]
    [DataRow("ZK3", true)]
    [DataRow("", true)]
    [DataRow(null, true)]
    [DataRow("zk3", false)]
    [DataRow("KZ", false)]
    [DataRow("ZKM", false)]
    public void IsCanonical_TextZodpovedaZapisu(string? text, bool expected)
    {
        Assert.AreEqual(expected, DirListFlags.IsCanonical(text));
    }
}
