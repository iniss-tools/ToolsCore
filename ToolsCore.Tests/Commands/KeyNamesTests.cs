using ToolsCore.Commands;

namespace ToolsCore.Tests.Commands;

/// <summary>
/// Skratky v nastaveniach: nazvy z enumu Shortcut (citatelne aj starsimi verziami) a ostatne kombinacie ako text.
/// </summary>
[TestClass]
public class KeyNamesTests
{
    [TestMethod]
    [DataRow(Keys.Control | Keys.S, "CtrlS")]
    [DataRow(Keys.F2, "F2")]
    [DataRow(Keys.Delete, "Del")]
    [DataRow(Keys.None, "None")]
    [DataRow(Keys.Control | Keys.Alt | Keys.K, "Ctrl+Alt+K")]
    [DataRow(Keys.Control | Keys.Enter, "Ctrl+Enter")]
    public void ToSetting_ANacitanie_ZachovaSkratku(Keys keys, string text)
    {
        Assert.AreEqual(text, KeyNames.ToSetting(keys));
        Assert.IsTrue(KeyNames.TryParseSetting(text, out var parsed));
        Assert.AreEqual(keys, parsed);
    }

    [TestMethod]
    [DataRow(Keys.Control | Keys.Alt | Keys.K, true)]
    [DataRow(Keys.F5, true)]
    [DataRow(Keys.Shift | Keys.A, false)]
    [DataRow(Keys.A, false)]
    [DataRow(Keys.Escape, false)]
    public void IsValid_AkoSkratkaPolozkyPonuky(Keys keys, bool valid) => Assert.AreEqual(valid, KeyNames.IsValid(keys));
}
