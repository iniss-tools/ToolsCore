using System.Globalization;
using System.Xml.Linq;

namespace ToolsCore.Iniss2.Grafikon;

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
