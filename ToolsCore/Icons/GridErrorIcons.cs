namespace ToolsCore.Icons;

/// <summary>
/// Ikona chyby buniek a riadkov <see cref="DataGridView" /> (<see cref="DataGridViewCell.ErrorText" />,
/// <see cref="DataGridViewRow.ErrorText" />) z banky <see cref="AppIcons" /> namiesto vstavanej systemovej ikony.
/// Napaja ju tema mriezky (<c>FormUtils.ApplyDataGridView</c>), takze plati pre vsetky mriezky programov.
/// </summary>
public static class GridErrorIcons
{
    // okraj ikony od pravej strany bunky - ako vstavana ikona, aby sedela s miestom, ktore bunka necha volne pre ikonu
    private const int Margin = 4;

    /// <summary>
    /// Napoji kreslenie ikony chyby na mriezku; opakovane volanie (napr. pri zmene temy) nevadi.
    /// </summary>
    public static void Attach(DataGridView grid)
    {
        ArgumentNullException.ThrowIfNull(grid);
        grid.CellPainting -= PaintErrorIcon;
        grid.CellPainting += PaintErrorIcon;
    }

    private static void PaintErrorIcon(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        // bunku uz nakreslila vlastna obsluha okna, alebo nema chybu
        if (e.Handled || sender is not DataGridView grid || e.Graphics is null || string.IsNullOrEmpty(e.ErrorText)
            || (e.PaintParts & DataGridViewPaintParts.ErrorIcon) == 0 || (e.ColumnIndex < 0 ? !grid.ShowRowErrors : !grid.ShowCellErrors))
            return;

        e.Paint(e.ClipBounds, e.PaintParts & ~DataGridViewPaintParts.ErrorIcon);

        var size = grid.LogicalToDeviceUnits(IconImages.Size);
        var selected = (e.State & DataGridViewElementStates.Selected) != 0;
        var back = e.CellStyle is null ? grid.BackgroundColor : selected ? e.CellStyle.SelectionBackColor : e.CellStyle.BackColor;
        if (AppIcons.Get("status-error", null, size, AppIcons.IsDarkBackground(back)) is { } icon)
        {
            var margin = grid.LogicalToDeviceUnits(Margin);
            var x = grid.RightToLeft == RightToLeft.Yes ? e.CellBounds.Left + margin : e.CellBounds.Right - margin - size;
            e.Graphics.DrawImage(icon, new Rectangle(x, e.CellBounds.Top + (e.CellBounds.Height - size) / 2, size, size));
        }

        e.Handled = true;
    }
}
