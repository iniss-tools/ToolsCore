using ExControls;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Text;
using System.Globalization;
using Microsoft.VisualBasic.FileIO;
using Vanara.Windows.Shell;
using SearchOption = System.IO.SearchOption;

// ReSharper disable UnusedMethodReturnValue.Global

namespace ToolsCore.Tools;

public static class Utils
{
    /// <summary>
    ///     Skonveruje pole bytov kodovany v ANSI (Windows 1250) na UTF8.
    /// </summary>
    /// <param name="data">Pole bytov.</param>
    /// <returns>skonverovane pole bytov.</returns>
    [ExcludeFromCodeCoverage]
    public static string ANSItoUTF(this byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (data.Length == 0)
            return "";

        return Encoding.UTF8.GetString(Encoding.Convert(Encodings.Win1250, Encodings.UTF8, data));
    }

    /// <summary>
    ///     Skonvertuje retazec kodovany v ANSI (Windows 1250) na UTF8.
    /// </summary>
    /// <param name="data">Retazec.</param>
    /// <returns>skonvetovany retazec.</returns>
    [ExcludeFromCodeCoverage]
    public static string ANSItoUTF(this string data)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (data.Length == 0)
            return "";

        return Encoding.UTF8.GetString(Encoding.Convert(Encodings.Win1250, Encoding.UTF8, Encodings.Win1250.GetBytes(data)));
    }

    /// <summary>
    ///     Skonveruje pole bytov kodovany v UTF8 na ANSI (Windows 1250).
    /// </summary>
    /// <param name="data">Pole bytov.</param>
    /// <returns>skonverovane pole bytov.</returns>
    [ExcludeFromCodeCoverage]
    public static string UTFtoANSI(this byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (data.Length == 0)
            return "";

        return Encodings.Win1250.GetString(Encoding.Convert(Encoding.UTF8, Encodings.Win1250, data));
    }

    /// <summary>
    ///     Skonvertuje retazec kódovaný v UTF8 na ANSI (Windows 1250).
    /// </summary>
    /// <param name="data">Retazec.</param>
    /// <returns>skonvetovany retazec.</returns>
    [ExcludeFromCodeCoverage]
    public static string UTFtoANSI(this string data)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (data.Length == 0)
            return "";

        return Encodings.Win1250.GetString(Encoding.Convert(Encoding.UTF8, Encodings.Win1250, Encoding.UTF8.GetBytes(data)));
    }

    /// <summary>
    ///     Skombinuje cestu k suborom/priecinkom.
    /// </summary>
    /// <param name="paths">pole retazcov s cestami k suborom/priecinkom.</param>
    /// <returns>skombinovanú cestu.</returns>
    public static string CombinePath(params string[] paths)
    {
        if (paths.Length == 0) return null;

        if (paths[0].Length == 0) return paths[0];

        for (var i = 1; i < paths.Length; i++)
        {
            paths[i] = paths[i].TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        return Path.Combine(paths);
    }

    /// <summary>
    ///     Vrati cestu k projektu zadanu ako argument prikazoveho riadka (napr. pri spusteni zo zoznamu odkazov
    ///     na paneli uloh). Prepinace zacinajuce znakom / alebo - sa preskakuju.
    /// </summary>
    /// <returns>cesta k projektu alebo <see langword="null"/>, ak nebola zadana.</returns>
    [ExcludeFromCodeCoverage]
    public static string GetProjectPathFromArgs()
    {
        string path = null;
        foreach (var arg in Environment.GetCommandLineArgs().Skip(1))
            if (!arg.StartsWith("/") && !arg.StartsWith("-"))
                path = arg;

        return path;
    }

    /// <summary>
    ///     Otvorí URL, mailto odkaz, súbor alebo priečinok cez asociovaný shell handler (predvolený prehliadač,
    ///     poštový klient, Prieskumník...).
    /// </summary>
    /// <param name="target">URL, mailto: odkaz, cesta k súboru alebo priečinku.</param>
    public static void OpenShell(string target) => Process.Start(new ProcessStartInfo(target) { UseShellExecute = true });

    /// <summary>
    ///     Reštartuje program.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static void RestartApp()
    {
        Application.Restart();
        Log.Info("Program sa reštartuje\r\n");
        Environment.Exit(0);
    }

    /// <summary>
    ///     Zobrazi chybovu hlasku s tlacidlom OK a ikonou cerveneho kriza.
    /// </summary>
    /// <param name="text">Text spravy.</param>
    /// <param name="buttons">Tlacidla, ktore sa zobrazia v dialogu.</param>
    /// <returns>vysledok dialogu.</returns>
    [ExcludeFromCodeCoverage]
    public static void ShowError([Localizable(true)] string text, MessageBoxButtons buttons = MessageBoxButtons.OK)
    {
        ExMessageBox.Show(text, GlobalResources.RError, buttons, MessageBoxIcon.Error);
    }

    /// <summary>
    ///     Zobrazi dialog s otazkou a ikonou bieleho otaznika v modrom kruhu.
    /// </summary>
    /// <param name="text">Text spravy.</param>
    /// <param name="buttons">Tlacidla, ktore sa zobrazia v dialogu.</param>
    /// <returns>vysledok dialogu.</returns>
    [ExcludeFromCodeCoverage]
    public static DialogResult ShowQuestion([Localizable(true)] string text, MessageBoxButtons buttons = MessageBoxButtons.YesNo)
    {
        return ExMessageBox.Show(text, GlobalResources.RQuestion, buttons, MessageBoxIcon.Question);
    }

    /// <summary>
    ///     Zobrazi dialog s varovanim a ikonou cierneho vykricnika v zltom trojuholniku.
    /// </summary>
    /// <param name="text">Text spravy.</param>
    /// <param name="buttons">Tlacidla, ktore sa zobrazia v dialogu.</param>
    /// <returns>vysledok dialogu.</returns>
    [ExcludeFromCodeCoverage]
    public static DialogResult ShowWarning([Localizable(true)] string text, MessageBoxButtons buttons = MessageBoxButtons.OK)
    {
        return ExMessageBox.Show(text, GlobalResources.RWarning, buttons, MessageBoxIcon.Warning);
    }

    /// <summary>
    ///     Zobrazí dialog s informaciou a ikonou bieleho pismena 'i' v modrom kruhu.
    /// </summary>
    /// <param name="text">Text spravy.</param>
    /// <param name="title">Titulok dialogu.</param>
    /// <param name="buttons">Tlacidla, ktore sa zobrazia v dialógu.</param>
    /// <returns>vysledok dialogu.</returns>
    [ExcludeFromCodeCoverage]
    public static DialogResult ShowInfo([Localizable(true)] string text, string title = null, MessageBoxButtons buttons = MessageBoxButtons.OK)
    {
        return ExMessageBox.Show(text, title ?? GlobalResources.RInfo, buttons, MessageBoxIcon.Information);
    }

    /// <summary>
    ///     Konvertuje dlzku zvuku v milisekundach (ms) to textovej podoby "mm:ss".
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
    ///     Konvertuje dlzku zvuku v textovej podobe "m:ss"/"mm:ss"/"h:mm:ss" do trvania ako cislo v milisekundach (ms).
    /// </summary>
    /// <param name="text">textovu podobu dlzky zvuku vo formate "mm:ss".</param>
    /// <returns>Dlzka zvuku v ms.</returns>
    public static int StringToLengthInt(string text)
    {
        if (text is null)
            throw new ArgumentNullException(nameof(text));
        if (text == "--:--")
            return -1;

        string[] timeformats = { @"m\:ss", @"mm\:ss", @"h\:mm\:ss" };
        if(TimeSpan.TryParseExact(text, timeformats, CultureInfo.InvariantCulture, out var duration))
            return (int)duration.TotalMilliseconds;
        else
            throw new ArgumentException("Neplatný formát času");
    }

    /// <summary>
    ///     Vráti čislo ako reťazec doplnené o určitý počet 0 na začiatok. <br></br>
    ///     Ak je pocet cifier menší ako 1, vráti číslo ako reťazec (bez žiadnych 0 pred začiatkom).
    /// </summary>
    /// <param name="num">Cislo.</param>
    /// <param name="pocetCifier">Pocet cifier.</param>
    /// <returns>reťazec s cislom.</returns>
    public static string PadZeros(this int num, int pocetCifier = 3)
    {
        if (pocetCifier <= 0) 
            return num.ToString();
        var sb = new StringBuilder();
        for (var i = 0; i < pocetCifier; i++) 
            sb.Append('0');
        return num.ToString(sb.ToString());
    }

    /// <summary>
    ///     Zistí, či je zadaná hodnota v reťazci celé číslo (<see cref="int"/>).
    /// </summary>
    /// <param name="num">Retazec s moznym cislom.</param>
    /// <returns>ci sa retazec da konverovat na cislo.</returns>
    public static bool IsInt(string num) => int.TryParse(num, out _);

    /// <summary>
    ///     Vrati skonverované číslo z retazca, alebo ak sa nedal retazec skonvertovat vrati nastavenu predvolenu hodnotu.
    /// </summary>
    /// <param name="nums">Retazec s moznym cislom.</param>
    /// <param name="def">Predvolena hodnota.</param>
    /// <returns>skonverovane cislo alebo predvolenu hodnotu.</returns>
    public static int ParseIntOrDefault(string nums, int def = 0) => int.TryParse(nums, out var numi) ? numi : def;

    /// <summary>
    ///     Vráti skonvertované číslo z retazca,
    ///     alebo ak sa nedal retazec skonvertovat vrati nastavenu predvolenu hodnotu (<see langword="null"/>).
    /// </summary>
    /// <param name="nums">Retazec s moznym cislom.</param>
    /// <param name="def">Predvolena hodnota.</param>
    /// <returns>skonverovane cislo alebo predvolenu hodnotu.</returns>
    public static int? ParseIntOrNull(string nums, int? def = null) => int.TryParse(nums, out var numi) ? numi : def;

    /// <summary>
    ///     Vrati retazec, ak je retazec <see langword="null" />, vrati predvoleny retazec.
    /// </summary>
    /// <param name="str">Retazec.</param>
    /// <param name="def">Predvoleny retazec.</param>
    /// <returns></returns>
    public static string ParseStringOrDefault(string str, string def = "") => str ?? def;

    /// <summary>
    ///     Vrati pole bitov ako <see cref="string"/>.
    /// </summary>
    /// <param name="bits">Pole bitov.</param>
    /// <returns>pole bitov ako <see cref="string"/>.</returns>
    public static string BitArrayToString(BitArray bits)
    {
        if (bits is null)
            throw new ArgumentNullException(nameof(bits));

        var sb = new StringBuilder();

        for (var i = 0; i < bits.Count; i++) 
            sb.Append(bits[i] ? '1' : '0');

        return sb.ToString();
    }

    /// <summary>
    ///     Konvertuje pole bitov (ako <see cref="string"/>) ako pole bitov <see cref="BitArray"/>.
    /// </summary>
    /// <param name="bits">Pole bitov.</param>
    /// <returns>pole bitov ako <see cref="BitArray"/>.</returns>
    public static BitArray StringToBitArray(string bits)
    {
        if (bits is null)
            throw new ArgumentNullException(nameof(bits));

        return new BitArray(bits.Select(c => c == '1').ToArray());
    }

    /// <summary>
    ///     Vrati retazec v uvodzovkach. Ak je text <see langword="null"/>, vrati "\"\"".
    /// </summary>
    /// <param name="text">Povodny retazec.</param>
    /// <returns>retazec v uvodzovkach.</returns>
    public static string Quote(this string text) => text == null ? "\"\"" : $"\"{text}\"";

    /// <summary>
    ///     Vrati 1 pre <see langword="true"/>, 0 pre <see langword="false"/>.
    /// </summary>
    /// <param name="hodnota">Hodnota <see langword="true"/> alebo <see langword="false"/>.</param>
    /// <returns>0 alebo 1.</returns>
    public static int ToNumber(this bool hodnota) => hodnota ? 1 : 0;

    /// <summary>
    ///     Vrati <see langword="false"/> pre 0, inak vrati 1.
    /// </summary>
    /// <param name="hodnota">Hodnota ako cislo.</param>
    /// <returns><see langword="true"/> alebo <see langword="false"/>.</returns>
    public static bool ToBool(this int hodnota) => hodnota != 0;

    /// <summary>
    ///     Vrati farbu zadanu v hexadecimalnom tvare (BGR) z objektu <see cref="Color"/>.
    /// </summary>
    /// <param name="c">Farbu <see cref="Color"/>.</param>
    /// <returns>farba v hexadecimalnom tvare.</returns>
    public static string ToHex(this Color c) => "0x" + c.B.ToString("X2") + c.G.ToString("X2") + c.R.ToString("X2");

    /// <summary>
    ///     Vrati objekt Color z farby zadanej hexadecimalnou hodnotou (BGR).
    /// </summary>
    /// <param name="hex">Farba v hexadecimalnom tvare.</param>
    /// <returns>farbu <see cref="Color"/>.</returns>
    public static Color ParseHex(string hex)
    {
        if (hex is null)
            throw new ArgumentNullException(nameof(hex));

        var replaced = hex.Replace("0x", "#");
        var color = ColorTranslator.FromHtml(replaced);
        var newColor = Color.FromArgb(color.B, color.G, color.R);
        return newColor;
    }

    /// <summary>
    ///     Vrati objekt Color z farby zadanej hexadecimalnou hodnotou (BGR) alebo <see langword="null"/>.
    /// </summary>
    /// <param name="hex">Farba v hexadecimalnom tvare.</param>
    /// <returns>farbu <see cref="Color"/> alebo <see langword="null"/>, ak konvertovanie neprebehlo uspesne.</returns>
    public static Color? TryParseHex(string hex)
    {
        if (hex == null) return null;
        try { return ParseHex(hex); } catch { return null; }
    }

    /// <summary>
    ///     Vrati zoznam vsetkych systemovych fontov.
    /// </summary>
    /// <returns>zoznam systemovych fontov.</returns>
    public static List<string> GetSystemFontNames()
    {
        var fontnames = new List<string>();

        using var col = new InstalledFontCollection();
        fontnames.AddRange(col.Families.Select(fa => fa.Name));

        return fontnames;
    }

    /// <summary>
    ///     Zisti, ci je <see cref="Font"/> <paramref name="ft"/> neproporcionalny.
    /// </summary>
    /// <param name="g">Grafika, na ktorej sa bude testovat proporcialnost.</param>
    /// <param name="ft">Pismo na otestovanie.</param>
    /// <returns><see langword="true" /> ak je font neproporcionalny, inak <see langword="false"/>.</returns>
    public static bool IsFontMonospaced(Graphics g, Font ft)
    {
        if (g is null)
            throw new ArgumentNullException(nameof(g));

        if (ft is null)
            throw new ArgumentNullException(nameof(ft));

        char[] charSizes = { 'i', 'a', 'Z', '%', '#', 'a', 'B', 'l', 'm', ',', '.' };
        var charWidth = g.MeasureString("I", ft).Width;

        return charSizes.All(c => Math.Abs(g.MeasureString(c.ToString(), ft).Width - charWidth) <= 0.0001f);
    }

    /// <summary>
    ///     Zisti, ci zoznam obsahuje vsetky prvky ineho zoznamu.
    /// </summary>
    /// <param name="containingList">Vacsi zoznam, ktory kontrolujeme.</param>
    /// <param name="lookupList">Zoznam, ktory treba vyhladat v zozname.</param>
    /// <returns><see langword="true"/> ak obsahuje vsetky prvky, inak <see langword="false"/>.</returns>
    public static bool ContainsAllItems<T>(this IEnumerable<T> containingList, IEnumerable<T> lookupList) => !lookupList.Except(containingList).Any();

    /// <summary>
    ///     Vrati retazec, ktory sa nachadza na pozicii <paramref name="index"/> pola/listu <paramref name="source"/>.<br></br>
    ///     Ak je <paramref name="index"/> mimo rozsahu pola alebo je retazec na indexe
    ///     <see langword="null"/>, vrati predvoleny retazec urceny parametrom <paramref name="def"/>.
    /// </summary>
    /// <param name="source">Pole/list retazcov.</param>
    /// <param name="index">Pozicia prvku.</param>
    /// <param name="def">Predvoleny retazec.</param>
    /// <returns>Retazec, alebo predvoleny retazec <paramref name="def"/>.</returns>
    public static string ElementAtOrDefaultStr(this IEnumerable<string> source, int index, string def = "") => source.ElementAtOrDefault(index) ?? def;

    /// <summary>
    ///     Kopiruje obsah priecinka, ak je aspon 1 z parametov <see cref="string.Empty"/> alebo <see langword="null"/>, nic sa nevykona.
    /// </summary>
    /// <param name="sourcePath">zdrojovy priecinok</param>
    /// <param name="destinationPath">cielovy priecinok</param>
    public static void CopyDirectory(string sourcePath, string destinationPath)
    {
        if (string.IsNullOrEmpty(sourcePath) || string.IsNullOrEmpty(destinationPath)) return;

        //Now Create all of the directories
        foreach (var dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(dirPath.Replace(sourcePath, destinationPath));

        //Copy all the files & Replaces any files with the same name
        foreach (var newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            File.Copy(newPath, newPath.Replace(sourcePath, destinationPath), true);
    }

    /// <summary>
    ///     Vrati nazov priecinka.
    /// </summary>
    /// <returns>nazov priecinka alebo <see langword="null"/> ak je vstup <see langword="null"/> alebo <see cref="string.Empty"/>.</returns>
    public static string GetDirectoryName(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        var dir = new DirectoryInfo(path);
        return dir.Name;
    }

    /// <summary>
    ///     Skontroluje, či zadaná klávesová stkratka je správna.
    /// </summary>
    /// <param name="keys">klávesy</param>
    /// <returns></returns>
    public static bool ValidateShortcut(Keys keys)
    {
        var values = Enum.GetValues(typeof(Shortcut));
        return values.Cast<object>().Any(val => (int)val == (int)keys);
    }

    /// <summary>
    ///     Odstrani diakritiku z retazca text. <br></br>
    ///     Source: https://stackoverflow.com/a/37070320/14438039
    /// </summary>
    /// <param name="text">Text.</param>
    /// <returns>text bez diakritiky.</returns>
    public static string RemoveDiacritics(string text)
    {
        if (text is null)
            throw new ArgumentNullException(nameof(text));

        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark) 
                stringBuilder.Append(c);
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }

    /// <summary>
    ///     Prevedie reťazec <paramref name="text"/>, ktorý je vo formáte dátumu (dd.MM.yyyy) na <see cref="DateTime"/>.
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
    ///     Prevedie reťazec <paramref name="text"/> na <see cref="DateTime"/>, pričom reťazec musí byť vo formáte:
    ///     dd.MM.yyyy / d.MM.yyyy / d.M.yyyy / dd.M.yyyy .
    /// </summary>
    /// <param name="text">Retazec s datumom.</param>
    /// <returns>datum vo forme objektu typu <see cref="DateTime"/>.</returns>
    public static DateTime ParseDateAlts(string text)
    {
        if (string.IsNullOrEmpty(text))
            return DateTime.MinValue;

        string[] types = { "dd.MM.yyyy", "d.MM.yyyy", "d.M.yyyy", "dd.M.yyyy" };
        return DateTime.ParseExact(text, types, CultureInfo.InvariantCulture, DateTimeStyles.None);
    }

    /// <summary>
    ///     Prevedie reťazec <paramref name="text"/>, ktorý je vo formáte času (HH:mm / H:mm) na <see cref="DateTime"/>.
    /// </summary>
    /// <param name="text">Retazec s casom.</param>
    /// <returns>cas vo forme objektu typu <see cref="DateTime"/>.</returns>
    public static DateTime ParseTime(string text)
    {
        return string.IsNullOrEmpty(text) ? 
            DateTime.MinValue : 
            DateTime.ParseExact(text, new[] { "HH:mm", "H:mm" }, CultureInfo.InvariantCulture, DateTimeStyles.None);
    }

    /// <summary>
    ///     Skúsi previesť reťazec <paramref name="text"/>, ktorý je vo formáte času (HH:mm / H:mm) na <see cref="DateTime"/>.
    /// </summary>
    /// <param name="text">Retazec s casom.</param>
    /// <param name="time">Cas vo forme objektu typu <see cref="DateTime"/> ak sa prevod podari, inak obsahuje <see cref="DateTime.MinValue"/>.</param>
    /// <returns><see langword="true" /> ak sa prevod podarí, inak vráti <see langword="false"/>.</returns>
    public static bool TryParseTime(string text, out DateTime time)
    {
        try
        {
            time = ParseTime(text);
            return true;
        }
        catch
        {
            time = DateTime.MinValue;
            return false;
        }
    }

    /// <summary>
    ///     Skúsi previesť reťazec na DateTime, pričom reťazec musí byť vo formáte:
    ///     dd.MM.yyyy / d.MM.yyyy / d.M.yyyy / dd.M.yyyy .
    /// </summary>
    /// <param name="text">Retazec s datumom.</param>
    /// <param name="date">Datum vo forme objektu typu <see cref="DateTime"/> ak sa prevod podari, inak obsahuje <see cref="DateTime.MinValue"/>.</param>
    /// <returns><see langword="true"/> ak sa prevod podarí, inak vráti <see langword="false"/>.</returns>
    public static bool TryParseDateAlts(string text, out DateTime date)
    {
        try
        {
            date = ParseDateAlts(text);
            return true;
        }
        catch
        {
            date = DateTime.MinValue;
            return false;
        }
    }

    /// <summary>
    ///     Zisti, ci zadanany retazec <paramref name="str"/> je vo formate casu.
    /// </summary>
    /// <param name="str">Retazec na testovanie.</param>
    /// <returns></returns>
    public static bool IsTime(string str) => TryParseTime(str, out _);

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
    ///     Porovna retazce, pricom ignoruje velkost pismen (VELKE/male).
    /// </summary>
    /// <param name="str1">Prvy retazec na porovnavanie.</param>
    /// <param name="str2">Druhy retazec na porovnavanie.</param>
    /// <returns>ci sa retazce zhoduju.</returns>
    public static bool EqualsIgnoreCase(this string str1, string str2) => str1 != null && str1.Equals(str2, StringComparison.CurrentCultureIgnoreCase);

    /// <summary>
    ///     Zisti, ci je riadok prazdny alebo obsahuje komentar alebo zacina mriezkou (#) (pouzitie v: <see cref="CsvRow"/>).
    /// </summary>
    /// <param name="ch">Typ zaciatku riadku.</param>
    /// <returns>Ci je riadok prazdny alebo obsahuje komentar alebo zacina mriezkou (#).</returns>
    [ExcludeFromCodeCoverage]
    public static bool LineIsEmpty(ReadStartChar ch) => ch is ReadStartChar.Semicolon or ReadStartChar.Empty or ReadStartChar.Slash;

    /// <summary>
    ///     Zisti, ci je riadok posledny (pouzitie v: <see cref="CsvRow"/>)
    /// </summary>
    /// <param name="ch">Typ zaciatku riadku.</param>
    /// <returns>Ci riadok obsahuje koniec suboru (EOF).</returns>
    [ExcludeFromCodeCoverage]
    public static bool LineIsEOF(ReadStartChar ch) => ch == ReadStartChar.Eof;

    /// <summary>
    ///     Returns a new string in which all occurrences of a specified string in the current instance are replaced with
    ///     another
    ///     specified string according the type of search to use for the specified string.<br></br>
    ///     Source: https://stackoverflow.com/questions/6275980/string-replace-ignoring-case
    /// </summary>
    /// <param name="str">The string performing the replace method.</param>
    /// <param name="oldValue">The string to be replaced.</param>
    /// <param name="newValue">
    ///     The string replace all occurrences of <paramref name="oldValue" />.
    ///     If value is equal to <c>null</c>, than all occurrences of <paramref name="oldValue" /> will be removed from the
    ///     <paramref name="str" />.
    /// </param>
    /// <param name="comparisonType">One of the enumeration values that specifies the rules for the search.</param>
    /// <returns>
    ///     A string that is equivalent to the current string except that all instances of <paramref name="oldValue" /> are
    ///     replaced with <paramref name="newValue" />.
    ///     If <paramref name="oldValue" /> is not found in the current instance, the method returns the current instance
    ///     unchanged.
    /// </returns>
    public static string Replace(this string str, string oldValue, string newValue, StringComparison comparisonType)
    {
        // Check inputs.
        if (str == null)
            throw new ArgumentNullException(nameof(str));
        if (str.Length == 0)
            return str;
        if (oldValue == null)
            throw new ArgumentNullException(nameof(oldValue));
        if (oldValue.Length == 0)
            throw new ArgumentException("String cannot be of zero length.");

        // Prepare string builder for storing the processed string.
        // Note: StringBuilder has a better performance than String by 30-40%.
        var resultStringBuilder = new StringBuilder(str.Length);

        // Analyze the replacement: replace or remove.
        var isReplacementNullOrEmpty = string.IsNullOrEmpty(newValue);

        // Replace all values.
        const int valueNotFound = -1;
        int foundAt;
        var startSearchFromIndex = 0;
        while ((foundAt = str.IndexOf(oldValue, startSearchFromIndex, comparisonType)) != valueNotFound)
        {
            // Append all characters until the found replacement.
            var charsUntilReplacment = foundAt - startSearchFromIndex;
            var isNothingToAppend = charsUntilReplacment == 0;
            if (!isNothingToAppend)
                resultStringBuilder.Append(str, startSearchFromIndex, charsUntilReplacment);

            // Process the replacement.
            if (!isReplacementNullOrEmpty)
                resultStringBuilder.Append(newValue);

            // Prepare start index for the next search.
            // This needed to prevent infinite loop, otherwise method always start search 
            // from the start of the string. For example: if an oldValue == "EXAMPLE", newValue == "example"
            // and comparisonType == "any ignore case" will conquer to replacing:
            // "EXAMPLE" to "example" to "example" to "example" … infinite loop.
            startSearchFromIndex = foundAt + oldValue.Length;
            if (startSearchFromIndex == str.Length)
                // It is end of the input string: no more space for the next search.
                // The input string ends with a value that has already been replaced. 
                // Therefore, the string builder with the result is complete and no further action is required.
                return resultStringBuilder.ToString();
        }

        // Append the last part to the result.
        var charsUntilStringEnd = str.Length - startSearchFromIndex;
        resultStringBuilder.Append(str, startSearchFromIndex, charsUntilStringEnd);

        return resultStringBuilder.ToString();
    }

    /// <summary>
    ///     Vytvori relativnu cestu k suboru <paramref name="filePath"/> podla absolutnej cesty <paramref name="folderPath"/>.
    /// </summary>
    /// <param name="filePath">Absolutna cesta k suboru.</param>
    /// <param name="folderPath">Cesta k priecinku, od ktoreho sa bude brat cesta k suboru ako relativna.</param>
    /// <returns>relativnu cestu k suboru.</returns>
    public static string GetRelativePath(string filePath, string folderPath)
    {
        var pathUri = new Uri(filePath);
        // Folders must end in a slash
        if (!folderPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
        {
            folderPath += Path.DirectorySeparatorChar;
        }
        var folderUri = new Uri(folderPath);
        return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
    }

    /// <summary>
    ///     Odstrani subor a premiestni ho do kosa (recycle bin).
    /// </summary>
    /// <param name="path">cesta k suboru</param>
    /// /// <param name="allDialogs"></param>
    public static void DeleteFileToRecycleBin(string path, bool allDialogs = false) 
        => FileSystem.DeleteFile(path, allDialogs ? UIOption.AllDialogs : UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);

    /// <summary>
    ///     Odstrani priecinok a premiestni ho do kosa (recycle bin).
    /// </summary>
    /// <param name="path">cesta k suboru</param>
    /// <param name="allDialogs"></param>
    public static void DeleteDirectoryToRecycleBin(string path, bool allDialogs = false) 
        => FileSystem.DeleteDirectory(path, allDialogs ? UIOption.AllDialogs : UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);

    /// <summary>
    ///     Pokusi sa obnovit subor/priecinok, ktory bol premiestneneny do kosa (recycle bin).
    /// </summary>
    /// <param name="fullPath">povodna cesta k suboru</param>
    /// <returns>ci sa podarilo obnovit subor/priecinok</returns>
    public static bool TryRecoverFileOrDirFromBin(string fullPath)
    {
        var item = RecycleBin.GetItemFromOriginalPath(fullPath);
        if (item is null)
            return false;

        RecycleBin.Restore(item, true);
        return true;
    }

    /// <summary>
    ///     Zisti, ci je v DGV vybrany aspon 1 riadok.
    /// </summary>
    /// <param name="dgv"></param>
    /// <returns></returns>
    public static bool IsSelectionEmpty(this DataGridView dgv) => dgv.SelectedRows.Count == 0;

    /// <summary>
    ///     Zisti, ci zadany nazov suboru/priecinka je platny.
    /// </summary>
    /// <param name="fullPath"></param>
    /// <param name="fileName"></param>
    /// <param name="checkIfExists"></param>
    /// <returns></returns>
    public static bool IsFileNameCorrect(string fullPath, string fileName, bool checkIfExists = true)
    {
        return !string.IsNullOrWhiteSpace(fileName) &&
               fileName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0 &&
               (!checkIfExists || !File.Exists(CombinePath(fullPath, fileName)));
    }

    public static bool StartsWithAny(this string text, params char[] values)
    {
        if (string.IsNullOrEmpty(text))
            return false;
        foreach (var c in values)
        {
            if (text[0] == c)
                return true;
        }

        return false;
    }
}

/// <summary>
///     Typ znaku na zaciatku riadku.
/// </summary>
public enum ReadStartChar
{
    /// <summary>
    ///     Neprazdny riadok.
    /// </summary>
    NonEmpty,

    /// <summary>
    ///     Prazdny riadok.
    /// </summary>
    Empty,

    /// <summary>
    ///     Bodkociarka.
    /// </summary>
    Semicolon,

    /// <summary>
    ///     Mriezka (#).
    /// </summary>
    Slash,

    /// <summary>
    ///     Znak konca suboru.
    /// </summary>
    Eof
}