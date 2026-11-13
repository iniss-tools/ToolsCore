namespace ToolsCore.Forms.Calendar;

internal class CalendarCellColumn : DataGridViewColumn
{
    public CalendarCellColumn()
    {
        base.CellTemplate = new CalendarCell();
    }
}
