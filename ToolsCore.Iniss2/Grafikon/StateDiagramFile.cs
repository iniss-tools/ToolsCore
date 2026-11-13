using System.Globalization;
using System.Xml.Linq;

namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Stavovy diagram stanice - statedgm.xml v priecinku stanice. Hodnoty vyrazov su kod VB.NET, server ich preklada pri starte.
/// Server subor nikdy nezapisuje.
/// </summary>
public static class StateDiagramFile
{
    /// <summary>
    /// Nazov suboru v priecinku stanice.
    /// </summary>
    public const string FileName = "statedgm.xml";

    /// <summary>
    /// Variant hlasenia automatiky, ked ho subor neurcuje (DefaultValue servera).
    /// </summary>
    public const int NoVariant = -1431655766;

    /// <summary>
    /// Nacita diagram; null = subor nie je. Text vyrazov ostane presne ako v subore (aj medzery a riadky).
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    public static StateDiagram? Read(string stationDir)
    {
        var file = Path.Combine(stationDir, FileName);
        return File.Exists(file) ? Parse(Iniss2Xml.Load(file).Root!) : null;
    }

    /// <summary>
    /// Zapise diagram. Existujuci subor sa upravi na mieste (komentare a nezmenene stavy ostanu po znakoch).
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    /// <param name="diagram">diagram</param>
    /// <returns><see langword="false" />, ak subor uz mal tento obsah.</returns>
    public static bool Write(string stationDir, StateDiagram diagram)
    {
        ArgumentNullException.ThrowIfNull(diagram);
        return Iniss2Xml.Write(Path.Combine(stationDir, FileName), Build(diagram), root => Build(Parse(root)));
    }

    private static StateDiagram Parse(XElement root) => new(Iniss2Xml.Text(root, "indg"))
    {
        Graphs = root.Elements("g").Select(graph => new StateGraph(Iniss2Xml.Text(graph, "ds"))
        {
            Nodes = graph.Elements("n").Select(ParseNode).ToList()
        }).ToList()
    };

    private static StateNode ParseNode(XElement element)
    {
        var condition = element.Element("c");
        var button = element.Element("b");
        var automation = element.Element("a");
        var automationCondition = automation?.Element("c");
        return new StateNode(Iniss2Xml.Text(element, "k"))
        {
            Name = Iniss2Xml.Text(element, "n"),
            Report = Iniss2Xml.Text(element, "r"),
            Flags = Words(Iniss2Xml.Text(element, "f")),
            Condition = condition?.Value ?? "",
            ConditionName = condition is null ? "" : Iniss2Xml.Text(condition, "n"),
            Button = button is null ? null : new StateButton(Iniss2Xml.Text(button, "t"), Iniss2Xml.Number(button, "p")),
            Automation = automation is null
                ? null
                : new StateAutomation(Iniss2Xml.Text(automation, "t"))
                {
                    Condition = automationCondition?.Value ?? "",
                    ConditionName = automationCondition is null ? "" : Iniss2Xml.Text(automationCondition, "n"),
                    Mode = Iniss2Xml.Text(automation, "m"),
                    Repeat = Iniss2Xml.Text(automation, "r"),
                    End = Iniss2Xml.Text(automation, "e"),
                    Variant = automation.Attribute("h") is null || Iniss2Xml.Number(automation, "h") == NoVariant ? null : Iniss2Xml.Number(automation, "h"),
                    Flags = Words(Iniss2Xml.Text(automation, "f"))
                }
        };
    }

    // atributy a prvky v poradi XmlSerializer servera (StateDgm.Node, Auto, Condition, Button), predvolene hodnoty sa nepisu
    private static XElement Build(StateDiagram diagram) => new("SD", new XAttribute("indg", diagram.GraphIndex),
        diagram.Graphs.Select(graph => new XElement("g", Iniss2Xml.Optional("ds", graph.Description), graph.Nodes.Select(BuildNode))));

    private static XElement BuildNode(StateNode node)
    {
        var element = new XElement("n", new XAttribute("k", node.Key), Iniss2Xml.Optional("n", node.Name), Iniss2Xml.Optional("r", node.Report),
            Iniss2Xml.Optional("f", string.Join(' ', node.Flags)));
        if (node.Condition.Length > 0)
            element.Add(Condition(node.ConditionName, node.Condition));
        if (node.Button is { } button)
            element.Add(new XElement("b", Iniss2Xml.Optional("t", button.Text), new XAttribute("p", button.Position)));
        if (node.Automation is { } automation)
        {
            element.Add(new XElement("a", new XAttribute("t", automation.Time), Iniss2Xml.Optional("m", automation.Mode),
                Iniss2Xml.Optional("r", automation.Repeat), Iniss2Xml.Optional("e", automation.End),
                automation.Variant is { } variant ? new XAttribute("h", variant.ToString(CultureInfo.InvariantCulture)) : null,
                Iniss2Xml.Optional("f", string.Join(' ', automation.Flags)),
                automation.Condition.Length > 0 ? Condition(automation.ConditionName, automation.Condition) : null));
        }

        return element;
    }

    private static XElement Condition(string name, string code) => new("c", Iniss2Xml.Optional("n", name), code);

    private static List<string> Words(string text) => [.. text.Split(' ', StringSplitOptions.RemoveEmptyEntries)];
}
