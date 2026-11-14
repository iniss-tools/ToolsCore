namespace ToolsCore.Iniss2.Grafikon.Script;

/// <summary>
/// Nazvy oznaceni druhov hlaseni v kode a co ktory druh prijima (podla tried oznaceni servera).
/// </summary>
public static class ScriptReportKinds
{
    private const string AttributeSuffix = "Attribute";

    // nazov, ktory zapisuje generator (ako v korpuse); pri citani plati aj s priponou Attribute alebo bez nej
    private static readonly (ScriptReportKind Kind, string Name)[] Names =
    [
        (ScriptReportKind.Basic, "ZakladniHlaseni"),
        (ScriptReportKind.Supplementary, "DoplnkoveHlaseni"),
        (ScriptReportKind.Other, "OstatniHlaseni"),
        (ScriptReportKind.Addition, "Dodatek"),
        (ScriptReportKind.Exclusion, "Vyluka"),
        (ScriptReportKind.Diversion, "Odklon"),
        (ScriptReportKind.AdditionPattern, "VzorDodatku"),
        (ScriptReportKind.ExclusionPattern, "VzorVyluky"),
        (ScriptReportKind.DiversionPattern, "VzorOdklonu"),
        (ScriptReportKind.Generic, "InterpreterAttribute")
    ];

    /// <summary>Druhy v poradi, v akom ich server pozna.</summary>
    public static IReadOnlyList<ScriptReportKind> All { get; } = [.. Names.Select(item => item.Kind)];

    /// <summary>
    /// Nazov oznacenia, ktory pre druh zapisuje generator.
    /// </summary>
    public static string AttributeName(ScriptReportKind kind) =>
        Names.First(item => item.Kind == kind).Name;

    /// <summary>
    /// Druh podla nazvu oznacenia v kode: bez ohladu na velkost pismen, s priponou <c>Attribute</c> aj bez nej a s mennym
    /// priestorom (<c>IniSrv.Dodatek</c>).
    /// </summary>
    public static bool TryParse(string attribute, out ScriptReportKind kind)
    {
        ArgumentNullException.ThrowIfNull(attribute);
        var name = Short(attribute);
        foreach (var item in Names)
        {
            if (!string.Equals(Short(item.Name), name, StringComparison.OrdinalIgnoreCase))
                continue;
            kind = item.Kind;
            return true;
        }

        kind = default;
        return false;
    }

    /// <summary>
    /// Oznacenie prijima obmedzenie na stanice <c>ProStanice</c> a <c>MimoStanice</c> (trieda servera
    /// <c>StationsLimitIntrprAttribute</c>).
    /// </summary>
    public static bool HasStationLimit(ScriptReportKind kind) =>
        kind is ScriptReportKind.Basic or ScriptReportKind.Supplementary or ScriptReportKind.Other
            or ScriptReportKind.AdditionPattern or ScriptReportKind.ExclusionPattern or ScriptReportKind.DiversionPattern;

    /// <summary>
    /// Vzor: funkcia ma parameter so zoznamom stanic, ktory dosadi server z hlasenia obsluhy.
    /// </summary>
    public static bool IsPattern(ScriptReportKind kind) =>
        kind is ScriptReportKind.AdditionPattern or ScriptReportKind.ExclusionPattern or ScriptReportKind.DiversionPattern;

    // nazov bez menneho priestoru a pripony Attribute
    private static string Short(string attribute)
    {
        var name = attribute.Trim();
        var dot = name.LastIndexOf('.');
        if (dot >= 0)
            name = name[(dot + 1)..];
        return name.Length > AttributeSuffix.Length && name.EndsWith(AttributeSuffix, StringComparison.OrdinalIgnoreCase)
            ? name[..^AttributeSuffix.Length]
            : name;
    }
}
