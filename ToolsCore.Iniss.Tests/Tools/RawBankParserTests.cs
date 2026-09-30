using ToolsCore.Iniss.Entities;
using ToolsCore.Iniss.Tools;

namespace ToolsCore.Tests.Tools;

/// <summary>
/// Zapis a citanie FYZBANK.DAT a FYZZVUK.DAT (banka zvukov INISS).
/// </summary>
[TestClass]
public class RawBankParserTests
{
    private string _bank = null!;

    [TestInitialize]
    public void Init()
    {
        _bank = Path.Combine(Path.GetTempPath(), "RawBankParserTests_" + Guid.NewGuid().ToString("N")) + "\\";
        Directory.CreateDirectory(Path.Combine(_bank, "SK"));
    }

    [TestCleanup]
    public void Cleanup()
    {
        foreach (var file in Directory.GetFiles(_bank, "*", SearchOption.AllDirectories))
            File.SetAttributes(file, FileAttributes.Normal);
        Directory.Delete(_bank, true);
    }

    [TestMethod]
    public void FyzBank_RoundTrip_ZachovaJazykyAjSDiakritikou()
    {
        List<FyzLanguage> languages =
        [
            new("SK", "Slovenčina", "FYZZVUK.DAT", "SK\\"),
            new("CZ", "Čeština", "ZVUKY.DAT", "Česky\\")
        ];

        RawBankParser.WriteFyzBankFile(_bank, languages);
        var read = RawBankParser.ReadFyzBankFile(_bank, out var count);

        Assert.AreEqual(2, count);
        Assert.HasCount(2, read);
        for (var i = 0; i < languages.Count; i++)
        {
            Assert.AreEqual(languages[i].Key, read[i].Key);
            Assert.AreEqual(languages[i].Name, read[i].Name);
            Assert.AreEqual(languages[i].FileDefName, read[i].FileDefName);
            Assert.AreEqual(languages[i].RelativePath, read[i].RelativePath);
        }
    }

    [TestMethod]
    public void FyzZvuk_RoundTrip_ZachovaSkupinyAZvuky()
    {
        var language = new FyzLanguage("SK", "Slovenčina", "FYZZVUK.DAT", "SK\\") { Groups = [] };
        var group = new FyzGroup(language, "VL", "Vlaky", "VLAKY\\");
        // text dlhsi ako 255 znakov sa zapisuje s dvojbajtovou dlzkou
        var longText = string.Concat(Enumerable.Repeat("Vážení cestujúci, ", 20));
        group.Sounds.Add(new FyzSound(group, "R", "Rýchlik", "R.WAV", "", longText, 1234));
        group.Sounds.Add(new FyzSound(group, "OS", "Osobný", "OS.WAV", "STARE\\", "Osobný vlak", 567));
        language.Groups.Add(group);
        language.Groups.Add(new FyzGroup(language, "PR", "Prázdna", "PRAZDNA\\"));

        RawBankParser.WriteFyzZvukFile(_bank, language);
        var copy = new FyzLanguage("SK", "Slovenčina", "FYZZVUK.DAT", "SK\\");
        var sounds = RawBankParser.ReadFyzZvukFile(_bank, copy);

        Assert.HasCount(2, copy.Groups);
        Assert.AreEqual("VL", copy.Groups[0].Key);
        Assert.AreEqual("Vlaky", copy.Groups[0].Name);
        Assert.AreEqual("VLAKY\\", copy.Groups[0].RelativePath);
        Assert.IsEmpty(copy.Groups[1].Sounds);

        Assert.HasCount(2, sounds);
        Assert.AreEqual(longText, sounds[0].Text);
        Assert.AreEqual("Rýchlik", sounds[0].Name);
        Assert.AreEqual("R.WAV", sounds[0].FileName);
        Assert.AreEqual("", sounds[0].AdditionalRelativePath);
        Assert.AreEqual(1234, sounds[0].Duration);
        Assert.AreEqual("OS.WAV", sounds[1].FileName);
        Assert.AreEqual("STARE\\", sounds[1].AdditionalRelativePath);
        Assert.AreSame(copy.Groups[0], sounds[1].Group);
    }

    [TestMethod]
    public void FyzBank_ZlyhanyZapis_NechaPovodnySuborANeostaneTmp()
    {
        RawBankParser.WriteFyzBankFile(_bank, [new FyzLanguage("SK", "Slovenčina")]);
        var file = RawBankParser.FyzBankFile(_bank);
        var original = File.ReadAllBytes(file);
        File.SetAttributes(file, FileAttributes.ReadOnly);

        Assert.ThrowsExactly<UnauthorizedAccessException>(() =>
            RawBankParser.WriteFyzBankFile(_bank, [new FyzLanguage("CZ", "Čeština")]));

        CollectionAssert.AreEqual(original, File.ReadAllBytes(file));
        Assert.IsFalse(File.Exists(file + ".tmp"));
    }

    [TestMethod]
    public void FyzBank_CitanieSuboruLenNaCitanie_Funguje()
    {
        RawBankParser.WriteFyzBankFile(_bank, [new FyzLanguage("SK", "Slovenčina")]);
        File.SetAttributes(RawBankParser.FyzBankFile(_bank), FileAttributes.ReadOnly);

        Assert.HasCount(1, RawBankParser.ReadFyzBankFile(_bank, out _));
    }
}
