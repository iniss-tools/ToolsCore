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
        RegValueType.String => RegRawValue.String(Convert.ToString(value, CultureInfo.InvariantCulture) ?? ""),
        RegValueType.Binary => RegRawValue.Binary((byte[])value),
        _ => RegRawValue.Dword(Convert.ToInt32(value, CultureInfo.InvariantCulture))
    };

    /// <summary>Text hodnoty do suboru .INI (cisla desiatkovo, bajty sestnastkovo bez oddelovacov).</summary>
    public static string ToIniText(object value, RegValueType type) => type switch
    {
        RegValueType.String => Convert.ToString(value, CultureInfo.InvariantCulture) ?? "",
        RegValueType.Binary => Convert.ToHexString((byte[])value),
        _ => Convert.ToInt32(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture)
    };
}
