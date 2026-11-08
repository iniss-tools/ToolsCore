namespace ToolsCore.Iniss.StateDgm;

/// <summary>
/// Obrazky tlacidla - hodnota kluca <c>Bitmaps</c> v tvare <c>posun-normalny,so zameranim,stlaceny</c>.
/// </summary>
/// <param name="Offset">Posun v pase obrazkov.</param>
/// <param name="Normal">Obrazok pre bezny stav.</param>
/// <param name="Focused">Obrazok pre tlacidlo so zameranim.</param>
/// <param name="Pushed">Obrazok pre stlacene tlacidlo.</param>
public readonly record struct StateDgmBitmaps(int Offset, int Normal, int Focused, int Pushed)
{
    /// <summary>Rozlozi text kluca <c>Bitmaps</c>; false pri nespravnom tvare.</summary>
    public static bool TryParse(string? text, out StateDgmBitmaps value)
    {
        value = default;
        if (string.IsNullOrWhiteSpace(text)) return false;
        var dash = text.IndexOf('-');
        if (dash <= 0) return false;
        if (!int.TryParse(text[..dash].Trim(), out var offset)) return false;
        var parts = text[(dash + 1)..].Split(',');
        if (parts.Length != 3) return false;
        if (!int.TryParse(parts[0].Trim(), out var n) || !int.TryParse(parts[1].Trim(), out var f) || !int.TryParse(parts[2].Trim(), out var p)) return false;
        value = new StateDgmBitmaps(offset, n, f, p);
        return true;
    }

    /// <summary>Tlacidlo sa neda stlacit (vsetky tri obrazky rovnake).</summary>
    public bool IsStatic => Normal == Focused && Focused == Pushed;

    /// <summary>Indexy obrazkov v pase (posun + cislo).</summary>
    public (int Normal, int Focused, int Pushed) Absolute => (Offset + Normal, Offset + Focused, Offset + Pushed);

    /// <inheritdoc />
    public override string ToString() => $"{Offset}-{Normal},{Focused},{Pushed}";
}
