using System.Xml.Linq;
using ToolsCore.Iniss.Entities;
using ToolsCore.Iniss.Tools;

namespace ToolsCore.Tests.Tools;

/// <summary>
/// Zapis jazyka banky zvukov do FyzBank.xml (INISS2).
/// </summary>
[TestClass]
public class FyzBankXmlWriterTests
{
    private static readonly DateTimeOffset Date = new(2026, 10, 1, 12, 30, 0, TimeSpan.FromHours(2));

    private static FyzLanguage Language()
    {
        var language = new FyzLanguage("SK", "Slovenčina", "FYZZVUK.DAT", "SK\\") { Groups = [] };
        var group = new FyzGroup(language, "DZ", "Dôvody zmeškania", "DZ\\");
        group.Sounds.Add(new FyzSound(group, "VNP", "Počasie", "VNP.WAV", "", "nepriaznivé počasie & \"búrka\"", 1234));
        group.Sounds.Add(new FyzSound(group, "01", "Jedna", "01.EWA", "..\\N5\\", "jedna", 0));
        language.Groups.Add(group);
        language.Groups.Add(new FyzGroup(language, "Slova", "Slová", "Slova\\"));
        return language;
    }

    private static XDocument WriteAndParse(FyzLanguage language, Func<FyzSound, string?> md5, out byte[] bytes, bool isDefault = true)
    {
        using var stream = new MemoryStream();
        FyzBankXmlWriter.Write(stream, language, isDefault, Date, md5);
        bytes = stream.ToArray();
        return XDocument.Load(new MemoryStream(bytes));
    }

    [TestMethod]
    public void Write_JazykSkupinyAZvukyPodlaKlucov()
    {
        var doc = WriteAndParse(Language(), s => s.Key == "VNP" ? "abc" : null, out _);

        var lang = doc.Root!.Element("lang")!;
        Assert.AreEqual("FYZBANK", doc.Root.Name.LocalName);
        Assert.AreEqual("true", (string?)lang.Attribute("default"));
        Assert.AreEqual("SK", (string?)lang.Attribute("k"));
        Assert.AreEqual("Slovenčina", (string?)lang.Attribute("n"));
        // priecinok jazyka bez koncovej lomky, priecinok skupiny s nou - ako v suboroch INISS2
        Assert.AreEqual("SK", (string?)lang.Attribute("d"));
        Assert.AreEqual(Date, (DateTimeOffset)lang.Attribute("date")!);

        var groups = lang.Elements("g").ToList();
        Assert.HasCount(2, groups);
        Assert.AreEqual("DZ", (string?)groups[0].Attribute("k"));
        Assert.AreEqual("DZ\\", (string?)groups[0].Attribute("d"));
        Assert.IsEmpty(groups[1].Elements("z"));

        var sounds = groups[0].Elements("z").ToList();
        // kluc zvuku, nie jeho nazov - INISS2 hlada zvuk podla klucov jazyk/skupina/zvuk
        Assert.AreEqual("VNP", (string?)sounds[0].Attribute("k"));
        Assert.AreEqual("nepriaznivé počasie & \"búrka\"", (string?)sounds[0].Attribute("t"));
        Assert.AreEqual("VNP.WAV", (string?)sounds[0].Attribute("f"));
        Assert.AreEqual("1234", (string?)sounds[0].Attribute("l"));
        Assert.AreEqual("abc", (string?)sounds[0].Attribute("md5"));
    }

    [TestMethod]
    public void Write_PridavnaCestaJeVNazveSuboruAChybajuciSuborNemaMd5()
    {
        var doc = WriteAndParse(Language(), _ => null, out _);

        var sound = doc.Descendants("z").Single(z => (string?)z.Attribute("k") == "01");
        Assert.AreEqual("..\\N5\\01.EWA", (string?)sound.Attribute("f"));
        Assert.IsNull(sound.Attribute("md5"));
    }

    [TestMethod]
    public void Write_NepredvolenyJazykNemaAtributDefault()
    {
        var doc = WriteAndParse(Language(), _ => null, out _, isDefault: false);

        Assert.IsNull(doc.Root!.Element("lang")!.Attribute("default"));
    }

    [TestMethod]
    public void Write_Utf8SBom()
    {
        WriteAndParse(Language(), _ => null, out var bytes);

        CollectionAssert.AreEqual(new byte[] { 0xEF, 0xBB, 0xBF }, bytes[..3]);
    }

    [TestMethod]
    public void FileMd5_MalePismenaAkoVINISS2()
    {
        var file = Path.GetTempFileName();
        try
        {
            File.WriteAllText(file, "abc");

            Assert.AreEqual("900150983cd24fb0d6963f7d28e17f72", FyzBankXmlWriter.FileMd5(file));
        }
        finally
        {
            File.Delete(file);
        }
    }

    [TestMethod]
    public void Write_DoSuboruPrepiseExistujuciANenechaTmp()
    {
        var dir = Path.Combine(Path.GetTempPath(), "FyzBankXmlWriterTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var file = Path.Combine(dir, FyzBankXmlWriter.FileName);
            File.WriteAllText(file, "stary obsah");

            FyzBankXmlWriter.Write(file, Language(), true, Date, _ => null);

            Assert.AreEqual("SK", (string?)XDocument.Load(file).Root!.Element("lang")!.Attribute("k"));
            Assert.IsFalse(File.Exists(file + ".tmp"));
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }
}
