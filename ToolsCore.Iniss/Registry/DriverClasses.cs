using System.Globalization;
using System.Text.RegularExpressions;
using ToolsCore.Iniss.Properties;

namespace ToolsCore.Iniss.Registry;

/// <summary>
/// Trieda linky k tabuliam (<c>TableClass</c>) - rodina protokolov a vyrobcovia tabul, ktori na linku patria.
/// </summary>
/// <param name="Class">hodnota TableClass</param>
/// <param name="Manufacturers">ciselne kody vyrobcov (<c>MANUFACTURER_KEY</c> katalogu) tabul tejto rodiny</param>
public sealed record DriverClass(int Class, IReadOnlyList<int> Manufacturers)
{
    /// <summary>Nazov rodiny v jazyku UI.</summary>
    public string Name => Resources.ResourceManager.GetString($"Reg_Class_{Class}", CultureInfo.CurrentUICulture) ?? Class.ToString(CultureInfo.CurrentCulture);

    /// <summary>Text do zoznamu: <c>4 – ELEN</c>.</summary>
    public override string ToString() => $"{Class} – {Name}";
}

/// <summary>
/// Triedy liniek, cisla komunikacnych liniek a predvolby jasu tabul - podla docs/iniss/protokoly a INISS 3.39.
/// </summary>
public static partial class DriverClasses
{
    // kody vyrobcov ako v INISSe (TableManufacturer v GVDEditore)
    private const int AdonBuse = 0, Lcd = 1, Ers = 2, Fers = 3, Elen = 4, Lcd1 = 5, ElenOld = 6, Elen10 = 7, Elen16 = 8, Elen16Kam = 9, Apel = 10, ApelAn = 11, Erp = 12,
        Elekon = 13;

    /// <summary>Triedy, ktore INISS pozna, v poradi dokumentacie.</summary>
    public static IReadOnlyList<DriverClass> All { get; } =
    [
        new(0, [AdonBuse]),
        new(1, [Lcd]),
        new(2, [Ers]),
        new(3, [Fers]),
        new(4, [Elen, ElenOld, Elen10, Elen16, Elen16Kam, Elekon]),
        new(5, [Lcd1, Erp]),
        new(10, [Apel, ApelAn]),
        new(128, []),
        new(129, []),
        new(130, [Lcd, Fers, Lcd1, Erp])
    ];

    /// <summary>Trieda podla cisla alebo null (nezname cislo sa sprava ako trieda 5 bez vlastneho nazvu).</summary>
    public static DriverClass? Find(int tableClass) => All.FirstOrDefault(c => c.Class == tableClass);

    /// <summary>
    /// Zhoda protokolu linky s vyrobcom tabule ako v INISSe 3.39: 3 = vlastna rodina, 1 = vzdialeny INISS (128) alebo
    /// obal ITP (130), ktore tabule len preposielaju, 0 = tabula na linku nepatri.
    /// </summary>
    public static int Score(int tableClass, int manufacturer)
    {
        if (tableClass == manufacturer) return 3;
        return tableClass switch
        {
            4 when manufacturer is ElenOld or Elen10 or Elen16 or Elen16Kam or Elekon => 3,
            10 when manufacturer == ApelAn => 3,
            5 when manufacturer == Erp => 3,
            130 when manufacturer is Lcd or Lcd1 or Fers or Erp => 1,
            128 when manufacturer is >= Lcd and <= Elekon => 1,
            _ => 0
        };
    }

    /// <summary>Tabula s cislom linky v TPhysic na linku tejto triedy patri (INISS jej bude posielat).</summary>
    public static bool Accepts(int tableClass, int manufacturer) => Score(tableClass, manufacturer) > 0;

    /// <summary>Tabula bez cisla linky (COMUNICATION_PORT=0) sa na linku tejto triedy da priradit automaticky.</summary>
    public static bool AcceptsAutomatically(int tableClass, int manufacturer) => Score(tableClass, manufacturer) > 1;

    /// <summary>
    /// Cislo komunikacnej linky z hodnoty TablePort: <c>N=…</c> -> N, seriovy port <c>COMn</c> -> n; inak null
    /// (rura, mailslot alebo sietovy kanal bez cisla - INISS ho berie ako linku 0).
    /// </summary>
    public static int? LineNumber(string? port)
    {
        if (string.IsNullOrWhiteSpace(port)) return null;
        if (DriverDefaults.LineNumber(port) is { } n) return n;
        var m = ComPort().Match(port.Trim());
        return m.Success ? int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture) : null;
    }

    /// <summary>
    /// Predvoleny vynuteny jas tabule (ForceLight&lt;N&gt;) podla vyrobcu: ELEN a jeho varianty 99, APEL 0, ELEKON 5,
    /// ostatne -1 (nevynucovat).
    /// </summary>
    public static int ForceLightDefault(int manufacturer) => manufacturer switch
    {
        Elen or ElenOld or Elen10 or Elen16 or Elen16Kam => 99,
        Apel or ApelAn => 0,
        Elekon => 5,
        _ => -1
    };

    [GeneratedRegex(@"^COM(\d{1,3})$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex ComPort();
}
