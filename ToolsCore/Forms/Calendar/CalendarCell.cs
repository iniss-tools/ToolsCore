namespace ToolsCore.Forms.Calendar;

internal class CalendarCell : DataGridViewTextBoxCell
{
    /// <summary>
    /// Vrati/nastavi absolutny index dna. Pouziva sa na vyber hodnoty z BitArray, ci je dany den vybrany do datumoveho obmedzenia.
    /// </summary>
    public int AbsoluteIndex { get; set; } = -1;

    /// <summary>
    /// Vrati/nastavi ci je dany den reprezentovany ako bunka vybrany do datumoveho obmedzenia.
    /// </summary>
    public bool SelectedDay { get; set; }

    /// <summary>
    /// Vrati/nastavi ci sa ma na danom policku zobrazit oramovanie - zvyraznenie posledne vybrateho dna
    /// </summary>
    public bool Highlighted { get; set; }

    /// <summary>
    /// Farba oramovania zvyrazneneho policka (podla temy).
    /// </summary>
    public Color HighlightColor { get; set; } = Color.Black;

    /// <summary>
    /// Vrati/nastavi akciu resp. stav, v akom sa policko nachadza.
    /// </summary>
    public CalendarCellAction Action { get; set; }

    protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex,
        DataGridViewElementStates cellState, object? value, object? formattedValue, string? errorText,
        DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle,
        DataGridViewPaintParts paintParts)
    {
        var foreColor = cellStyle.ForeColor;
        var backColor = cellStyle.BackColor;

        if (Action == CalendarCellAction.Selection || (Action != CalendarCellAction.Deselection && SelectedDay))
        {
            cellStyle.ForeColor = cellStyle.SelectionForeColor;
            cellStyle.BackColor = cellStyle.SelectionBackColor;
        }

        base.Paint(graphics, clipBounds, cellBounds, rowIndex, cellState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);
        
        cellStyle.ForeColor = foreColor;
        cellStyle.BackColor = backColor;

        if (Highlighted)
        {
            using var pen = new Pen(HighlightColor, 2);
            graphics.DrawRectangle(pen, cellBounds.X + 1, cellBounds.Y + 1, cellBounds.Width - 2, cellBounds.Height - 2);
        }
    }
}
