namespace ToolsCore.Tools;

/// <summary>
/// Pomocky pre ovladacie prvky.
/// </summary>
public static partial class Utils
{
    /// <summary>
    /// Skontroluje, či zadaná klávesová stkratka je správna.
    /// </summary>
    /// <param name="keys">klávesy</param>
    /// <returns></returns>
    public static bool ValidateShortcut(Keys keys)
    {
        var values = Enum.GetValues<Shortcut>();
        return values.Cast<object>().Any(val => (int)val == (int)keys);
    }

    /// <summary>
    /// Zisti, ci je v DGV vybrany aspon 1 riadok.
    /// </summary>
    /// <param name="dgv"></param>
    /// <returns></returns>
    public static bool IsSelectionEmpty(this DataGridView dgv) => dgv.SelectedRows.Count == 0;
}
