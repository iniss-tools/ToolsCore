using System.Globalization;

namespace ToolsCore.Iniss.Registry;

/// <summary>Porovnanie a prevod hodnot nastaveni podla typu.</summary>
public static class RegValues
{
    /// <summary>Farba "bez vlastnej farby" - pouzije sa systemova.</summary>
    public const int SystemColor = unchecked((int)0xFE000000);

    /// <summary>Ci su dve hodnoty pre INISS rovnake (bool: nula / nenula, text: presne, bajty: obsah).</summary>
    public static bool AreEqual(object? a, object? b, RegValueType type)
    {
        if (a is null || b is null) return a is null && b is null;
        return type switch
        {
            RegValueType.Bool when a is int x && b is int y => x != 0 == (y != 0),
            RegValueType.Binary when a is byte[] x && b is byte[] y => x.AsSpan().SequenceEqual(y),
            _ => Equals(a, b)
        };
    }

    /// <summary>Surova hodnota do registra pre hodnotu nastavenia (int alebo string podla typu).</summary>
    public static RegRawValue ToRaw(object value, RegValueType type) => type switch
    {
        RegValueType.String => RegRawValue.String(System.Convert.ToString(value, CultureInfo.InvariantCulture) ?? ""),
        RegValueType.Binary => RegRawValue.Binary((byte[])value),
        _ => RegRawValue.Dword(System.Convert.ToInt32(value, CultureInfo.InvariantCulture))
    };

    /// <summary>Ci ma surova hodnota z registra typ, ktory INISS pri nastaveni tohto typu cita.</summary>
    public static bool Matches(RegValueType type, RegRawValue raw) => type switch
    {
        RegValueType.String => raw.Kind == RegRawKind.String,
        RegValueType.Binary => raw.Kind == RegRawKind.Binary,
        _ => raw.Kind == RegRawKind.Dword
    };

    /// <summary>
    /// Prevod hodnoty zleho typu na spravny, ak sa da bez straty (text s cislom na REG_DWORD, cislo na text,
    /// sestnastkovy text na REG_BINARY); inak null.
    /// </summary>
    public static RegRawValue? Convert(RegValueType type, RegRawValue raw)
    {
        if (Matches(type, raw)) return raw;
        switch (type)
        {
            case RegValueType.String when raw.Kind == RegRawKind.Dword:
                return RegRawValue.String(raw.Number.ToString(CultureInfo.InvariantCulture));
            case RegValueType.Binary when raw.Kind == RegRawKind.String && raw.Text is { Length: > 0 } hex && hex.Length % 2 == 0 && hex.All(char.IsAsciiHexDigit):
                return RegRawValue.Binary(System.Convert.FromHexString(hex));
            case RegValueType.Dword or RegValueType.Bool or RegValueType.Color when raw.Kind == RegRawKind.String && raw.Text is { } text:
                text = text.Trim();
                if (int.TryParse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var number)) return RegRawValue.Dword(number);
                if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                    && uint.TryParse(text.AsSpan(2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var hexNumber))
                    return RegRawValue.Dword(unchecked((int)hexNumber));
                return null;
            default:
                return null;
        }
    }

    /// <summary>Text hodnoty do suboru .INI (cisla desiatkovo, bajty sestnastkovo bez oddelovacov).</summary>
    public static string ToIniText(object value, RegValueType type) => type switch
    {
        RegValueType.String => System.Convert.ToString(value, CultureInfo.InvariantCulture) ?? "",
        RegValueType.Binary => System.Convert.ToHexString((byte[])value),
        _ => System.Convert.ToInt32(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture)
    };
}
