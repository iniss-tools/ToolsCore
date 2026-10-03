using System.Globalization;
using System.Resources;

namespace ToolsCore.Iniss.Registry;

/// <summary>
/// Popisy sekcii a nastaveni registra v jazyku UI (<c>RegTexts.resx</c> slovensky, <c>RegTexts.cs.resx</c> cesky).
/// Kluce generuje <c>tools/gen_registry_catalog.py</c> spolu s katalogom.
/// </summary>
public static class RegTexts
{
    private static readonly ResourceManager Manager = new("ToolsCore.Iniss.Registry.RegTexts", typeof(RegTexts).Assembly);

    /// <summary>Text podla kluca v aktualnej kulture UI; chybajuci kluc vrati ako text.</summary>
    public static string Get(string key) => Manager.GetString(key, CultureInfo.CurrentUICulture) ?? key;

    /// <summary>Text podla kluca v zadanej kulture (testy).</summary>
    public static string? GetIn(string key, CultureInfo culture) => Manager.GetString(key, culture);
}
