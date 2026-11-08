namespace ToolsCore.Iniss.Registry;

/// <summary>Jedna operacia na konkretnom mieste.</summary>
/// <param name="Location">miesto</param>
/// <param name="Section">sekcia</param>
/// <param name="Name">nazov hodnoty</param>
/// <param name="Value">nova hodnota; null = zmazat</param>
public sealed record RegWriteOp(RegLocation Location, string Section, string Name, RegRawValue? Value)
{
    /// <summary>Zalozenie kluca sekcie (prazdna sekcia = kluc aplikacie) bez hodnot.</summary>
    public bool IsKeyCreate { get; init; }

    /// <summary>Zmazanie hodnoty.</summary>
    public bool IsDelete => Value is null && !IsKeyCreate;

    /// <summary>Zmazanie celej sekcie (prazdny nazov hodnoty); pri prazdnej sekcii celej vetvy aplikacie.</summary>
    public bool IsSectionDelete => Name.Length == 0 && Value is null && !IsKeyCreate;

    /// <summary>Operacia, ktora zmaze celu sekciu na danom mieste.</summary>
    public static RegWriteOp DeleteSection(RegLocation location, string section) => new(location, section, "", null);

    /// <summary>Operacia, ktora zmaze celu vetvu aplikacie na danom mieste.</summary>
    public static RegWriteOp DeleteBranch(RegLocation location) => new(location, "", "", null);

    /// <summary>Operacia, ktora zalozi kluc sekcie (prazdna sekcia = kluc aplikacie).</summary>
    public static RegWriteOp CreateKey(RegLocation location, string section) => new(location, section, "", null) { IsKeyCreate = true };
}
