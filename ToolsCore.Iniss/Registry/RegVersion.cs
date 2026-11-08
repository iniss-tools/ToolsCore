using System.Globalization;

namespace ToolsCore.Iniss.Registry;

/// <summary>
/// Verzia INISSu (hlavna.vedlajsia) - porovnavacie body katalogu su 3.0, 3.10, 3.34 a 3.39.
/// </summary>
/// <param name="Major">hlavna verzia</param>
/// <param name="Minor">vedlajsia verzia ako cislo (3.10 = 10, nie 1)</param>
public readonly record struct RegVersion(int Major, int Minor) : IComparable<RegVersion>
{
    /// <inheritdoc />
    public int CompareTo(RegVersion other) => Major != other.Major ? Major.CompareTo(other.Major) : Minor.CompareTo(other.Minor);

    /// <summary>Mensia verzia.</summary>
    public static bool operator <(RegVersion a, RegVersion b) => a.CompareTo(b) < 0;

    /// <summary>Vacsia verzia.</summary>
    public static bool operator >(RegVersion a, RegVersion b) => a.CompareTo(b) > 0;

    /// <summary>Mensia alebo rovnaka verzia.</summary>
    public static bool operator <=(RegVersion a, RegVersion b) => a.CompareTo(b) <= 0;

    /// <summary>Vacsia alebo rovnaka verzia.</summary>
    public static bool operator >=(RegVersion a, RegVersion b) => a.CompareTo(b) >= 0;

    /// <summary>
    /// Verzia z textu suboru exe (<c>3.39</c>, <c>3.34.6</c>, <c>3.00</c>); null, ak text nie je verzia.
    /// </summary>
    public static RegVersion? Parse(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var parts = text.Trim().Split('.');
        if (parts.Length < 2
            || !int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out var major)
            || !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var minor))
            return null;
        return new RegVersion(major, minor);
    }

    /// <inheritdoc />
    public override string ToString() => string.Create(CultureInfo.InvariantCulture, $"{Major}.{Minor:00}").Replace(".00", ".0", StringComparison.Ordinal);
}
