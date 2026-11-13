using System.Collections;
using Serilog;
using ToolsCore.Iniss.Calendar;

namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Datumove poznamky grafikonu.
/// </summary>
public static class GvValidity
{
    /// <summary>Najdlhsie obdobie v dnoch, do ktoreho server poznamku rozvinie.</summary>
    public const int MaxDays = 400;

    /// <summary>
    /// Prevedie poznamku na dni rovnako ako server: prazdna plati kazdy den, <c>-</c> ziadny, poznamka moze byt aj
    /// retazec nul a jednotiek s dlzkou obdobia. Chybna poznamka a obdobie nad 400 dni znamenaju ziadny den.
    /// </summary>
    /// <param name="from">prvy den obdobia</param>
    /// <param name="to">posledny den obdobia</param>
    /// <param name="text">poznamka</param>
    /// <returns>bit <c>i</c> plati pre den <paramref name="from" /> plus <c>i</c> dni.</returns>
    public static BitArray Days(DateOnly from, DateOnly to, string? text)
    {
        var (days, problem) = Expand(from, to, text);
        if (problem != GvValidityProblem.None)
            Log.Warning("Datumovu poznamku {Text} sa nepodarilo rozvinut do obdobia {From} - {To}", text, from, to);
        return days;
    }

    /// <summary>
    /// Ci server poznamku do obdobia rozvinie - pozri <see cref="Days" />. Nic nezapisuje do logu.
    /// </summary>
    /// <param name="from">prvy den obdobia</param>
    /// <param name="to">posledny den obdobia</param>
    /// <param name="text">poznamka</param>
    public static GvValidityProblem Check(DateOnly from, DateOnly to, string? text) => Expand(from, to, text).Problem;

    private static (BitArray Days, GvValidityProblem Problem) Expand(DateOnly from, DateOnly to, string? text)
    {
        var count = Math.Max(0, to.DayNumber - from.DayNumber + 1);

        if (string.IsNullOrEmpty(text))
            return (new BitArray(count, true), GvValidityProblem.None);
        if (text is "-" or "--")
            return (new BitArray(count, false), GvValidityProblem.None);

        if (count > MaxDays)
            return (new BitArray(count), GvValidityProblem.PeriodTooLong);
        if (count == 0)
            return (new BitArray(count), GvValidityProblem.Invalid);

        if (text.Length == count && text != "1" && text.All(c => c is '0' or '1'))
        {
            var bits = new BitArray(count);
            for (var i = 0; i < count; i++)
                bits[i] = text[i] == '1';
            return (bits, GvValidityProblem.None);
        }

        try
        {
            // server pocita sviatky vzdy podla ceskeho kalendara, aj na slovenskych staniciach
            var days = new DateLimit(from, to, insertMarks: false) { HolidayLocale = DateLimit.Locale.Cz }.TextToBitArray(text);
            return days.Length == count ? (days, GvValidityProblem.None) : (new BitArray(count), GvValidityProblem.Invalid);
        }
        catch (DateLimit.ParseException)
        {
            return (new BitArray(count), GvValidityProblem.Invalid);
        }
    }

    /// <summary>
    /// Poznamka pre server: text po cesky, ako ho pise server aj klient (slovensku poznamku server precita tiez). Ked by ho
    /// server rozvinul na ine dni, retazec nul a jednotiek.
    /// </summary>
    /// <param name="from">prvy den obdobia</param>
    /// <param name="to">posledny den obdobia</param>
    /// <param name="days">dni obdobia; pri obdobi nad <see cref="MaxDays" /> dni server poznamku nerozvinie</param>
    public static string Text(DateOnly from, DateOnly to, BitArray days)
    {
        ArgumentNullException.ThrowIfNull(days);

        if (days.HasAllSet())
            return "";
        if (!days.HasAnySet())
            return "-";

        var bits = string.Concat(days.Cast<bool>().Select(day => day ? '1' : '0'));
        if (days.Length > MaxDays)
            return bits;

        // texty poznamok sa beru z jazyka nastaveneho pre cely program
        var locale = DateLimit.Loc;
        DateLimit.Loc = DateLimit.Locale.Cz;
        try
        {
            var text = new DateLimit(from, to, insertMarks: false) { HolidayLocale = DateLimit.Locale.Cz }.BitArrayToText(days);
            return new BitArray(Days(from, to, text)).Xor(days).HasAnySet() ? bits : text;
        }
        finally
        {
            DateLimit.Loc = locale;
        }
    }
}
