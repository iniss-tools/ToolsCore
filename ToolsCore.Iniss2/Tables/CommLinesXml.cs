using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using ToolsCore.Iniss.Tools;

namespace ToolsCore.Iniss2.Tables;

/// <summary>
/// Sposob spojenia linky.
/// </summary>
public enum CommLineType
{
    /// <summary>Linka bez spojenia alebo s neznamym klucom.</summary>
    None,

    /// <summary>Seriovy port (kluc SERIAL).</summary>
    Serial,

    /// <summary>Server pocuva datagramy (kluc UDP_SERVER).</summary>
    UdpServer,

    /// <summary>Server posiela datagramy na adresu (kluc UDP_CLIENT).</summary>
    UdpClient,

    /// <summary>Server pocuva spojenia TCP (kluc TCP_SERVER).</summary>
    TcpServer,

    /// <summary>Server sa pripaja na adresu cez TCP (kluc TCP_CLIENT).</summary>
    TcpClient
}

/// <summary>
/// Komunikacna linka servera.
/// </summary>
/// <param name="Key">kluc - podla neho sa na linku odkazuju tabule</param>
/// <param name="Type">sposob spojenia</param>
/// <param name="Protocol">protokol, napriklad ITP</param>
/// <param name="Address">atribut <c>line</c> - adresa alebo port</param>
public sealed record CommLine(string Key, CommLineType Type, string Protocol, string Address)
{
    /// <summary>Server linku vynechava (atribut <c>omit</c>).</summary>
    public bool Omitted { get; init; }

    /// <summary>Povodna adresa, ak je linka presmerovana na simulator.</summary>
    public string? OriginalAddress { get; init; }

    /// <summary>Linka sa da presmerovat: server sa na nej pripaja na adresu, alebo je seriova a presmerovanie z nej urobi TCP_CLIENT.</summary>
    public bool CanRedirect => Type is CommLineType.UdpClient or CommLineType.TcpClient or CommLineType.Serial;
}

/// <summary>
/// Komunikacne linky servera - subor CommLines.xml v priecinku dat.
/// </summary>
/// <remarks>
/// Presmerovanie na simulator meni len hodnotu atributu <c>line</c>, pri seriovej linke aj <c>typ</c> na <c>TCP_CLIENT</c>; server
/// ma na oboch rovnake ramce a parametre portu pri TCP ignoruje. Ostatok suboru ostava po znakoch rovnaky. Povodne hodnoty si
/// subor pamata v komentari pred linkou (server ho nevidi), zrusenie presmerovania vrati presne to, co v nom bolo.
/// </remarks>
public static partial class CommLinesXml
{
    /// <summary>
    /// Nazov suboru v priecinku dat.
    /// </summary>
    public const string FileName = "CommLines.xml";

    // text komentara je data suboru (bez diakritiky, citatelny aj bez simulatora)
    private const string Marker = "TableSimulator: povodne line=";

    private const string SerialKey = "SERIAL";
    private const string TcpClientKey = "TCP_CLIENT";

    private static readonly Dictionary<string, CommLineType> TypeKeys = new(StringComparer.Ordinal)
    {
        [SerialKey] = CommLineType.Serial,
        ["UDP_SERVER"] = CommLineType.UdpServer,
        ["UDP_CLIENT"] = CommLineType.UdpClient,
        ["TCP_SERVER"] = CommLineType.TcpServer,
        [TcpClientKey] = CommLineType.TcpClient
    };

    /// <summary>
    /// Nacita linky v poradi suboru. Chybajuci subor je prazdny zoznam.
    /// </summary>
    /// <param name="dataDir">priecinok dat servera</param>
    public static List<CommLine> Read(string dataDir)
    {
        var file = Path.Combine(dataDir, FileName);
        if (!File.Exists(file))
            return [];

        var lines = new List<CommLine>();
        Original? original = null;

        foreach (var node in Iniss2Xml.Load(file).Root!.Nodes())
        {
            if (node is XComment comment)
                original = OriginalOf(comment.Value);

            if (node is not XElement { Name.LocalName: "line" } element)
                continue;

            lines.Add(new CommLine(Iniss2Xml.Text(element, "k"), TypeKeys.GetValueOrDefault(Iniss2Xml.Text(element, "typ")), Iniss2Xml.Text(element, "protocol"),
                Iniss2Xml.Text(element, "line"))
            {
                Omitted = string.Equals(Iniss2Xml.Text(element, "omit"), "true", StringComparison.OrdinalIgnoreCase),
                OriginalAddress = original?.Address
            });
            original = null;
        }

        return lines;
    }

    /// <summary>
    /// Zapise presmerovanie do suboru - pozri <see cref="Redirect" />.
    /// </summary>
    /// <param name="dataDir">priecinok dat servera</param>
    /// <param name="addresses">kluc linky → nova adresa</param>
    public static void Write(string dataDir, IReadOnlyDictionary<string, string> addresses)
    {
        var file = Path.Combine(dataDir, FileName);
        var updated = Redirect(File.ReadAllText(file, Encodings.Win1250), addresses);

        // novy obsah vznikne vedla a nahradi subor naraz, server tak nikdy nenacita rozpisany subor
        var temporary = file + ".tmp";
        File.WriteAllText(temporary, updated, Encodings.Win1250);
        File.Move(temporary, file, true);
    }

    /// <summary>
    /// Novy obsah suboru, v ktorom su na simulator presmerovane prave linky <paramref name="addresses" />; skor presmerovane
    /// linky, ktore v zozname nie su, sa vratia do povodneho stavu. Prazdny zoznam = zrusenie presmerovania.
    /// </summary>
    /// <param name="text">sucasny obsah suboru</param>
    /// <param name="addresses">kluc linky → nova adresa</param>
    public static string Redirect(string text, IReadOnlyDictionary<string, string> addresses)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(addresses);

        var newline = text.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        var result = new StringBuilder(text.Length + 128);
        var position = 0;
        Match? marker = null;

        foreach (Match token in Token().Matches(text))
        {
            if (token.Value.StartsWith("<!--", StringComparison.Ordinal))
            {
                if (OriginalOf(token.Value[4..^3]) is not null)
                {
                    result.Append(text, position, token.Index - position);
                    position = token.Index + token.Length;
                    marker = token;
                }

                continue;
            }

            var element = token.Value;
            var key = Attribute(element, "k");
            var original = marker is null ? null : OriginalOf(marker.Value[4..^3]);
            var gap = marker is null ? "" : text[position..token.Index];

            if (marker is null)
                result.Append(text, position, token.Index - position);

            if (key is not null && addresses.TryGetValue(key, out var address))
            {
                if (marker is not null)
                    result.Append(marker.Value).Append(gap);
                else
                    result.Append(Comment(element)).Append(newline).Append(Indent(text, token.Index));

                result.Append(WithAddress(IsSerial(element) ? WithType(element, TcpClientKey) : element, address));
            }
            else
            {
                result.Append(original is null ? element : Restored(element, original));
            }

            position = token.Index + token.Length;
            marker = null;
        }

        return result.Append(text, position, text.Length - position).ToString();
    }

    private static bool IsSerial(string element) => Attribute(element, "typ") == SerialKey;

    // pri seriovej linke si komentar pamata aj typ, ostatne ho nemenia
    private static string Comment(string element)
    {
        var type = IsSerial(element) ? $" typ=\"{SerialKey}\"" : "";
        return $"<!-- {Marker}\"{Attribute(element, "line") ?? ""}\"{type} -->";
    }

    private static Original? OriginalOf(string comment)
    {
        var match = OriginalText().Match(comment.Trim());
        return match.Success ? new Original(match.Groups[1].Value, match.Groups[2].Success ? match.Groups[2].Value : null) : null;
    }

    private static string Restored(string element, Original original)
    {
        var restored = WithAddress(element, original.Address);
        return original.Type is null ? restored : WithType(restored, original.Type);
    }

    // medzery pred prvkom na jeho riadku
    private static string Indent(string text, int index)
    {
        var start = index;
        while (start > 0 && text[start - 1] is ' ' or '\t')
            start--;

        return text[start..index];
    }

    private static string? Attribute(string element, string name)
    {
        var match = Regex.Match(element, $@"\s{name}\s*=\s*""([^""]*)""", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
        return match.Success ? match.Groups[1].Value : null;
    }

    private static string WithAddress(string element, string address) =>
        LineAttribute().Replace(element, match => match.Groups[1].Value + address + match.Groups[2].Value, 1);

    private static string WithType(string element, string type) =>
        TypeAttribute().Replace(element, match => match.Groups[1].Value + type + match.Groups[2].Value, 1);

    /// <summary>Povodne hodnoty z komentara.</summary>
    /// <param name="Address">hodnota <c>line</c></param>
    /// <param name="Type">hodnota <c>typ</c>, ak ju presmerovanie zmenilo</param>
    private sealed record Original(string Address, string? Type);

    [GeneratedRegex(@"<!--.*?-->|<line\b(?:[^>""]|""[^""]*"")*>", RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex Token();

    [GeneratedRegex(@"(\sline\s*=\s*"")[^""]*("")", RegexOptions.CultureInvariant)]
    private static partial Regex LineAttribute();

    [GeneratedRegex(@"(\styp\s*=\s*"")[^""]*("")", RegexOptions.CultureInvariant)]
    private static partial Regex TypeAttribute();

    [GeneratedRegex(@"^TableSimulator: povodne line=""([^""]*)""(?: typ=""([^""]*)"")?$", RegexOptions.CultureInvariant)]
    private static partial Regex OriginalText();
}
