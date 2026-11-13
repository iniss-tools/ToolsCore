namespace ToolsCore.Forms.Calendar;

/// <summary>
/// Riadok pruhu kalendara - jedna varianta.
/// </summary>
/// <param name="Position">poradie varianty (od 1)</param>
/// <param name="Runs">den od <see cref="CalendarStripData.From" /> - varianta ide</param>
/// <param name="Invalid">obmedzenie sa neda precitat - riadok nema ziadny den</param>
/// <param name="IsOwn">upravovana varianta - tucny popis a vlastna farba</param>
/// <param name="Tag">objekt programu, ktoremu riadok patri (napr. vlak varianty)</param>
public sealed record CalendarStripRow(int Position, bool[] Runs, bool Invalid, bool IsOwn, object? Tag = null);
