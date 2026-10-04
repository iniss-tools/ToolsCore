using System.Globalization;

namespace ToolsCore.Iniss.Registry;

/// <summary>Linka, ktoru subor .INI presmerovava na simulator tabul, a co v nom bolo predtym.</summary>
/// <param name="Section">sekcia (Driver, Driver0…)</param>
/// <param name="OriginalPort">povodny TablePort v .INI; null = v .INI nebol (sekcia bola len v registri alebo bez neho)</param>
/// <param name="RegistryOnly">sekcia bola len v registri - do .INI ju pridalo presmerovanie celu</param>
public sealed record RedirectedLine(string Section, string? OriginalPort, bool RegistryOnly);

/// <summary>
/// Presmerovanie liniek INISSu na simulator tabul cez subor .INI vedla programu: sekcia Driver* v .INI uplne nahradi
/// register, preto sa do nej zapise cela ucinna sekcia s novym TablePort (<c>N=TCP://host:port</c>) a v Environment
/// sa zapne OutToTableDriver. Povodny stav si subor pamata v komentaroch (INISS ich nevidi), takze zrusenie vrati
/// presne to, co v nom bolo; register sa nemeni.
/// </summary>
public static class SimulatorRedirect
{
    /// <summary>Zaciatok komentara, ktorym presmerovanie oznacuje svoje zmeny.</summary>
    public const string Marker = "GVDEditor-simulator:";

    private const string Environment = "Environment";
    private const string OutToTableDriver = "OutToTableDriver";
    private const string TablePortName = "TablePort";

    // texty komentarov su data suboru (bez diakritiky, citatelne aj bez GVDEditora)
    private const string RegistryOnlyText = "sekcia povodne len v registri";
    private const string NoPortText = "TablePort povodne nebol v .INI";
    private const string OriginalPortText = "povodne TablePort=";
    private const string NoOutText = "OutToTableDriver povodne nebol v .INI";
    private const string OriginalOutText = "povodne OutToTableDriver=";

    /// <summary>TablePort linky na simulatore: <c>N=TCP://host:port</c> (cislo linky ostava, podla neho INISS priraduje tabule).</summary>
    public static string TablePort(int line, string host, int port) =>
        string.Create(CultureInfo.InvariantCulture, $"{line}=TCP://{host}:{port}");

    /// <summary>Linky, ktore subor presmerovava (podla komentarov presmerovania).</summary>
    public static IReadOnlyList<RedirectedLine> Find(InissIniFile? ini)
    {
        if (ini is null) return [];
        var result = new List<RedirectedLine>();
        foreach (var section in ini.SectionNames)
        {
            var marker = ini.Comments(section).FirstOrDefault(IsMarker);
            if (marker is null || string.Equals(section, Environment, StringComparison.OrdinalIgnoreCase)) continue;
            var text = MarkerText(marker);
            if (text == RegistryOnlyText)
                result.Add(new RedirectedLine(section, null, true));
            else if (text.StartsWith(OriginalPortText, StringComparison.Ordinal))
                result.Add(new RedirectedLine(section, text[OriginalPortText.Length..], false));
            else
                result.Add(new RedirectedLine(section, null, false));
        }

        return result;
    }

    /// <summary>
    /// Novy obsah .INI, v ktorom su na simulator presmerovane prave linky <paramref name="ports" /> (sekcia → TablePort);
    /// skor presmerovane linky, ktore v zozname nie su, sa vratia do povodneho stavu. Prazdny zoznam = zrusenie
    /// presmerovania vratane OutToTableDriver.
    /// </summary>
    /// <param name="config">ucinna konfiguracia (hodnoty sekcii, ktore su len v registri)</param>
    /// <param name="ini">sucasny subor .INI alebo null, ak nie je</param>
    /// <param name="ports">sekcia Driver* → novy TablePort</param>
    public static InissIniFile Update(ResolvedConfig config, InissIniFile? ini, IReadOnlyDictionary<string, string> ports)
    {
        var result = ini?.Clone() ?? InissIniFile.Empty();
        foreach (var line in Find(result).Where(l => !ports.ContainsKey(l.Section)))
            Restore(result, line);

        var redirected = Find(result).Select(l => l.Section).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var (section, port) in ports)
        {
            if (!redirected.Contains(section))
            {
                if (result.HasSection(section))
                {
                    var original = result.GetString(section, TablePortName);
                    result.AddComment(section, $"{Marker} {(original is null ? NoPortText : OriginalPortText + original)}");
                }
                else
                {
                    // sekcia v .INI uplne nahradi register - musi obsahovat vsetko, co je zapisane v registri
                    result.AddComment(section, $"{Marker} {RegistryOnlyText}");
                    var resolved = config.FindSection(section);
                    foreach (var setting in resolved?.Settings ?? [])
                        if (setting.Value is not null && RegTools.IsExplicit(setting.Source))
                            result.Set(section, setting.Name, RegValues.ToIniText(setting.Value, setting.Setting.Type));
                }
            }

            result.Set(section, TablePortName, port);
        }

        if (ports.Count > 0)
        {
            if (!result.Comments(Environment).Any(IsMarker))
            {
                var original = result.GetString(Environment, OutToTableDriver);
                result.AddComment(Environment, $"{Marker} {(original is null ? NoOutText : OriginalOutText + original)}");
            }

            result.Set(Environment, OutToTableDriver, 1);
        }
        else if (result.Comments(Environment).FirstOrDefault(IsMarker) is { } marker)
        {
            var text = MarkerText(marker);
            if (text.StartsWith(OriginalOutText, StringComparison.Ordinal))
                result.Set(Environment, OutToTableDriver, text[OriginalOutText.Length..]);
            else
                result.Remove(Environment, OutToTableDriver);
            result.RemoveComments(Environment, IsMarker);
            // sekciu zalozilo presmerovanie
            if (result.Values(Environment).Count == 0 && result.Comments(Environment).Count == 0)
                result.RemoveSection(Environment);
        }

        return result;
    }

    /// <summary>Vrati linku do stavu pred presmerovanim.</summary>
    private static void Restore(InissIniFile ini, RedirectedLine line)
    {
        if (line.RegistryOnly)
        {
            ini.RemoveSection(line.Section);
            return;
        }

        if (line.OriginalPort is null)
            ini.Remove(line.Section, TablePortName);
        else
            ini.Set(line.Section, TablePortName, line.OriginalPort);
        ini.RemoveComments(line.Section, IsMarker);
    }

    private static bool IsMarker(string comment) => comment.StartsWith(Marker, StringComparison.Ordinal);

    private static string MarkerText(string comment) => comment[Marker.Length..].Trim();
}
