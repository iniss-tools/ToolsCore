namespace ToolsCore.Iniss.Registry;

/// <summary>Vyhodnotena konfiguracia jednej vetvy INISSu.</summary>
public sealed class ResolvedConfig
{
    internal ResolvedConfig(InissConfigSource source, bool userBranchActive, IReadOnlyList<ResolvedSection> sections, IReadOnlyList<RegDiagnostic> unknownSections)
    {
        Source = source;
        UserBranchActive = userBranchActive;
        Sections = sections;
        UnknownSections = unknownSections;
    }

    /// <summary>Vstup.</summary>
    public InissConfigSource Source { get; }

    /// <summary>Vetva HKCU existuje - per-user sekcie sa citaju z HKCU.</summary>
    public bool UserBranchActive { get; }

    /// <summary>Sekcie v poradi katalogu.</summary>
    public IReadOnlyList<ResolvedSection> Sections { get; }

    /// <summary>Sekcie v registri alebo .INI, ktore katalog nepozna.</summary>
    public IReadOnlyList<RegDiagnostic> UnknownSections { get; }

    /// <summary>Vsetky zistenia.</summary>
    public IEnumerable<RegDiagnostic> Diagnostics =>
        UnknownSections.Concat(Sections.SelectMany(s => s.Diagnostics.Concat(s.Settings.SelectMany(v => v.Diagnostics))));

    /// <summary>Sekcia podla nazvu podkluca.</summary>
    public ResolvedSection? FindSection(string name) => Sections.FirstOrDefault(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));

    /// <summary>Nastavenie podla sekcie a nazvu.</summary>
    public ResolvedSetting? Find(string section, string name) => FindSection(section)?.Find(name);
}
