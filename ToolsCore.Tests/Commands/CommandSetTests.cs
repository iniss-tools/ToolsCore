using ToolsCore.Commands;
using ToolsCore.XML;

namespace ToolsCore.Tests.Commands;

/// <summary>
/// Prikazy hlavneho okna: jedno tlacidlo aj polozka ponuky, stav a skratky na jednom mieste.
/// </summary>
[TestClass]
public class CommandSetTests
{
    private static readonly CommandInfo Save = new("Save", "Uložiť", Shortcut.CtrlS);
    private static readonly CommandInfo Delete = new("DeleteTrains", "Odstrániť", Shortcut.Del);

    [TestMethod]
    public void Bind_KlikNaTlacidloAjPolozku_VykonaPrikaz()
    {
        var count = 0;
        var commands = new CommandSet();
        var button = new ToolStripButton();
        var item = new ToolStripMenuItem();
        commands.Add(Save, () => count++).Bind(button, item);

        button.PerformClick();
        item.PerformClick();

        Assert.AreEqual(2, count);
        Assert.AreEqual(Keys.Control | Keys.S, item.ShortcutKeys);
    }

    [TestMethod]
    public void Bind_RozdelovacieTlacidlo_VykonaPrikazLenKlikomNaTlacidlo()
    {
        var count = 0;
        var split = new ToolStripSplitButton();
        new CommandSet().Add(Save, () => count++).Bind(split);

        split.PerformButtonClick();

        Assert.AreEqual(1, count);
    }

    [TestMethod]
    public void BindState_Prepinac_KlikNevykonaPrikazAleStavASkratkaPlatia()
    {
        var count = 0;
        var commands = new CommandSet();
        var toggle = new ToolStripMenuItem { CheckOnClick = true };
        commands.Add(Save, () => count++, () => false).BindState(toggle);

        toggle.PerformClick();
        commands.UpdateStates();

        Assert.AreEqual(0, count);
        Assert.IsFalse(toggle.Enabled);
        Assert.AreEqual(Keys.Control | Keys.S, toggle.ShortcutKeys);
    }

    [TestMethod]
    public void UpdateStates_PodlaCanExecute_PovoliAleboZakazeVsetkyPrvky()
    {
        var open = false;
        var commands = new CommandSet();
        var button = new ToolStripButton();
        var item = new ToolStripMenuItem();
        commands.Add(Save, () => { }, () => open).Bind(button, item);

        commands.UpdateStates();
        Assert.IsFalse(button.Enabled);
        Assert.IsFalse(item.Enabled);

        open = true;
        commands.UpdateStates();
        Assert.IsTrue(button.Enabled);
        Assert.IsTrue(item.Enabled);
    }

    [TestMethod]
    public void Execute_PrikazSaNedaVykonat_NicNespravi()
    {
        var count = 0;
        var command = new CommandSet().Add(Save, () => count++, () => false);

        Assert.IsFalse(command.Execute());
        Assert.AreEqual(0, count);
    }

    [TestMethod]
    public void ProcessShortcut_SkratkaPrikazu_VykonaHoAjBezPolozkyPonuky()
    {
        var saved = 0;
        var commands = new CommandSet();
        commands.Add(Save, () => saved++);

        Assert.IsTrue(commands.ProcessShortcut(Keys.Control | Keys.S));
        Assert.IsFalse(commands.ProcessShortcut(Keys.Control | Keys.O));
        Assert.IsFalse(commands.ProcessShortcut(Keys.None));
        Assert.AreEqual(1, saved);
    }

    [TestMethod]
    public void ProcessShortcut_ZakazanyPrikaz_KlavesOstavaOknu()
    {
        var commands = new CommandSet();
        commands.Add(Delete, () => Assert.Fail("zakazany prikaz sa nesmie vykonat"), () => false);

        Assert.IsFalse(commands.ProcessShortcut(Keys.Delete));
    }

    [TestMethod]
    public void ApplyShortcuts_ZmenenaAChybajucaSkratka_PouzijeNastavenieAleboPredvolenu()
    {
        var commands = new CommandSet();
        var saveItem = new ToolStripMenuItem();
        var deleteItem = new ToolStripMenuItem();
        commands.Add(Save, () => { }).Bind(saveItem);
        commands.Add(Delete, () => { }).Bind(deleteItem);
        var map = new ShortcutMap();
        map.Set(Save.Id, Shortcut.CtrlShiftS);

        commands.ApplyShortcuts(map);

        Assert.AreEqual(Keys.Control | Keys.Shift | Keys.S, saveItem.ShortcutKeys);
        Assert.AreEqual(Keys.Delete, deleteItem.ShortcutKeys);
        Assert.AreEqual(Keys.Control | Keys.Shift | Keys.S, commands[Save.Id].ShortcutKeys);
    }

    [TestMethod]
    public void Add_DuplicitnyIdentifikator_Vynimka()
    {
        var commands = new CommandSet();
        commands.Add(Save, () => { });

        Assert.ThrowsExactly<ArgumentException>(() => commands.Add(Save with { Text = "iný" }, () => { }));
    }
}
