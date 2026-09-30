namespace ToolsCore.Iniss.Tools;

/// <summary>
/// Trieda reprezentujuca casovy interval medzi 2 <see cref="DateTime"/> objektami.
/// Pouzivane pre zistenie, ci sa 2 intervaly prekryvaju.
/// </summary>
public class Interval
{
    public Interval(DateTime from, DateTime to)
    {
        From = from;
        To = to;
    }

    /// <summary>
    /// Date from.
    /// </summary>
    public DateTime From { get; }

    /// <summary>
    /// Date to.
    /// </summary>
    public DateTime To { get; }

    /// <summary>
    /// Zisti, ci sa 2 casove intervaly prekryvaju.
    /// </summary>
    /// <param name="interval2">Druhy casovy interval.</param>
    /// <returns><see langword="true"/>, ak sa intervaly prekryvaju, inak <see langword="false"/>.</returns>
    public bool Overlaps(Interval interval2)
    {
        return From <= interval2.From ? To >= interval2.From : interval2.To >= From;
    }
}