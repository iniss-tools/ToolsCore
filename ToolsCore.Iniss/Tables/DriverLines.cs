using System.Globalization;
using ToolsCore.Iniss.Registry;

namespace ToolsCore.Iniss.Tables;

/// <summary>
/// Priradenie fyzickych tabul k linkam ako v INISSe 3.39: linka dostane cislo z TablePort (COMn alebo predpona N=),
/// cislo musi byt 1-100 a pri duplicite INISS dalsiu linku ignoruje. Tabula s COMUNICATION_PORT=N ide na linku N,
/// ak jej protokol vyrobcu tabule prijima; tabula s 0 sa priradi automaticky linke s vlastnou rodinou protokolu
/// (prvej v poradi). Tabule s ID=-1 sa nikam neposielaju.
/// </summary>
public static class DriverLines
{
    /// <summary>Linky a priradenie tabul.</summary>
    public static DriverLineMap Build(ResolvedConfig config, IReadOnlyList<InissTable> tables)
    {
        var drivers = config.Sections.Where(s => s.Definition.Name == "Driver")
            .Select(s => (s.Name, Port: s.Find("TablePort")?.Value as string ?? DriverDefaults.DefaultPort,
                Class: s.Find("TableClass")?.Value as int? ?? DriverDefaults.DefaultClass))
            .ToList();

        // linky, ktore INISS naozaj zalozi
        var active = new Dictionary<int, string>();
        var problems = drivers.ToDictionary(d => d.Name, _ => new List<DriverLineProblem>());
        foreach (var d in drivers)
        {
            var line = DriverClasses.LineNumber(d.Port);
            if (line is null or < 1 or > 100)
                problems[d.Name].Add(DriverLineProblem.Of(RegSeverity.Error, DriverLineProblemCode.LineOutOfRange, line ?? 0));
            else if (active.TryGetValue(line.Value, out var first))
                problems[d.Name].Add(DriverLineProblem.Of(RegSeverity.Error, DriverLineProblemCode.LineDuplicate, line, first));
            else
                active[line.Value] = d.Name;
        }

        var assigned = drivers.ToDictionary(d => d.Name, _ => new List<LineTable>());
        var unserved = new List<UnservedTable>();
        foreach (var t in tables.Where(t => t.Table.ID != -1))
        {
            var port = t.Table.CommunicationPort;
            var manufacturer = t.Manufacturer ?? -1;
            if (port == 0)
            {
                // automaticky: linka s najlepsou zhodou (vlastna rodina), pri rovnosti prva
                var best = drivers.Where(d => active.ContainsValue(d.Name) && DriverClasses.AcceptsAutomatically(d.Class, manufacturer))
                    .OrderByDescending(d => DriverClasses.Score(d.Class, manufacturer)).Select(d => d.Name).FirstOrDefault();
                if (best is null) unserved.Add(new UnservedTable(t, DriverLineProblem.Of(RegSeverity.Warning, DriverLineProblemCode.NoAutomaticLine)));
                else assigned[best].Add(new LineTable(t, true));
                continue;
            }

            if (!active.TryGetValue(port, out var driver))
            {
                unserved.Add(new UnservedTable(t, DriverLineProblem.Of(RegSeverity.Warning, DriverLineProblemCode.NoDriver, port)));
                continue;
            }

            var cls = drivers.First(d => d.Name == driver).Class;
            if (DriverClasses.Accepts(cls, manufacturer))
            {
                assigned[driver].Add(new LineTable(t, false));
                continue;
            }

            var reason = DriverLineProblem.Of(RegSeverity.Warning, DriverLineProblemCode.WrongFamily, t.Table.Key, t.Table.TableCatalog?.Manufacturer?.Name,
                DriverClasses.Find(cls)?.Name ?? cls.ToString(CultureInfo.InvariantCulture));
            problems[driver].Add(reason);
            unserved.Add(new UnservedTable(t, reason));
        }

        var lines = drivers.Select(d =>
        {
            var list = problems[d.Name];
            var isActive = active.ContainsValue(d.Name);
            if (isActive && assigned[d.Name].Count == 0 && d.Class is not (128 or 129))
                list.Add(DriverLineProblem.Of(RegSeverity.Info, DriverLineProblemCode.NoTables));
            return new DriverLine(d.Name, DriverClasses.LineNumber(d.Port), d.Class, d.Port, isActive, assigned[d.Name], list);
        }).ToList();
        return new DriverLineMap(lines, unserved);
    }

    /// <summary>Prve volne cislo linky (1-100): linka, ktoru tabule v datach pouzivaju a nema ovladac, inak najmensie volne.</summary>
    public static int FreeLine(DriverLineMap map, IReadOnlyList<InissTable> tables)
    {
        var used = map.Lines.Select(l => l.Line).OfType<int>().ToHashSet();
        foreach (var port in tables.Where(t => t.Table.ID != -1).Select(t => t.Table.CommunicationPort).Distinct().Order())
            if (port is >= 1 and <= 100 && !used.Contains(port))
                return port;
        for (var i = 1; i <= 100; i++)
            if (!used.Contains(i))
                return i;
        return 1;
    }
}
