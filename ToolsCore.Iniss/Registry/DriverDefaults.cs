using System.Text.RegularExpressions;

namespace ToolsCore.Iniss.Registry;

/// <summary>
/// Predvolene hodnoty sekcie <c>Driver*</c> podla triedy linky (<c>TableClass</c>) a druhu portu - presne ako ich
/// INISS 3.39 nastavi pred citanim ostatnych hodnot sekcie.
/// </summary>
public static partial class DriverDefaults
{
    /// <summary>Predvolena trieda linky.</summary>
    public const int DefaultClass = 5;

    /// <summary>Predvoleny port.</summary>
    public const string DefaultPort = "COM2";

    /// <summary>
    /// Predvolene hodnoty zavisle od triedy a portu (nazov hodnoty -> int alebo string). Hodnoty, ktore tu nie su,
    /// maju pevnu predvolbu z katalogu.
    /// </summary>
    public static IReadOnlyDictionary<string, object> For(int tableClass, string? port)
    {
        var d = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            ["TablePortParam"] = "9600,n,8,1",
            ["SetRTS"] = 1,
            ["TableTimeoutInterval [ms]"] = 15,
            ["TableTimeoutConst [ms]"] = 200,
            ["TableMinWriteDelay [ms]"] = 50,
            ["SyncTimeInterval [s]"] = 0,
            ["RTCAddrs"] = "",
            ["RTCBroadcastAddr"] = 0,
            ["CheckModem"] = "",
            ["CTSFlow"] = 0,
            ["ComputerAddr"] = 0
        };
        switch (tableClass)
        {
            case 2:
                d["TablePortParam"] = "9600,e,7,1";
                break;
            case 4:
                d["SyncTimeInterval [s]"] = 3600;
                break;
            case 10:
                d["TablePortParam"] = "2400,n,8,1";
                d["ComputerAddr"] = 128;
                d["SyncTimeInterval [s]"] = 86400;
                break;
            case 128:
                d["TablePortParam"] = "57600,n,8,1";
                d["SetRTS"] = 0;
                d["TableTimeoutInterval [ms]"] = 30;
                d["TableTimeoutConst [ms]"] = 300;
                d["TableMinWriteDelay [ms]"] = 30;
                d["SyncTimeInterval [s]"] = 3600;
                d["CheckModem"] = "DCD,DSR";
                d["CTSFlow"] = 1;
                break;
            case 129:
                d["TablePortParam"] = "19200,n,8,2";
                d["SetRTS"] = 0;
                d["TableTimeoutConst [ms]"] = 100;
                break;
            case 130:
                d["TablePortParam"] = "";
                d["SetRTS"] = 0;
                d["TableTimeoutConst [ms]"] = 500;
                break;
            default:
                d["SyncTimeInterval [s]"] = 46;
                d["RTCBroadcastAddr"] = 250;
                break;
        }

        // parametre seriovej linky pre ine kanaly nedavaju zmysel - INISS ich vynuluje
        if (!IsSerialPort(port ?? DefaultPort))
        {
            d["TablePortParam"] = "";
            d["SetRTS"] = 0;
            d["TableTimeoutInterval [ms]"] = 0;
            d["TableMinWriteDelay [ms]"] = 0;
            d["CheckModem"] = "";
            d["CTSFlow"] = 0;
        }

        return d;
    }

    /// <summary>Port nie je pomenovana rura, mailslot ani sietovy kanal.</summary>
    public static bool IsSerialPort(string port)
    {
        var p = port.Trim();
        return !p.StartsWith(@"\\", StringComparison.Ordinal) && !p.Contains("://", StringComparison.Ordinal);
    }

    /// <summary>
    /// Sietovy kanal (TCP/UDP) bez cisla komunikacnej linky pred <c>=</c> - INISS ho ohlasi ako linku 0.
    /// </summary>
    public static bool IsNetworkPortWithoutLine(string port)
    {
        var p = port.Trim();
        return NetworkPrefix().IsMatch(p);
    }

    /// <summary>Cislo komunikacnej linky z tvaru <c>N=TCP://…</c>; null, ak ho port nema.</summary>
    public static int? LineNumber(string port)
    {
        var m = LinePrefix().Match(port.Trim());
        return m.Success ? int.Parse(m.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture) : null;
    }

    [GeneratedRegex(@"^(?:TCP|UDP)://", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex NetworkPrefix();

    [GeneratedRegex(@"^(\d{1,3})\s*=", RegexOptions.CultureInvariant)]
    private static partial Regex LinePrefix();
}
