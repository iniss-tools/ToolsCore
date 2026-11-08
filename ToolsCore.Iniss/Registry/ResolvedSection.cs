namespace ToolsCore.Iniss.Registry;

/// <summary>Vyhodnotena sekcia (pri cislovanych jedna konkretna, napr. <c>Driver3</c>).</summary>
public sealed class ResolvedSection
{
    internal ResolvedSection(RegSection definition, string name, bool fromIni)
    {
        Definition = definition;
        Name = name;
        FromIni = fromIni;
    }

    /// <summary>Sekcia z katalogu.</summary>
    public RegSection Definition { get; }

    /// <summary>Nazov podkluca.</summary>
    public string Name { get; }

    /// <summary>Cislovana sekcia je v .INI - register sa pre nu necita.</summary>
    public bool FromIni { get; }

    /// <summary>Nastavenia.</summary>
    public IReadOnlyList<ResolvedSetting> Settings { get; internal set; } = [];

    /// <summary>Hodnoty, ktore katalog nepozna (pozostatky, preklepy, farby inej jazykovej verzie).</summary>
    public IReadOnlyList<RegLayerValue> Extra { get; internal set; } = [];

    /// <summary>Zistenia k sekcii a k hodnotam mimo katalogu.</summary>
    public IReadOnlyList<RegDiagnostic> Diagnostics { get; internal set; } = [];

    /// <summary>Nastavenie podla nazvu.</summary>
    public ResolvedSetting? Find(string name) => Settings.FirstOrDefault(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));

    /// <inheritdoc />
    public override string ToString() => Name;
}
