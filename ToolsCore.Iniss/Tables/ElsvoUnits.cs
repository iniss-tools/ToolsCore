using System.Globalization;
using ToolsCore.Iniss.Grafikon;
using ToolsCore.Iniss.Tools;

namespace ToolsCore.Iniss.Tables;

/// <summary>
/// Spinacia jednotka zosilnovaca ELSVO, ako ju INISS pozna z okruhov <c>Audio.txt</c>.
/// </summary>
/// <param name="Line">cislo linky (stlpec 8 <c>E&lt;n&gt;</c>)</param>
/// <param name="Address">adresa jednotky na linke (dolny bajt stlpca 9)</param>
/// <param name="Circuits">nazvy okruhov, ktore jednotka spina, v poradi suboru</param>
public sealed record ElsvoUnit(int Line, int Address, IReadOnlyList<string> Circuits)
{
    /// <summary>Nazov jednotky pre obsluhu: okruhy oddelene ciarkou.</summary>
    public string Name => string.Join(", ", Circuits);
}

/// <summary>
/// Jednotky ELSVO zo zvukovych okruhov (<c>Audio.txt</c>): okruh so stlpcom 8 <c>E&lt;n&gt;</c> alebo
/// <c>EE&lt;n&gt;</c> (n = 1-99, cislo linky) spina zosilnovac cez jednotku s adresou zo stlpca 9 (prazdne = 0, posiela
/// sa dolny bajt). Cislo v stlpci 8 je vystup ustredne TORNZ, <c>Z…</c> nespina nic. Viz docs
/// <c>formaty-suborov/audio.mdx</c> a <c>protokoly/elsvo.mdx</c>.
/// </summary>
public static class ElsvoUnits
{
    /// <summary>Stlpce Audio.txt (od 0): nazov, protokolovy nazov, spinanie zosilnovaca, parameter ustredne.</summary>
    private const int ColName = 1;

    private const int ColShortName = 2;
    private const int ColAmplifier = 7;
    private const int ColExchange = 8;

    /// <summary>Najmensi pocet poli riadku - kratsi INISS preskoci.</summary>
    private const int MinFields = 3;

    /// <summary>Cislo linky ELSVO zo stlpca 8; null = okruh nespina cez ELSVO (TORNZ, <c>Z…</c>, prazdne, mimo rozsahu).</summary>
    public static int? Line(string? amplifierPort)
    {
        var text = (amplifierPort ?? "").Trim();
        var prefix = text.StartsWith("EE", StringComparison.Ordinal) ? 2 : text.StartsWith('E') ? 1 : 0;
        if (prefix == 0)
            return null;
        return int.TryParse(text.AsSpan(prefix), NumberStyles.None, CultureInfo.InvariantCulture, out var line) && line is >= 1 and <= 99 ? line : null;
    }

    /// <summary>Adresa jednotky zo stlpca 9: cele cislo ako ho cita INISS (cislice na zaciatku, inak 0), dolny bajt.</summary>
    public static int Address(string? exchangeParameter)
    {
        var text = (exchangeParameter ?? "").Trim();
        var end = 0;
        if (end < text.Length && text[end] is '+' or '-')
            end++;
        while (end < text.Length && char.IsAsciiDigit(text[end]))
            end++;
        return long.TryParse(text.AsSpan(0, end), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var number) ? (int)(number & 0xFF) : 0;
    }

    /// <summary>
    /// Jednotky z okruhov (nazov, stlpec 8, stlpec 9) - okruhy s rovnakou linkou a adresou spina ta ista jednotka.
    /// </summary>
    public static IReadOnlyList<ElsvoUnit> FromCircuits(IEnumerable<(string Name, string? AmplifierPort, string? ExchangeParameter)> circuits)
    {
        ArgumentNullException.ThrowIfNull(circuits);
        var units = new List<(int Line, int Address, List<string> Circuits)>();
        foreach (var (name, amplifier, exchange) in circuits)
        {
            if (Line(amplifier) is not { } line)
                continue;
            var address = Address(exchange);
            var unit = units.FindIndex(u => u.Line == line && u.Address == address);
            if (unit < 0)
                units.Add((line, address, [name.Trim()]));
            else if (!units[unit].Circuits.Contains(name.Trim(), StringComparer.Ordinal))
                units[unit].Circuits.Add(name.Trim());
        }

        return units.Select(u => new ElsvoUnit(u.Line, u.Address, u.Circuits)).ToList();
    }

    /// <summary>
    /// Jednotky z <c>Audio.txt</c> v priecinku DATA; bez suboru ziadne. Riadky okruhov sa citaju ako v INISSe: komentar
    /// <c>;</c> a prazdny riadok sa preskocia, riadok s menej ako 3 poliami tiez, prvy riadok zacinajuci <c>/</c> zoznam
    /// ukonci. Okruhy neaktivnych stanic z DirList sa nevynechavaju (jednotka navyse nevadi).
    /// </summary>
    public static IReadOnlyList<ElsvoUnit> Read(string dataDir)
    {
        ArgumentNullException.ThrowIfNull(dataDir);
        var file = AudioFile(dataDir);
        if (file is null)
            return [];

        var circuits = new List<(string, string?, string?)>();
        using var reader = new CsvFileReader(file);
        var row = new CsvRow();
        while (true)
        {
            var status = reader.ReadRow(row);
            if (status == ReadStartChar.Eof)
                break;
            var text = row.LineText?.TrimStart() ?? "";
            if (status == ReadStartChar.Slash || text.StartsWith('/'))
                break;
            if (status != ReadStartChar.NonEmpty || text.StartsWith(';') || row.Count < MinFields)
                continue;

            var name = row[ColName].Length > 0 ? row[ColName] : row[ColShortName];
            circuits.Add((name, Field(row, ColAmplifier), Field(row, ColExchange)));
        }

        return FromCircuits(circuits);
    }

    private static string? Field(CsvRow row, int index) => index < row.Count ? row[index] : null;

    /// <summary>Audio.txt v priecinku DATA bez ohladu na velkost pismen (INISS bezi vo Windows).</summary>
    private static string? AudioFile(string dataDir)
    {
        if (!Directory.Exists(dataDir))
            return null;
        var exact = Path.Combine(dataDir, GvdFileConsts.FileAudio);
        if (File.Exists(exact))
            return exact;
        return Directory.EnumerateFiles(dataDir).FirstOrDefault(f => string.Equals(Path.GetFileName(f), GvdFileConsts.FileAudio, StringComparison.OrdinalIgnoreCase));
    }
}
