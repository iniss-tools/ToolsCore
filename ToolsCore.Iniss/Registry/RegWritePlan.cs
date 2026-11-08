namespace ToolsCore.Iniss.Registry;

/// <summary>
/// Operacie, ktore zmeny zapisu tak, aby mali u INISSu ucinok v oboch rezimoch spustenia.
/// </summary>
public sealed class RegWritePlan
{
    /// <summary>Plan z hotovych operacii (nastroje: klon, cistenie, vetva HKCU).</summary>
    public RegWritePlan(IReadOnlyList<RegWriteOp> ops) : this(ops, [], [])
    {
    }

    internal RegWritePlan(IReadOnlyList<RegWriteOp> ops, IReadOnlyList<RegChange> iniStillOverrides, IReadOnlyList<RegChange> registryNotRead)
    {
        Ops = ops;
        IniStillOverrides = iniStillOverrides;
        RegistryNotRead = registryNotRead;
    }

    /// <summary>Operacie v poradi vykonania.</summary>
    public IReadOnlyList<RegWriteOp> Ops { get; }

    /// <summary>Zmeny do registra, ktore aj po zapise prebije .INI (odstranenie zo suboru nebolo povolene).</summary>
    public IReadOnlyList<RegChange> IniStillOverrides { get; }

    /// <summary>Zmeny do registra v cislovanej sekcii zo suboru .INI - INISS ich necita.</summary>
    public IReadOnlyList<RegChange> RegistryNotRead { get; }

    /// <summary>Plan zapisuje do HKLM (potrebuje prava spravcu).</summary>
    public bool WritesMachine => Ops.Any(o => o.Location == RegLocation.Machine);

    /// <summary>Plan meni subor .INI.</summary>
    public bool WritesIni => Ops.Any(o => o.Location == RegLocation.Ini);
}
