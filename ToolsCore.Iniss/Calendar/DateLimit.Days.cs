using System.Globalization;
using ToolsCore.Iniss.Properties;

namespace ToolsCore.Iniss.Calendar;

/// <summary>
/// Typy dni: sviatky SK/CZ, Velka noc, dni v tyzdni a ich skupiny.
/// </summary>
public partial class DateLimit
{
    /// <summary>Statne sviatky s pevnym datumom v tvare MMDD.</summary>
    private static readonly int[] FixedHolidaysCz =
    [
        101, 501, 508, 705, 706, 928, 1028, 1117, 1224, 1225, 1226
    ];

    /// <summary>Statne sviatky s pevnym datumom v tvare MMDD.</summary>
    private static readonly int[] FixedHolidaysSk =
    [
        101, 106, 501, 508, 705, 829, 901, 915, 1101, 1117, 1224, 1225, 1226
    ];

    /// <summary>
    /// Vrati, ci je zadany datum sviatok alebo nedela podla <see cref="Loc" />.
    /// </summary>
    public static bool IsHoliday(DateTime date) => IsHoliday(date, Loc);

    /// <summary>
    /// Vrati, ci je zadany datum sviatok alebo nedela v zadanej krajine.
    /// </summary>
    public static bool IsHoliday(DateTime date, Locale locale)
    {
        if (date.DayOfWeek == DayOfWeek.Sunday)
            return true;

        var fixedHolidays = locale switch
        {
            Locale.Cz => FixedHolidaysCz,
            Locale.Sk => FixedHolidaysSk,
            _ => throw new ArgumentOutOfRangeException(nameof(locale))
        };

        if (Array.IndexOf(fixedHolidays, date.Month * 100 + date.Day) >= 0)
            return true;

        // pohyblive sviatky (Velky piatok a Velkonocny pondelok) mozu pripadnut len na marec alebo april
        if (date.Month is not (3 or 4))
            return false;

        var easterMonday = GetEasterMonday(date.Year);

        return date.DayOfWeek switch
        {
            DayOfWeek.Friday => date.AddDays(3).Date == easterMonday,
            DayOfWeek.Monday => date.Date == easterMonday,
            _ => false
        };
    }

    /// <summary>
    /// Vrati, ci je token zlozeny len z pevnych kodov dni.
    /// </summary>
    private static bool IsDayType(string token) => token.ToUpper(CultureInfo.CurrentCulture).All(c => DayTypeSigns.Contains(c));

    /// <summary>
    /// Vrati, ci je token rozsah dni v tvare "1-5".
    /// </summary>
    private static bool IsDayRange(string token) =>
        token is [_, '-', _] && char.IsDigit(token[0]) && char.IsDigit(token[2]);

    /// <summary>
    /// Prevedie pevny kod dni ("1", "X+", "1-5") na priznaky typov dni.
    /// </summary>
    private DayType GetDayType(string token)
    {
        token = token.ToUpper(CultureInfo.CurrentCulture);

        if (IsDayType(token))
            return token.Aggregate(DayType.None, (current, c) => current | (DayType)(1 << DayTypeSigns.IndexOf(c)));

        var first = DayTypeSigns.IndexOf(token[0]);

        // rozsah dni - jeho zaciatok musi lezat pred nedelou
        if (token is not [_, '-', _] || first >= 6)
            throw new ParseException(string.Format(CultureInfo.CurrentCulture, Resources.DateLimit_BadDayCode, token), _position);
        
        var last = WeekDaySigns.IndexOf(token[2]);

        if (last > first)
        {
            var range = DayType.None;

            for (var i = first; i <= last; i++)
                range |= (DayType)(1 << i);

            return range;
        }

        throw new ParseException(string.Format(CultureInfo.CurrentCulture, Resources.DateLimit_BadDayCode, token), _position);
    }

    /// <summary>
    /// Vrati typ zadaneho dna - den v tyzdni a pripadne aj priznak pracovneho dna alebo sviatku.
    /// </summary>
    private DayType GetDayType(DateTime date, bool forceSpecDays = false)
    {
        var dayType = (DayType)(1 << (int)GetDayIndex(date));

        if (!_specDays && !forceSpecDays)
            return dayType;

        if (IsHoliday(date, HolidayLocale ?? Loc))
            dayType |= DayType.Holiday;
        else if (dayType <= DayType.Friday)
            dayType |= DayType.Workday;

        return dayType;
    }

    /// <inheritdoc cref="GetDayType(DateTime,bool)"/>
    private DayType GetDayType(int day, bool forceSpecDays = false) => GetDayType(DateFrom.AddDays(day), forceSpecDays);

    /// <summary>
    /// Vrati typy dni, ktore maju v pocitadle nenulovu hodnotu.
    /// </summary>
    private static DayType GetDayType(DayCounter okCount)
    {
        var dayType = DayType.None;

        for (var index = DayIndex.Monday; index <= DayIndex.Holiday; index++)
            if (okCount[index] != 0)
                dayType |= (DayType)(1 << (int)index);

        return dayType;
    }

    /// <summary>
    /// Vrati <paramref name="dayType"/>, ak sa v obdobi vyskytuje aspon jeden den mimo tychto typov,
    /// inak <see cref="DayType.None"/> - typy dni potom netreba v poznamke uvadzat.
    /// </summary>
    private DayType CheckDayType(int dayFrom, int dayTo, DayType dayType)
    {
        while (dayFrom <= dayTo)
        {
            if ((GetDayType(dayFrom) & dayType) == DayType.None)
                return dayType;

            dayFrom++;
        }

        return DayType.None;
    }

    /// <summary>
    /// Prevedie datum na index dna v tyzdni, kde pondelok je 0.
    /// </summary>
    private static DayIndex GetDayIndex(DateTime date) => (DayIndex)date.AddDays(-1).DayOfWeek;

    /// <summary>
    /// Vrati index dna pouzity pri porovnavani. Pri zluceni na pracovne dni a sviatky vrati
    /// <see cref="DayIndex.Workday"/> alebo <see cref="DayIndex.Holiday"/> namiesto dna v tyzdni.
    /// </summary>
    private DayIndex GetDayIndex(int day, DayGrouping grouping)
    {
        var date = DateFrom.AddDays(day);
        var index = GetDayIndex(date);

        if (grouping == DayGrouping.None || !_specDays)
            return index;

        if (index == DayIndex.Saturday && (grouping & DayGrouping.KeepSaturday) != 0)
            return index;

        if (IsHoliday(date, HolidayLocale ?? Loc))
            return DayIndex.Holiday;

        return index == DayIndex.Saturday ? index : DayIndex.Workday;
    }

    /// <summary>
    /// Vrati index nasledujuceho dna.
    /// Ak je <paramref name="day"/> <see cref="DayIndex.Sunday"/>, vrati <see cref="DayIndex.Monday"/>.
    /// </summary>
    /// <param name="day">Index dna.</param>
    /// <returns>Index nasledujuceho dna.</returns>
    private static DayIndex GetNextDayIndex(DayIndex day) => day >= DayIndex.Sunday ? DayIndex.Monday : day + 1;

    /// <summary>
    /// Vrati datum Velkonocneho pondelka v zadanom roku (Gaussov velkonocny algoritmus).
    /// </summary>
    private static DateTime GetEasterMonday(int year)
    {
        var a = year % 19;
        var b = year / 100;
        var c = year % 100;
        var d = b / 4;
        var e = b % 4;
        var f = c / 4;
        var g = c % 4;
        var h = (8 * b + 13) / 25;
        var i = (19 * a + b - d - h + 15) % 30;
        var j = (a + 11 * i) / 319;
        var k = (2 * e + 2 * f - g - i + j + 32) % 7;

        var month = (i - j + k + 91) / 25;
        var day = (i - j + k + 20 + month) % 32;

        return new DateTime(year, month, day);
    }

    /// <summary>
    /// Sposob, akym sa dni zlucuju pri hladani tyzdenneho vzoru.
    /// </summary>
    [Flags]
    private enum DayGrouping
    {
        /// <summary>
        /// Dni sa posudzuju podla dna v tyzdni.
        /// </summary>
        None = 0,

        /// <summary>
        /// Dni sa zlucuju na pracovne dni a sviatky.
        /// </summary>
        WorkdayHoliday = 1,

        /// <summary>
        /// Sobota sa aj napriek zluceniu posudzuje samostatne.
        /// </summary>
        KeepSaturday = 32
    }

    /// <summary>
    /// Pocitadlo dni pre kazdu polozku <see cref="DayIndex"/>.
    /// </summary>
    private sealed class DayCounter
    {
        private readonly int[] _counts = new int[DayIndexCount];

        public int this[DayIndex index]
        {
            get => _counts[(int)index];
            set => _counts[(int)index] = value;
        }

        /// <summary>
        /// Vrati, ci su vsetky pocitadla nulove.
        /// </summary>
        public bool AllZero => _counts.All(count => count == 0);

        public void Clear() => Array.Clear(_counts, 0, _counts.Length);
    }

    [Flags]
    private enum DayType
    {
        None = 0,
        Monday = 1,
        Tuesday = 2,
        Wednesday = 4,
        Thursday = 8,
        Friday = 16,
        Saturday = 32,
        Sunday = 64,
        Workday = 128,
        Holiday = 256,

        /// <summary>Vsetky dni v tyzdni.</summary>
        All1 = 127,

        /// <summary>Sobota, pracovne dni a sviatky.</summary>
        All2 = 416
    }

    private enum DayIndex
    {
        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday,
        Saturday,
        Sunday,
        Workday,
        Holiday
    }
}
