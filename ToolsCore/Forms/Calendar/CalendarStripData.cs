namespace ToolsCore.Forms.Calendar;

/// <summary>
/// Dni variant v spolocnom obdobi pre <see cref="CalendarStrip" />. Den, v ktory ide viac variant naraz, je prekrytie.
/// </summary>
/// <param name="From">prvy den pruhu</param>
/// <param name="To">posledny den pruhu</param>
/// <param name="Rows">riadky v poradi variant</param>
public sealed record CalendarStripData(DateTime From, DateTime To, IReadOnlyList<CalendarStripRow> Rows)
{
    /// <summary>
    /// Pocet dni pruhu.
    /// </summary>
    public int Days => (To - From).Days + 1;

    /// <summary>
    /// V den <paramref name="day" /> ide viac variant naraz.
    /// </summary>
    public bool IsOverlap(int day) => Rows.Count(row => row.Runs[day]) > 1;
}
