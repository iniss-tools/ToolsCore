namespace ToolsCore.Tools;

/// <summary>
/// Tabulky na pracovnej ploche programov.
/// </summary>
public static partial class FormUtils
{
    /// <summary>
    /// Posledny viditelny stlpec v poradi zobrazenia (nie v poradi kolekcie - pouzivatel moze stlpce presuvat a skryvat).
    /// </summary>
    public static DataGridViewColumn? LastVisibleColumn(this DataGridView grid) =>
        grid.Columns.GetLastColumn(DataGridViewElementStates.Visible, DataGridViewElementStates.None);

    /// <summary>
    /// Posledny viditelny stlpec vyplni zvysnu sirku tabulky; stlpec, ktory ju vyplnal doteraz, dostane pevnu sirku.
    /// </summary>
    public static void FillLastVisibleColumn(this DataGridView grid)
    {
        var last = grid.LastVisibleColumn();
        foreach (DataGridViewColumn column in grid.Columns)
        {
            if (column == last)
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            else if (column.AutoSizeMode == DataGridViewAutoSizeColumnMode.Fill)
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
        }
    }
}
