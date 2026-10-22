using System.Globalization;

namespace ToolsCore.Iniss.Updates;

/// <summary>
/// Verzia vydania X.Y alebo X.Y.Z (znacka vX.Y). Zostava ju ma ako X.Y.Z.0 - porovnava sa bez revizie a chybajuca
/// cast je 0 (1.10 = 1.10.0).
/// </summary>
public static class ReleaseVersion
{
    /// <summary>Precita verziu vydania X.Y alebo X.Y.Z.</summary>
    public static bool TryParse(string? text, out Version version)
    {
        version = new Version(0, 0, 0);
        var parts = (text ?? "").Trim().Split('.');
        if (parts.Length is < 2 or > 3)
            return false;
        var numbers = new int[3];
        for (var i = 0; i < parts.Length; i++)
        {
            if (!int.TryParse(parts[i], NumberStyles.None, CultureInfo.InvariantCulture, out numbers[i]))
                return false;
        }
        version = new Version(numbers[0], numbers[1], numbers[2]);
        return true;
    }

    /// <summary>Verzia bez revizie s nulami namiesto chybajucich casti (1.10 = 1.10.0).</summary>
    public static Version Normalize(Version version)
    {
        ArgumentNullException.ThrowIfNull(version);
        return new Version(version.Major, version.Minor, Math.Max(version.Build, 0));
    }

    /// <summary>Verzia ako v znacke vydania: X.Y, tretia cast len ak nie je 0 (1.10.0.0 = 1.10).</summary>
    public static string Format(Version version)
    {
        ArgumentNullException.ThrowIfNull(version);
        return version.Build > 0
            ? string.Create(CultureInfo.InvariantCulture, $"{version.Major}.{version.Minor}.{version.Build}")
            : string.Create(CultureInfo.InvariantCulture, $"{version.Major}.{version.Minor}");
    }
}
