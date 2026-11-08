namespace ToolsCore.Iniss.Registry;

/// <summary>
/// Predvolena hodnota nastavenia: cislo, text, alebo dynamicka (urci ju INISS podla okolnosti - trieda linky,
/// vyrobca tabule, nazov sekcie).
/// </summary>
public sealed record RegDefault
{
    private RegDefault(int? number, string? text, bool dynamic)
    {
        Number = number;
        Text = text;
        IsDynamic = dynamic;
    }

    /// <summary>Ciselna predvolba.</summary>
    public int? Number { get; }

    /// <summary>Textova predvolba.</summary>
    public string? Text { get; }

    /// <summary>Predvolbu urcuje program za behu (napr. podla triedy linky).</summary>
    public bool IsDynamic { get; }

    /// <summary>Bez predvolby (binarne hodnoty).</summary>
    public static RegDefault None { get; } = new(null, null, false);

    /// <summary>Dynamicka predvolba.</summary>
    public static RegDefault Dynamic { get; } = new(null, null, true);

    /// <summary>Ciselna predvolba.</summary>
    public static RegDefault FromNumber(int value) => new(value, null, false);

    /// <summary>Textova predvolba.</summary>
    public static RegDefault FromText(string value) => new(null, value, false);

    /// <summary>Hodnota predvolby (int, string alebo null).</summary>
    public object? Value => Number is { } n ? n : Text;
}
