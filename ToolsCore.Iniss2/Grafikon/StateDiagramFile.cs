using System.Globalization;
using System.Xml.Linq;

namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Stavovy diagram stanice - subor statedgm.xml.
/// </summary>
/// <param name="GraphIndex">vyraz VB.NET typu Integer, ktory vrati cislo grafu od 1</param>
public sealed record StateDiagram(string GraphIndex)
{
    /// <summary>Grafy v poradi suboru.</summary>
    public IReadOnlyList<StateGraph> Graphs { get; init; } = [];
}

/// <summary>
/// Graf stavoveho diagramu.
/// </summary>
/// <param name="Description">popis, objavi sa v chybach prekladu</param>
public sealed record StateGraph(string Description)
{
    /// <summary>Stavy v poradi suboru - vlak postupuje v smere suboru.</summary>
    public IReadOnlyList<StateNode> Nodes { get; init; } = [];
}

/// <summary>
/// Stav vlaku.
/// </summary>
/// <param name="Key">kluc stavu</param>
public sealed record StateNode(string Key)
{
    /// <summary>Nazov; prazdny = bez nazvu.</summary>
    public string Name { get; init; } = "";

    /// <summary>Nazov hlasenia z ReportKeys.xml; prazdny = bez hlasenia.</summary>
    public string Report { get; init; } = "";

    /// <summary>Priznaky stavu, pozri <see cref="StateNodeFlags" />.</summary>
    public IReadOnlyList<string> Flags { get; init; } = [];

    /// <summary>Podmienka stavu, vyraz VB.NET typu Boolean; prazdna = plati vzdy.</summary>
    public string Condition { get; init; } = "";

    /// <summary>Tlacidlo; null = stav nema tlacidlo.</summary>
    public StateButton? Button { get; init; }

    /// <summary>Automatika; null = stav nema automatiku.</summary>
    public StateAutomation? Automation { get; init; }
}

/// <summary>
/// Tlacidlo stavu.
/// </summary>
/// <param name="Text">text tlacidla</param>
/// <param name="Position">poradie tlacidla v klientovi</param>
public sealed record StateButton(string Text, int Position);

/// <summary>
/// Automatika stavu - kedy vlak do stavu prejde sam.
/// </summary>
/// <param name="Time">cas spustenia, vyraz VB.NET typu DateTime</param>
public sealed record StateAutomation(string Time)
{
    /// <summary>Dodatocna podmienka, vyraz typu Boolean; prazdna = plati vzdy.</summary>
    public string Condition { get; init; } = "";

    /// <summary>Rezim 0 rucne, 1 poloautomat, 2 automat - cislo alebo vyraz typu Byte; prazdny = 0.</summary>
    public string Mode { get; init; } = "";

    /// <summary>Interval opakovania v sekundach, cislo alebo vyraz; prazdny = bez opakovania.</summary>
    public string Repeat { get; init; } = "";

    /// <summary>Koncovy cas opakovania, vyraz typu DateTime; prazdny = bez konca.</summary>
    public string End { get; init; } = "";

    /// <summary>Variant hlasenia 1 kratke, 2 dlhe; null = server zvoli sam.</summary>
    public int? Variant { get; init; }

    /// <summary>Priznaky automatiky, pozri <see cref="StateNodeFlags.KeepWithoutClient" />.</summary>
    public IReadOnlyList<string> Flags { get; init; } = [];
}

/// <summary>
/// Priznaky stavu a automatiky, ako ich pise statedgm.xml.
/// </summary>
public static class StateNodeFlags
{
    /// <summary>Pomocny stav, vlak don neprejde.</summary>
    public const string Auxiliary = "AUXSTATE";

    /// <summary>Vlak stoji v stanici.</summary>
    public const string Standing = "STOJI";

    /// <summary>Kolaj je potvrdena.</summary>
    public const string TrackConfirmed = "POTVRZENAKOLEJ";

    /// <summary>Vlak je vybaveny.</summary>
    public const string Dispatched = "ODBAVEN";

    /// <summary>Spolu s <see cref="Dispatched" /> skryje vlak v zozname obsluhy.</summary>
    public const string Shadow = "SHADOW";

    /// <summary>Vlak prave stoji.</summary>
    public const string StandingNow = "NYNISTOJI";

    /// <summary>Vlak je na odchodovych castiach tabul.</summary>
    public const string OnDeparture = "JENAODJEZDOVE";

    /// <summary>Vlak je na prichodovych castiach tabul.</summary>
    public const string OnArrival = "JENAPRIJEZDOVE";

    /// <summary>Vlak je na nastupistnych a podchodovych castiach tabul.</summary>
    public const string OnPlatform = "JENASMEROVYCH";

    /// <summary>Na tabuli sa ukaze nastupiste.</summary>
    public const string ShowPlatform = "JEZOBRAZENAPOZICE";

    /// <summary>K nastupistu sa ukaze aj kolaj.</summary>
    public const string ShowTrack = "JEZOBRAZENAKOLEJ";

    /// <summary>Automatika bezi, aj ked stanicu neriadi ziadny klient.</summary>
    public const string KeepWithoutClient = "NEVYPINATBEZKLIENTA";
}

/// <summary>
/// Stavovy diagram stanice - statedgm.xml v priecinku stanice. Hodnoty vyrazov su kod VB.NET, server ich preklada pri starte.
/// </summary>
public static class StateDiagramFile
{
    /// <summary>
    /// Nazov suboru v priecinku stanice.
    /// </summary>
    public const string FileName = "statedgm.xml";

    // variant hlasenia, ked ho subor neurcuje (DefaultValue servera)
    private const int NoVariant = -1431655766;

    /// <summary>
    /// Nacita diagram; null = subor nie je.
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    public static StateDiagram? Read(string stationDir)
    {
        var file = Path.Combine(stationDir, FileName);
        if (!File.Exists(file))
            return null;

        var root = Iniss2Xml.Load(file).Root!;
        return new StateDiagram(Iniss2Xml.Text(root, "indg"))
        {
            Graphs = root.Elements("g").Select(graph => new StateGraph(Iniss2Xml.Text(graph, "ds"))
            {
                Nodes = graph.Elements("n").Select(ReadNode).ToList()
            }).ToList()
        };
    }

    /// <summary>
    /// Zapise diagram.
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    /// <param name="diagram">diagram</param>
    public static void Write(string stationDir, StateDiagram diagram)
    {
        ArgumentNullException.ThrowIfNull(diagram);
        Iniss2Xml.Save(Path.Combine(stationDir, FileName), new XElement("SD", new XAttribute("indg", diagram.GraphIndex),
            diagram.Graphs.Select(graph => new XElement("g", new XAttribute("ds", graph.Description), graph.Nodes.Select(WriteNode)))));
    }

    private static StateNode ReadNode(XElement element)
    {
        var button = element.Element("b");
        var automation = element.Element("a");
        return new StateNode(Iniss2Xml.Text(element, "k"))
        {
            Name = Iniss2Xml.Text(element, "n"),
            Report = Iniss2Xml.Text(element, "r"),
            Flags = Words(Iniss2Xml.Text(element, "f")),
            Condition = element.Element("c")?.Value ?? "",
            Button = button is null ? null : new StateButton(Iniss2Xml.Text(button, "t"), Iniss2Xml.Number(button, "p")),
            Automation = automation is null
                ? null
                : new StateAutomation(Iniss2Xml.Text(automation, "t"))
                {
                    Condition = automation.Element("c")?.Value ?? "",
                    Mode = Iniss2Xml.Text(automation, "m"),
                    Repeat = Iniss2Xml.Text(automation, "r"),
                    End = Iniss2Xml.Text(automation, "e"),
                    Variant = automation.Attribute("h") is null ? null : Iniss2Xml.Number(automation, "h"),
                    Flags = Words(Iniss2Xml.Text(automation, "f"))
                }
        };
    }

    private static XElement WriteNode(StateNode node)
    {
        var element = new XElement("n", new XAttribute("k", node.Key), Iniss2Xml.Optional("n", node.Name), Iniss2Xml.Optional("r", node.Report),
            Iniss2Xml.Optional("f", string.Join(' ', node.Flags)));
        if (node.Condition.Length > 0)
            element.Add(new XElement("c", node.Condition));
        if (node.Button is { } button)
            element.Add(new XElement("b", new XAttribute("t", button.Text), new XAttribute("p", button.Position)));
        if (node.Automation is { } automation)
        {
            element.Add(new XElement("a", new XAttribute("t", automation.Time), Iniss2Xml.Optional("m", automation.Mode),
                Iniss2Xml.Optional("r", automation.Repeat), Iniss2Xml.Optional("e", automation.End),
                automation.Variant is { } variant && variant != NoVariant ? new XAttribute("h", variant.ToString(CultureInfo.InvariantCulture)) : null,
                Iniss2Xml.Optional("f", string.Join(' ', automation.Flags)),
                automation.Condition.Length > 0 ? new XElement("c", automation.Condition) : null));
        }

        return element;
    }

    private static List<string> Words(string text) => [.. text.Split(' ', StringSplitOptions.RemoveEmptyEntries)];
}
