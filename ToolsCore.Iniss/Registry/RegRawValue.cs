using System.Globalization;

namespace ToolsCore.Iniss.Registry;

/// <summary>
/// Surova hodnota z registra tak, ako tam je (pred vyhodnotenim podla typu nastavenia).
/// </summary>
public sealed record RegRawValue
{
    private RegRawValue(RegRawKind kind, int number, string? text, byte[]? bytes, string? otherKind)
    {
        Kind = kind;
        Number = number;
        Text = text;
        Bytes = bytes;
        OtherKind = otherKind;
    }

    /// <summary>Druh.</summary>
    public RegRawKind Kind { get; }

    /// <summary>Cislo pri REG_DWORD.</summary>
    public int Number { get; }

    /// <summary>Text pri REG_SZ.</summary>
    public string? Text { get; }

    /// <summary>Bajty pri REG_BINARY.</summary>
    public byte[]? Bytes { get; }

    /// <summary>Nazov typu pri <see cref="RegRawKind.Other" /> (napr. <c>REG_EXPAND_SZ</c>).</summary>
    public string? OtherKind { get; }

    /// <summary>REG_DWORD.</summary>
    public static RegRawValue Dword(int value) => new(RegRawKind.Dword, value, null, null, null);

    /// <summary>REG_SZ.</summary>
    public static RegRawValue String(string value) => new(RegRawKind.String, 0, value, null, null);

    /// <summary>REG_BINARY.</summary>
    public static RegRawValue Binary(byte[] value) => new(RegRawKind.Binary, 0, null, value, null);

    /// <summary>Iny typ.</summary>
    public static RegRawValue Other(string kindName, string? display = null) => new(RegRawKind.Other, 0, display, null, kindName);

    /// <summary>Hodnota ako objekt (int, string, byte[]).</summary>
    public object? Value => Kind switch
    {
        RegRawKind.Dword => Number,
        RegRawKind.String => Text,
        RegRawKind.Binary => Bytes,
        _ => Text
    };

    /// <inheritdoc />
    public bool Equals(RegRawValue? other) =>
        other is not null && Kind == other.Kind && Number == other.Number && Text == other.Text && OtherKind == other.OtherKind
        && (Bytes is null ? other.Bytes is null : other.Bytes is not null && Bytes.AsSpan().SequenceEqual(other.Bytes));

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Kind, Number, Text, OtherKind, Bytes?.Length);

    /// <inheritdoc />
    public override string ToString() => Kind switch
    {
        RegRawKind.Dword => Number.ToString(CultureInfo.InvariantCulture),
        RegRawKind.String => Text ?? "",
        RegRawKind.Binary => Convert.ToHexString(Bytes ?? []),
        _ => OtherKind ?? ""
    };
}
