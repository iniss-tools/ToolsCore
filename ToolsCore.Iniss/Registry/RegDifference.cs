namespace ToolsCore.Iniss.Registry;

/// <summary>
/// Rozdiel nastavenia medzi dvoma konfiguraciami (aktualnou a druhou - ina vetva, subor .reg alebo .INI).
/// </summary>
/// <param name="Section">sekcia</param>
/// <param name="Name">nazov hodnoty</param>
/// <param name="Type">typ</param>
/// <param name="Current">nastavenie v aktualnej konfiguracii (null = v nej nie je)</param>
/// <param name="Other">nastavenie v druhej konfiguracii (null = v nej nie je)</param>
public sealed record RegDifference(string Section, string Name, RegValueType Type, ResolvedSetting? Current, ResolvedSetting? Other)
{
    /// <summary>Hodnota je v druhej konfiguracii zapisana (nie predvolena).</summary>
    public bool OtherExplicit => Other is not null && RegTools.IsExplicit(Other.Source);

    /// <summary>Hodnota je v aktualnej konfiguracii zapisana (nie predvolena).</summary>
    public bool CurrentExplicit => Current is not null && RegTools.IsExplicit(Current.Source);

    /// <summary>Ucinne hodnoty su rovnake.</summary>
    public bool IsEqual => RegValues.AreEqual(Current?.Value, Other?.Value, Type);
}
