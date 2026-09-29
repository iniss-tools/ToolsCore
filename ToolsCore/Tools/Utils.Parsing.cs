using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace ToolsCore.Tools;

/// <summary>
/// Citanie a zapis cisel, casov, datumov a bitovych map v suboroch INISS.
/// </summary>
public static partial class Utils
{
    /// <summary>
    /// Konvertuje dlzku zvuku v milisekundach (ms) to textovej podoby "mm:ss".
    /// </summary>
    /// <param name="len">Dlzka zvuku v ms.</param>
    /// <returns>textovu podobu dlzky zvuku vo formate "mm:ss".</returns>
    public static string LengthIntToString(int len)
    {
        if (len < 0) return "--:--";
        var totalSecF = len / 1000f;
        var totalSec = (int)Math.Round(totalSecF);
        var mins = totalSec / 60;
        var zv = totalSec % 60;

        return $"{mins:D2}:{zv:D2}";
    }

    /// <summary>
    /// Konvertuje dlzku zvuku v textovej podobe "m:ss"/"mm:ss"/"h:mm:ss" do trvania ako cislo v milisekundach (ms).
    /// </summary>
    /// <param name="text">textovu podobu dlzky zvuku vo formate "mm:ss".</param>
    /// <returns>Dlzka zvuku v ms.</returns>
    public static int StringToLengthInt(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (text == "--:--")
            return -1;

        string[] timeformats = { @"m\:ss", @"mm\:ss", @"h\:mm\:ss" };
        if(TimeSpan.TryParseExact(text, timeformats, CultureInfo.InvariantCulture, out var duration))
            return (int)duration.TotalMilliseconds;
        else
            throw new ArgumentException("Neplatný formát času");
    }

    /// <summary>
    /// Vráti čislo ako reťazec doplnené o určitý počet 0 na začiatok. <br></br>
    /// Ak je pocet cifier menší ako 1, vráti číslo ako reťazec (bez žiadnych 0 pred začiatkom).
    /// </summary>
    /// <param name="num">Cislo.</param>
    /// <param name="pocetCifier">Pocet cifier.</param>
    /// <returns>reťazec s cislom.</returns>
    public static string PadZeros(this int num, int pocetCifier = 3)
    {
        if (pocetCifier <= 0) 
            return num.ToString(CultureInfo.InvariantCulture);
        var sb = new StringBuilder();
        for (var i = 0; i < pocetCifier; i++) 
            sb.Append('0');
        return num.ToString(sb.ToString(), CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Zistí, či je zadaná hodnota v reťazci celé číslo (<see cref="int"/>).
    /// </summary>
    /// <param name="num">Retazec s moznym cislom.</param>
    /// <returns>ci sa retazec da konverovat na cislo.</returns>
    public static bool IsInt(string num) => int.TryParse(num, NumberStyles.Integer, CultureInfo.InvariantCulture, out _);

    /// <summary>
    /// Vrati skonverované číslo z retazca, alebo ak sa nedal retazec skonvertovat vrati nastavenu predvolenu hodnotu.
    /// </summary>
    /// <param name="nums">Retazec s moznym cislom.</param>
    /// <param name="def">Predvolena hodnota.</param>
    /// <returns>skonverovane cislo alebo predvolenu hodnotu.</returns>
    public static int ParseIntOrDefault(string? nums, int def = 0) =>
        int.TryParse(nums, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numi) ? numi : def;

    /// <summary>
    /// Vráti skonvertované číslo z retazca,
    /// alebo ak sa nedal retazec skonvertovat vrati nastavenu predvolenu hodnotu (<see langword="null"/>).
    /// </summary>
    /// <param name="nums">Retazec s moznym cislom.</param>
    /// <param name="def">Predvolena hodnota.</param>
    /// <returns>skonverovane cislo alebo predvolenu hodnotu.</returns>
    public static int? ParseIntOrNull(string? nums, int? def = null) =>
        int.TryParse(nums, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numi) ? numi : def;

    /// <summary>
    /// Vrati retazec, ak je retazec <see langword="null" />, vrati predvoleny retazec.
    /// </summary>
    /// <param name="str">Retazec.</param>
    /// <param name="def">Predvoleny retazec.</param>
    /// <returns></returns>
    public static string ParseStringOrDefault(string? str, string def = "") => str ?? def;

    /// <summary>
    /// Vrati pole bitov ako <see cref="string"/>.
    /// </summary>
    /// <param name="bits">Pole bitov.</param>
    /// <returns>pole bitov ako <see cref="string"/>.</returns>
    public static string BitArrayToString(BitArray bits)
    {
        ArgumentNullException.ThrowIfNull(bits);

        var sb = new StringBuilder();

        for (var i = 0; i < bits.Count; i++) 
            sb.Append(bits[i] ? '1' : '0');

        return sb.ToString();
    }

    /// <summary>
    /// Konvertuje pole bitov (ako <see cref="string"/>) ako pole bitov <see cref="BitArray"/>.
    /// </summary>
    /// <param name="bits">Pole bitov.</param>
    /// <returns>pole bitov ako <see cref="BitArray"/>.</returns>
    public static BitArray StringToBitArray(string bits)
    {
        ArgumentNullException.ThrowIfNull(bits);

        return new BitArray(bits.Select(c => c == '1').ToArray());
    }

    /// <summary>
    /// Vrati 1 pre <see langword="true"/>, 0 pre <see langword="false"/>.
    /// </summary>
    /// <param name="hodnota">Hodnota <see langword="true"/> alebo <see langword="false"/>.</param>
    /// <returns>0 alebo 1.</returns>
    public static int ToNumber(this bool hodnota) => hodnota ? 1 : 0;

    /// <summary>
    /// Vrati <see langword="false"/> pre 0, inak vrati 1.
    /// </summary>
    /// <param name="hodnota">Hodnota ako cislo.</param>
    /// <returns><see langword="true"/> alebo <see langword="false"/>.</returns>
    public static bool ToBool(this int hodnota) => hodnota != 0;

    /// <summary>
    /// Prevedie reťazec <paramref name="text"/>, ktorý je vo formáte dátumu (dd.MM.yyyy) na <see cref="DateTime"/>.
    /// </summary>
    /// <param name="text">Retazec s datumom.</param>
    /// <returns>datum vo forme objektu typu <see cref="DateTime"/>.</returns>
    public static DateTime ParseDate(string text)
    {
        return string.IsNullOrEmpty(text) ?
            DateTime.MinValue : 
            DateTime.ParseExact(text, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None);
    }

    /// <summary>
    /// Prevedie reťazec <paramref name="text"/> na <see cref="DateTime"/>, pričom reťazec musí byť vo formáte:
    /// dd.MM.yyyy / d.MM.yyyy / d.M.yyyy / dd.M.yyyy .
    /// </summary>
    /// <param name="text">Retazec s datumom.</param>
    /// <returns>datum vo forme objektu typu <see cref="DateTime"/>.</returns>
    public static DateTime ParseDateAlts(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return DateTime.MinValue;

        return DateTime.ParseExact(text, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None);
    }

    /// <summary>
    /// Prevedie reťazec <paramref name="text"/>, ktorý je vo formáte času (HH:mm / H:mm) na <see cref="DateTime"/>.
    /// </summary>
    /// <param name="text">Retazec s casom.</param>
    /// <returns>cas vo forme objektu typu <see cref="DateTime"/>.</returns>
    public static DateTime ParseTime(string text)
    {
        return string.IsNullOrEmpty(text) ? 
            DateTime.MinValue : 
            DateTime.ParseExact(text, TimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.None);
    }

    private static readonly string[] DateFormats = ["dd.MM.yyyy", "d.MM.yyyy", "d.M.yyyy", "dd.M.yyyy"];

    private static readonly string[] TimeFormats = ["HH:mm", "H:mm"];

    /// <summary>
    /// Skúsi previesť reťazec <paramref name="text"/>, ktorý je vo formáte času (HH:mm / H:mm) na <see cref="DateTime"/>.
    /// </summary>
    /// <param name="text">Retazec s casom.</param>
    /// <param name="time">Cas vo forme objektu typu <see cref="DateTime"/> ak sa prevod podari, inak obsahuje <see cref="DateTime.MinValue"/>.</param>
    /// <returns><see langword="true" /> ak sa prevod podarí, inak vráti <see langword="false"/>.</returns>
    public static bool TryParseTime(string text, out DateTime time)
    {
        // prazdny text je platny (bez casu) - rovnako ako v ParseTime
        if (string.IsNullOrEmpty(text))
        {
            time = DateTime.MinValue;
            return true;
        }

        return DateTime.TryParseExact(text, TimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out time);
    }

    /// <summary>
    /// Skúsi previesť reťazec na DateTime, pričom reťazec musí byť vo formáte:
    /// dd.MM.yyyy / d.MM.yyyy / d.M.yyyy / dd.M.yyyy .
    /// </summary>
    /// <param name="text">Retazec s datumom.</param>
    /// <param name="date">Datum vo forme objektu typu <see cref="DateTime"/> ak sa prevod podari, inak obsahuje <see cref="DateTime.MinValue"/>.</param>
    /// <returns><see langword="true"/> ak sa prevod podarí, inak vráti <see langword="false"/>.</returns>
    public static bool TryParseDateAlts(string text, out DateTime date)
    {
        // prazdny text je platny (bez datumu) - rovnako ako v ParseDateAlts
        if (string.IsNullOrEmpty(text))
        {
            date = DateTime.MinValue;
            return true;
        }

        return DateTime.TryParseExact(text, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
    }

    /// <summary>
    /// Zisti, ci zadanany retazec <paramref name="str"/> je vo formate casu.
    /// </summary>
    /// <param name="str">Retazec na testovanie.</param>
    /// <returns></returns>
    public static bool IsTime(string str) => TryParseTime(str, out _);

    /// <summary>
    /// Datum ako <see cref="DateTime" /> o polnoci - pre prvky a vypocty, ktore pracuju s <see cref="DateTime" />
    /// (DateTimePicker, kalendar datumovych obmedzeni).
    /// </summary>
    public static DateTime ToDateTime(this DateOnly date) => date.ToDateTime(TimeOnly.MinValue);

    /// <summary>
    /// Datum v tvare dd.MM.yyyy (subory INISS) - bez ohladu na kulturu.
    /// </summary>
    public static DateOnly ParseDateOnlyAlts(string? text) => DateOnly.FromDateTime(ParseDateAlts(text));

    /// <summary>
    /// Zisti, ci zadany datum sa nachadza medzi dvoma datumami start a end.
    /// </summary>
    /// <param name="dt">hladany datum</param>
    /// <param name="start">zaciatok intervalu</param>
    /// <param name="end">koniec intervalu</param>
    /// <returns></returns>
    public static bool IsBewteenTwoDates(this DateTime dt, DateTime start, DateTime end)
    {
        return dt >= start && dt <= end;
    }

    /// <summary>
    /// Zisti, ci je riadok prazdny alebo obsahuje komentar alebo zacina mriezkou (#) (pouzitie v: <see cref="CsvRow"/>).
    /// </summary>
    /// <param name="ch">Typ zaciatku riadku.</param>
    /// <returns>Ci je riadok prazdny alebo obsahuje komentar alebo zacina mriezkou (#).</returns>
    [ExcludeFromCodeCoverage]
    public static bool LineIsEmpty(ReadStartChar ch) => ch is ReadStartChar.Semicolon or ReadStartChar.Empty or ReadStartChar.Slash;

    /// <summary>
    /// Zisti, ci je riadok posledny (pouzitie v: <see cref="CsvRow"/>)
    /// </summary>
    /// <param name="ch">Typ zaciatku riadku.</param>
    /// <returns>Ci riadok obsahuje koniec suboru (EOF).</returns>
    [ExcludeFromCodeCoverage]
    public static bool LineIsEOF(ReadStartChar ch) => ch == ReadStartChar.Eof;
}


/// <summary>
/// Typ znaku na zaciatku riadku.
/// </summary>
public enum ReadStartChar
{
    /// <summary>
    /// Neprazdny riadok.
    /// </summary>
    NonEmpty,

    /// <summary>
    /// Prazdny riadok.
    /// </summary>
    Empty,

    /// <summary>
    /// Bodkociarka.
    /// </summary>
    Semicolon,

    /// <summary>
    /// Mriezka (#).
    /// </summary>
    Slash,

    /// <summary>
    /// Znak konca suboru.
    /// </summary>
    Eof
}
