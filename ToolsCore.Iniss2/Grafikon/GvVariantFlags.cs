namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Priznaky varianty vlaku. Nazvy zodpovedaju klucom v subore.
/// </summary>
[Flags]
public enum GvVariantFlags
{
    /// <summary>Bez priznakov.</summary>
    None = 0,

    /// <summary>Kluc O.</summary>
    O = 1,

    /// <summary>Kluc M.</summary>
    M = 2,

    /// <summary>Kluc R.</summary>
    R = 4,

    /// <summary>Kluc L.</summary>
    L = 8,

    /// <summary>Kluc D - s takou variantou sa spravy GTN neparuju.</summary>
    D = 0x10,

    /// <summary>Kluc X.</summary>
    X = 0x20,

    /// <summary>Kluc N.</summary>
    N = 0x40,

    /// <summary>Kluc NP.</summary>
    NP = 0x80
}
