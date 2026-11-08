namespace ToolsCore.Iniss.Registry;

/// <summary>
/// Vyhodnotene nastavenie - ucinna hodnota, odkial pochadza a co lezi v jednotlivych vrstvach.
/// </summary>
public sealed class ResolvedSetting
{
    internal ResolvedSetting(RegSetting setting, string section, string name)
    {
        Setting = setting;
        Section = section;
        Name = name;
    }

    /// <summary>Nastavenie z katalogu.</summary>
    public RegSetting Setting { get; }

    /// <summary>Nazov sekcie (pri cislovanych konkretny, napr. <c>Driver3</c>).</summary>
    public string Section { get; }

    /// <summary>Nazov hodnoty (pri sablonach konkretny, napr. <c>Enabled3</c>; pri farbach nazov z jazykovej kniznice).</summary>
    public string Name { get; }

    /// <summary>Ucinna hodnota (int, string, byte[]) alebo null, ak ju editor nevie urcit.</summary>
    public object? Value { get; internal set; }

    /// <summary>Odkial ucinna hodnota pochadza.</summary>
    public RegSource Source { get; internal set; }

    /// <summary>Predvolena hodnota v tomto kontexte (pri Driver podla triedy) alebo null, ak je dynamicka.</summary>
    public object? DefaultValue { get; internal set; }

    /// <summary>Fyzicka tabula pri hodnotach Tables\…&lt;N&gt; (ak je znama).</summary>
    public RegTableInfo? Table { get; internal set; }

    /// <summary>Koren registra, z ktoreho INISS hodnotu cita a kam ju zapisuje (User alebo Machine).</summary>
    public RegLocation RegistryLocation { get; internal set; }

    /// <summary>Hodnoty vo vrstvach (od najvyssej priority).</summary>
    public IReadOnlyList<RegLayerValue> Layers { get; internal set; } = [];

    /// <summary>Zistenia k tomuto nastaveniu.</summary>
    public IReadOnlyList<RegDiagnostic> Diagnostics { get; internal set; } = [];

    /// <summary>Verzia INISSu hodnotu cita.</summary>
    public bool IsRead => Source != RegSource.NotRead;

    /// <summary>Ucinna hodnota sa rovna predvolenej (aj ked lezi v registri).</summary>
    public bool IsDefault => DefaultValue is not null && RegValues.AreEqual(Value, DefaultValue, Setting.Type);

    /// <inheritdoc />
    public override string ToString() => $"{Section}\\{Name} = {Value} ({Source})";
}
