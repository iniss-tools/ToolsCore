using System.Xml.Linq;
using ToolsCore.Iniss2.Grafikon;

namespace ToolsCore.Iniss2.Tables;

/// <summary>
/// Tabulky TabTab - TabTab.xml v priecinku stanice a TabTabGlobal.xml v priecinku grafikonu maju rovnaky tvar. Server ich
/// nikdy nezapisuje.
/// </summary>
public static class TabTabXml
{
    /// <summary>Subor tabuliek stanice.</summary>
    public const string StationFileName = "TabTab.xml";

    /// <summary>Subor spolocnych tabuliek v priecinku grafikonu.</summary>
    public const string GlobalFileName = "TabTabGlobal.xml";

    /// <summary>
    /// Nacita tabulky zo suboru. Chybajuci subor je prazdny zoznam. Vyraz ostane presne ako v subore (aj odsadenie
    /// a prazdne riadky okolo kodu).
    /// </summary>
    /// <param name="file">cesta k suboru</param>
    public static List<TabTabTable> Read(string file) => File.Exists(file) ? Parse(Iniss2Xml.Load(file).Root!) : [];

    /// <summary>
    /// Spolocne tabulky instalacie ako ich pouzije server: prvy TabTabGlobal.xml v poradi grafikonov DirList.xml. Ziadny =
    /// prazdny zoznam.
    /// </summary>
    /// <param name="dataDir">priecinok dat servera</param>
    public static List<TabTabTable> ReadGlobal(string dataDir) =>
        DirListXml.Read(dataDir).Select(group => Path.Combine(dataDir, group.Name, GlobalFileName)).FirstOrDefault(File.Exists) is { } file
            ? Read(file)
            : [];

    /// <summary>
    /// Zapise tabulky. Existujuci subor sa upravi na mieste (komentare a nezmenene tabulky ostanu po znakoch).
    /// </summary>
    /// <param name="file">cesta k suboru</param>
    /// <param name="tables">tabulky s roznymi nazvami</param>
    /// <returns><see langword="false" />, ak subor uz mal tento obsah.</returns>
    public static bool Write(string file, IEnumerable<TabTabTable> tables) => Iniss2Xml.Write(file, Build(tables), root => Build(Parse(root)));

    private static List<TabTabTable> Parse(XElement root) => root.Elements("t").Select(table => new TabTabTable(Iniss2Xml.Text(table, "n"))
    {
        Description = Iniss2Xml.Text(table, "d"),
        ViewValues = Flag(table, "viewval"),
        IgnoreCase = Flag(table, "ignorecase"),
        Expression = table.Element("expr")?.Value ?? "",
        Entries = table.Elements("k").Select(entry => new TabTabEntry(Text(entry.Element("k")), Font(entry.Element("k")), Text(entry.Element("v")),
            Font(entry.Element("v")))).ToList()
    }).ToList();

    private static XElement Build(IEnumerable<TabTabTable> tables) => new("TABTAB", tables.Select(table => new XElement("t",
        new XAttribute("n", table.Name), Iniss2Xml.Optional("d", table.Description), table.ViewValues ? new XAttribute("viewval", "true") : null,
        table.IgnoreCase ? new XAttribute("ignorecase", "true") : null,
        table.Expression.Length > 0 ? new XElement("expr", table.Expression) : null,
        table.Entries.Select(entry => new XElement("k", Part("k", entry.KeyText, entry.KeyFont), Part("v", entry.ValueText, entry.ValueFont))))));

    private static XElement Part(string name, string text, int font) => new(name, Iniss2Xml.Optional("s", text), new XAttribute("f", font));

    private static bool Flag(XElement element, string attribute) => string.Equals(Iniss2Xml.Text(element, attribute), "true", StringComparison.OrdinalIgnoreCase);

    private static string Text(XElement? element) => element is null ? "" : Iniss2Xml.Text(element, "s");

    private static int Font(XElement? element) => element is null ? TabTabTable.NoFont : Iniss2Xml.Number(element, "f", TabTabTable.NoFont);
}
