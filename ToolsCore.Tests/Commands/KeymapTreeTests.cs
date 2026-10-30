using ToolsCore.Commands;

namespace ToolsCore.Tests.Commands;

/// <summary>
/// Strom klavesovych skratiek: poradie podla hlavnej ponuky okna, skupiny pre prikazy mimo ponuky a dalsie okna.
/// </summary>
[TestClass]
public class KeymapTreeTests
{
    private static readonly CommandInfo Open = new("Open", "Otvoriť", Shortcut.CtrlO);
    private static readonly CommandInfo ImportData = new("ImportData", "Importovať dáta", Shortcut.None);
    private static readonly CommandInfo Save = new("Save", "Uložiť", Shortcut.CtrlS);
    private static readonly CommandInfo Settings = new("LSettings", "Lokálne nastavenia", Shortcut.CtrlL);
    private static readonly CommandInfo Tracks = new("LSTracks", "Lokálne nastavenia – Koľaje", Shortcut.None);
    private static readonly CommandInfo AddGroup = new("AddGroup", "AddGroup", Shortcut.None, "Skupiny");
    private static readonly CommandInfo EditorSave = new("TabTab.Save", "Uložiť", Shortcut.CtrlS, Window: "Editor TabTab");
    private static readonly CommandInfo Unbound = new("Docs", "Dokumentácia", Shortcut.F1);

    [TestMethod]
    public void Build_PodlaPonuky_PoradieACestaZPonuky()
    {
        using var menu = CreateMenu(out var commands);

        var roots = KeymapTree.Build([Save, Open, ImportData, Settings, Tracks, AddGroup, Unbound, EditorSave], commands, "Hlavné okno");

        Assert.HasCount(2, roots);
        var main = roots[0];
        Assert.AreEqual("Hlavné okno", main.Text);
        CollectionAssert.AreEqual(new[] { "Súbor", "Vlastnosti", "Skupiny", "Dokumentácia" }, main.Children.Select(c => c.Text).ToArray());

        var file = main.Children[0];
        CollectionAssert.AreEqual(new[] { "Otvoriť", "Importovať", "Uložiť" }, file.Children.Select(c => c.Text).ToArray(),
            "text polozky bez & a trojbodky, separator a polozka Nedavne bez prikazu chybaju");
        Assert.AreSame(ImportData, file.Children[1].Children.Single().Command);

        var settings = main.Children[1].Children.Single();
        Assert.AreSame(Settings, settings.Command, "polozka s podponukou je aj prikazom");
        Assert.AreSame(Tracks, settings.Children.Single().Command);

        Assert.AreEqual("Pridať skupinu", main.Children[2].Children.Single().Text, "prikaz mimo ponuky ma text svojej polozky");
        Assert.AreEqual("Editor TabTab", roots[1].Text);
        Assert.AreSame(EditorSave, roots[1].Children.Single().Command);
    }

    [TestMethod]
    public void Build_BezOkna_PodlaKatalogu()
    {
        var roots = KeymapTree.Build([Open, AddGroup], null, "Hlavné okno");

        var main = roots.Single();
        Assert.AreEqual("Otvoriť", main.Children[0].Text);
        Assert.AreEqual("Skupiny", main.Children[1].Text);
        Assert.AreEqual("AddGroup", main.Children[1].Children.Single().Text);
    }

    [TestMethod]
    [DataRow("&Otvoriť...", "Otvoriť")]
    [DataRow("Stavový diagram…", "Stavový diagram")]
    [DataRow("Tom && Jerry", "Tom & Jerry")]
    public void ItemText_BezMnemonikyATrojbodky(string text, string expected)
    {
        using var item = new ToolStripMenuItem(text);

        Assert.AreEqual(expected, KeymapTree.ItemText(item));
    }

    private static MenuStrip CreateMenu(out CommandSet commands)
    {
        var menu = new MenuStrip();
        var file = new ToolStripMenuItem("&Súbor");
        var open = new ToolStripMenuItem("&Otvoriť...");
        var import = new ToolStripMenuItem("Importovať");
        var importData = new ToolStripMenuItem("Dáta...");
        var save = new ToolStripMenuItem("&Uložiť");
        var recent = new ToolStripMenuItem("Nedávne");
        recent.DropDownItems.Add(new ToolStripMenuItem(@"C:\INISS"));
        import.DropDownItems.Add(importData);
        file.DropDownItems.AddRange([open, import, new ToolStripSeparator(), save, recent]);

        var properties = new ToolStripMenuItem("Vlastnosti");
        var localSettings = new ToolStripMenuItem("Lokálne nastavenia...");
        var tracks = new ToolStripMenuItem("Koľaje...");
        localSettings.DropDownItems.Add(tracks);
        properties.DropDownItems.Add(localSettings);
        menu.Items.AddRange([file, properties]);

        var context = new ContextMenuStrip();
        var addGroup = new ToolStripMenuItem("Pridať skupinu...");
        context.Items.Add(addGroup);

        commands = new CommandSet();
        commands.Add(Open, () => { }).Bind(open);
        commands.Add(ImportData, () => { }).Bind(importData);
        commands.Add(Save, () => { }).Bind(save);
        commands.Add(Settings, () => { }).Bind(localSettings);
        commands.Add(Tracks, () => { }).Bind(tracks);
        commands.Add(AddGroup, () => { }).Bind(addGroup);
        return menu;
    }
}
