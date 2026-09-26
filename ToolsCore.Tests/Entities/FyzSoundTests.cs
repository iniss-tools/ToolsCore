using ToolsCore.Entities;

namespace ToolsCore.Tests.Entities;

/// <summary>
///     Cesta k suboru zvuku - pridavna cesta je v INISS relativna k priecinku skupiny.
/// </summary>
[TestClass]
public class FyzSoundTests
{
    private const string Bank = @"C:\INISS\RAWBANK\";

    [TestMethod]
    [DataRow("SK\\", "R1\\", "", "9900100.EWA", @"C:\INISS\RAWBANK\SK\R1\9900100.EWA")]
    [DataRow("CZ\\", "N5\\", "..\\N5\\", "01.WAV", @"C:\INISS\RAWBANK\CZ\N5\01.WAV")]
    [DataRow("SK\\", "R1\\", "..\\C9\\..\\Poz7\\..\\Poz1\\", "ZALOK.WAV", @"C:\INISS\RAWBANK\SK\Poz1\ZALOK.WAV")]
    [DataRow("SK\\", "Linka\\", "dedek\\", "S.WAV", @"C:\INISS\RAWBANK\SK\Linka\dedek\S.WAV")]
    [DataRow("SK\\", "R1\\", ".\\", "1.WAV", @"C:\INISS\RAWBANK\SK\R1\1.WAV")]
    public void GetAbsPath_PridavnaCestaRelativnaKSkupine(string language, string group, string additional, string file, string expected)
    {
        var sound = Sound(language, group, additional, file);

        Assert.AreEqual(expected, sound.GetAbsPath(Bank));
    }

    [TestMethod]
    public void GetAbsPath_BezBankyVratiCestuRelativnuKBanke()
    {
        var sound = Sound("CZ\\", "N5\\", "..\\N5\\", "01.WAV");

        Assert.AreEqual(@"CZ\N5\..\N5\01.WAV", sound.GetAbsPath(""));
    }

    private static FyzSound Sound(string language, string group, string additional, string file)
    {
        var lang = new FyzLanguage("SK", "Slovenčina", "FYZZVUK.DAT", language);
        var grp = new FyzGroup(lang, group.TrimEnd('\\'), group.TrimEnd('\\'), group);
        return new FyzSound(grp, "k", "k", file, additional, "", 0);
    }
}
