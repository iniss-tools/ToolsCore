using System.Globalization;
using ToolsCore.Properties;

namespace ToolsCore.Commands;

/// <summary>
/// Klavesove skratky: platnost, text pre pouzivatela a zapis do nastaveni.
/// </summary>
public static class KeyNames
{
    private static readonly KeysConverter Converter = new();

    /// <summary>
    /// Ci sa kombinacia da pouzit ako skratka prikazu - rovnako ako skratka polozky ponuky (s Ctrl alebo Alt, alebo
    /// samostatny funkcny kláves, Insert a Delete).
    /// </summary>
    public static bool IsValid(Keys keys) => keys != Keys.None && ToolStripManager.IsValidShortcut(keys);

    /// <summary>
    /// Text skratky pre pouzivatela (napr. <c>Ctrl+Shift+K</c>); bez skratky „(Žiadna)“.
    /// </summary>
    public static string Display(Keys keys) =>
        keys == Keys.None ? Resources.Shortcut_None : Converter.ConvertToString(null, CultureInfo.CurrentCulture, keys) ?? "";

    /// <summary>
    /// Text skratky v nastaveniach: nazov z <see cref="Shortcut" />, ak ho ma (citatelny aj staršími verziami programu),
    /// inak kombinacia klavesov (<c>Ctrl+Alt+K</c>).
    /// </summary>
    public static string ToSetting(Keys keys) =>
        Enum.IsDefined((Shortcut)keys) ? ((Shortcut)keys).ToString() : Converter.ConvertToInvariantString(keys) ?? "";

    /// <summary>
    /// Precita skratku z nastaveni (nazov z <see cref="Shortcut" /> alebo kombinaciu klavesov); neplatnu odmietne.
    /// </summary>
    public static bool TryParseSetting(string? text, out Keys keys)
    {
        keys = Keys.None;
        // cislo by Enum.TryParse prijal ako hodnotu
        if (string.IsNullOrWhiteSpace(text) || char.IsDigit(text[0]) || text[0] == '-')
            return false;

        if (Enum.TryParse<Shortcut>(text, out var shortcut) && Enum.IsDefined(shortcut))
        {
            keys = (Keys)shortcut;
            return true;
        }

        try
        {
            if (Converter.ConvertFromInvariantString(text) is Keys parsed && IsValid(parsed))
            {
                keys = parsed;
                return true;
            }
        }
        catch (ArgumentException)
        {
            // neznamy nazov klavesu
        }
        catch (FormatException)
        {
        }

        return false;
    }
}
