namespace ToolsCore.Iniss.Registry;

/// <summary>Vstup vyhodnotenia - nacitane vrstvy jednej konfiguracie INISSu.</summary>
public sealed record InissConfigSource
{
    /// <summary>Nazov vetvy pod CHAPS (meno exe bez pripony alebo hodnota /Reg:).</summary>
    public required string AppName { get; init; }

    /// <summary>Verzia INISSu z exe; null = neznama (berie sa ako najnovsia).</summary>
    public RegVersion? Version { get; init; }

    /// <summary>Ako sa INISS spusta.</summary>
    public InissRunMode RunMode { get; init; }

    /// <summary>HKCU\Software\CHAPS\aplikacia.</summary>
    public RegBranch User { get; init; } = RegBranch.Missing;

    /// <summary>HKLM\SOFTWARE\(WOW6432Node\)CHAPS\aplikacia.</summary>
    public RegBranch Machine { get; init; } = RegBranch.Missing;

    /// <summary>VirtualStore kopia HKLM pre aktualneho pouzivatela.</summary>
    public RegBranch VirtualStore { get; init; } = RegBranch.Missing;

    /// <summary>Subor .INI vedla programu alebo null.</summary>
    public InissIniFile? Ini { get; init; }

    /// <summary>Nazvy 42 farieb z jazykovej kniznice RCIniss.dll; null = slovenske nazvy z katalogu.</summary>
    public IReadOnlyList<string>? ColorNames { get; init; }

    /// <summary>
    /// Fyzicke tabule podla indexu &lt;N&gt; v sekcii Tables (poradie v spojenom zozname tabul, ktory INISS nacita).
    /// Ich hodnoty sa ukazu aj ked v registri nie su; vyrobca urci predvoleny vynuteny jas.
    /// </summary>
    public IReadOnlyDictionary<int, RegTableInfo>? Tables { get; init; }
}
