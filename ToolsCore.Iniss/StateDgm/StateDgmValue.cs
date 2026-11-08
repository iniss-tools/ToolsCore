using System.Globalization;

namespace ToolsCore.Iniss.StateDgm;

/// <summary>
/// Hodnota v strome StateDgm.
/// </summary>
public sealed class StateDgmValue : StateDgmItem
{
    /// <summary>Vytvori textovu hodnotu.</summary>
    public StateDgmValue(string key, string text)
    {
        Kind = StateDgmValueKind.String;
        Key = key;
        Text = text;
    }

    /// <summary>Vytvori celociselnu hodnotu; <paramref name="raw" /> je povodny zapis (napr. <c>0x08</c>), ak sa ma zachovat.</summary>
    public StateDgmValue(string key, int number, string? raw = null)
    {
        Kind = StateDgmValueKind.Int;
        Key = key;
        Number = number;
        Raw = raw;
    }

    /// <summary>Vytvori pravdivostnu hodnotu.</summary>
    public StateDgmValue(string key, bool flag)
    {
        Kind = StateDgmValueKind.Bool;
        Key = key;
        Flag = flag;
    }

    private StateDgmValue(StateDgmValueKind kind, string key)
    {
        Kind = kind;
        Key = key;
    }

    /// <summary>Druh hodnoty.</summary>
    public StateDgmValueKind Kind { get; }

    /// <summary>Kluc.</summary>
    public string Key { get; }

    /// <summary>Text (pre <see cref="StateDgmValueKind.String" />).</summary>
    public string Text { get; set; } = "";

    /// <summary>Cislo (pre <see cref="StateDgmValueKind.Int" />).</summary>
    public int Number { get; set; }

    /// <summary>Povodny zapis cisla (napr. <c>0x18</c>), aby sa pri zapise zachoval; null = zapisat desiatkovo.</summary>
    public string? Raw { get; set; }

    /// <summary>Priznak (pre <see cref="StateDgmValueKind.Bool" />).</summary>
    public bool Flag { get; set; }

    /// <summary>
    /// Zapis <c>S:"kluc"=#</c> (bez hodnoty) - INISS nim kluc zo skupiny odstrani. V suboroch grafikonov sa nepouziva,
    /// ale parser ho prijme a zapisovac zachova.
    /// </summary>
    public bool IsRemoval { get; private init; }

    /// <summary>Vytvori znacku odstranenia kluca (<c>X:"kluc"=#</c>).</summary>
    public static StateDgmValue Removal(StateDgmValueKind kind, string key) => new(kind, key) { IsRemoval = true };

    /// <inheritdoc />
    public override StateDgmItem Clone()
    {
        var c = new StateDgmValue(Kind, Key) { IsRemoval = IsRemoval, Text = Text, Number = Number, Raw = Raw, Flag = Flag, Line = Line };
        return c;
    }

    /// <inheritdoc />
    public override string ToString() => Kind switch
    {
        StateDgmValueKind.String => $"S:\"{Key}\"=\"{Text}\"",
        StateDgmValueKind.Int => $"I:\"{Key}\"={Raw ?? Number.ToString(CultureInfo.InvariantCulture)}",
        _ => $"B:\"{Key}\"={(Flag ? "Ano" : "Ne")}"
    };
}
