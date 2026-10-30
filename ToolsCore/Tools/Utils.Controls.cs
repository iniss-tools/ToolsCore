namespace ToolsCore.Tools;

/// <summary>
/// Pomocky pre ovladacie prvky.
/// </summary>
public static partial class Utils
{
    /// <summary>
    /// Zisti, ci je v DGV vybrany aspon 1 riadok.
    /// </summary>
    /// <param name="dgv"></param>
    /// <returns></returns>
    public static bool IsSelectionEmpty(this DataGridView dgv) => dgv.SelectedRows.Count == 0;
}
