namespace ToolsCore.XML;

/// <summary>
/// Casti polozky farebnej schemy, ktore moze pouzivatel menit.
/// </summary>
[Flags]
public enum ColorParts
{
    /// <summary>Nic.</summary>
    None = 0,

    /// <summary>Farba popredia (textu).</summary>
    Fore = 1,

    /// <summary>Farba pozadia.</summary>
    Back = 2,

    /// <summary>Tucne pismo.</summary>
    Bold = 4,

    /// <summary>Popredie a pozadie.</summary>
    ForeBack = Fore | Back
}
