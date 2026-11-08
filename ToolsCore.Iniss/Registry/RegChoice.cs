using System.Globalization;

namespace ToolsCore.Iniss.Registry;

/// <summary>Hodnota, ktoru program rozlisuje, s popisom (text v <see cref="RegTexts" />).</summary>
/// <param name="Value">hodnota tak, ako je v dokumentacii (<c>0</c>, <c>1–65535</c>, <c>*</c>…)</param>
/// <param name="TextKey">kluc popisu</param>
public sealed record RegChoice(string Value, string TextKey)
{
    /// <summary>Presne cislo, ak hodnota je jedno cislo (inak rozsah alebo slovny popis).</summary>
    public int? Number => int.TryParse(Value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var n) ? n : null;

    /// <summary>Popis hodnoty v jazyku UI.</summary>
    public string Text => RegTexts.Get(TextKey);
}
