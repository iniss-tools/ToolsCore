using System.Xml.Linq;

namespace ToolsCore.Iniss2.Tables;

/// <summary>
/// Polozka slovnika tabulky TabTab: text pred prekodovanim a po nom, kazdy s pismom.
/// </summary>
/// <param name="KeyText">text pred prekodovanim</param>
/// <param name="KeyFont">pismo kluca; <see cref="TabTabTable.AnyFont" /> = zhoda pri akomkolvek pisme</param>
/// <param name="ValueText">text po prekodovani</param>
/// <param name="ValueFont">pismo hodnoty; <see cref="TabTabTable.AnyFont" /> = pismo vstupu, <see cref="TabTabTable.NoFont" /> = bez pisma</param>
public sealed record TabTabEntry(string KeyText, int KeyFont, string ValueText, int ValueFont);

/// <summary>
/// Pomenovana tabulka pre stlpce tabul: vyraz VB.NET a slovnik.
/// </summary>
/// <param name="Name">nazov, na ktory sa odkazuje stlpec predlohy</param>
public sealed record TabTabTable(string Name)
{
    /// <summary>Text bez pisma.</summary>
    public const int NoFont = -1;

    /// <summary>Kluc: akekolvek pismo; hodnota: pismo vstupu.</summary>
    public const int AnyFont = -2;

    /// <summary>Popis pre citatela suboru.</summary>
    public string Description { get; init; } = "";

    /// <summary>Obsluha vidi prekodovany text.</summary>
    public bool ViewValues { get; init; }

    /// <summary>Slovnik nerozlisuje velkost pismen.</summary>
    public bool IgnoreCase { get; init; }

    /// <summary>Telo funkcie VB.NET, ktora vrati text stlpca; prazdne = bez vyrazu.</summary>
    public string Expression { get; init; } = "";

    /// <summary>Slovnik; dvojica textu a pisma kluca sa nesmie opakovat.</summary>
    public IReadOnlyList<TabTabEntry> Entries { get; init; } = [];
}

/// <summary>
/// Tabulky TabTab - TabTab.xml v priecinku stanice a TabTabGlobal.xml v priecinku grafikonu maju rovnaky tvar.
/// </summary>
public static class TabTabXml
{
    /// <summary>Subor tabuliek stanice.</summary>
    public const string StationFileName = "TabTab.xml";

    /// <summary>Subor spolocnych tabuliek v priecinku grafikonu.</summary>
    public const string GlobalFileName = "TabTabGlobal.xml";

    /// <summary>
    /// Nacita tabulky zo suboru. Chybajuci subor je prazdny zoznam.
    /// </summary>
    /// <param name="file">cesta k suboru</param>
    public static List<TabTabTable> Read(string file)
    {
        if (!File.Exists(file))
            return [];

        return Iniss2Xml.Load(file).Root!.Elements("t").Select(table => new TabTabTable(Iniss2Xml.Text(table, "n"))
        {
            Description = Iniss2Xml.Text(table, "d"),
            ViewValues = Flag(table, "viewval"),
            IgnoreCase = Flag(table, "ignorecase"),
            Expression = table.Element("expr")?.Value.Trim() ?? "",
            Entries = table.Elements("k").Select(entry => new TabTabEntry(Text(entry.Element("k")), Font(entry.Element("k")), Text(entry.Element("v")),
                Font(entry.Element("v")))).ToList()
        }).ToList();
    }

    /// <summary>
    /// Zapise tabulky.
    /// </summary>
    /// <param name="file">cesta k suboru</param>
    /// <param name="tables">tabulky s roznymi nazvami</param>
    public static void Write(string file, IEnumerable<TabTabTable> tables) =>
        Iniss2Xml.Save(file, new XElement("TABTAB", tables.Select(table => new XElement("t",
            new XAttribute("n", table.Name), Iniss2Xml.Optional("d", table.Description), table.ViewValues ? new XAttribute("viewval", "true") : null,
            table.IgnoreCase ? new XAttribute("ignorecase", "true") : null,
            table.Expression.Length > 0 ? new XElement("expr", table.Expression) : null,
            table.Entries.Select(entry => new XElement("k", Part("k", entry.KeyText, entry.KeyFont), Part("v", entry.ValueText, entry.ValueFont)))))));

    private static XElement Part(string name, string text, int font) => new(name, Iniss2Xml.Optional("s", text), new XAttribute("f", font));

    private static bool Flag(XElement element, string attribute) => string.Equals(Iniss2Xml.Text(element, attribute), "true", StringComparison.OrdinalIgnoreCase);

    private static string Text(XElement? element) => element is null ? "" : Iniss2Xml.Text(element, "s");

    private static int Font(XElement? element) => element is null ? TabTabTable.NoFont : Iniss2Xml.Number(element, "f", TabTabTable.NoFont);
}
