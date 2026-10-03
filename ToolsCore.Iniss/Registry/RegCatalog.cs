namespace ToolsCore.Iniss.Registry;

/// <summary>
/// Katalog nastaveni INISSu v registri - sekcie a hodnoty podla dokumentacie (docs/iniss/registry.mdx).
/// Data su v <c>RegCatalog.g.cs</c>, ktory generuje <c>tools/gen_registry_catalog.py</c>.
/// </summary>
public static partial class RegCatalog
{
    /// <summary>Sekcie v poradi dokumentacie.</summary>
    public static IReadOnlyList<RegSection> Sections { get; } = Link(BuildSections());

    /// <summary>Nazvy, ktore program pozna, ale necita ziadna verzia (podla sekcie).</summary>
    public static IReadOnlyDictionary<string, string[]> UnusedNames { get; } = BuildUnusedNames();

    /// <summary>Nazvy pozostatkov po verziach spred roku 2006 - program ich vobec nepozna.</summary>
    public static IReadOnlyList<string> ObsoleteNames { get; } = BuildObsoleteNames();

    /// <summary>Pocet farieb v sekcii Colors.</summary>
    public const int ColorCount = 42;

    /// <summary>Sekcia podla nazvu; cislovane (<c>Driver3</c>, <c>Volume</c>) podla zakladu.</summary>
    public static RegSection? FindSection(string name)
    {
        foreach (var section in Sections)
        {
            if (string.Equals(section.Name, name, StringComparison.OrdinalIgnoreCase)) return section;
            if (section.IsNumbered && section.InstanceNames().Contains(name, StringComparer.OrdinalIgnoreCase)) return section;
        }

        return null;
    }

    /// <summary>Ci je nazov pozostatkom (nepouzivany alebo programu neznamy).</summary>
    public static bool IsLeftover(string section, string name) =>
        (UnusedNames.TryGetValue(section, out var unused) && unused.Contains(name, StringComparer.OrdinalIgnoreCase))
        || ObsoleteNames.Contains(name, StringComparer.OrdinalIgnoreCase);

    private static RegSection[] Link(RegSection[] sections)
    {
        foreach (var section in sections)
        foreach (var setting in section.Settings)
            setting.Section = section;
        return sections;
    }
}
