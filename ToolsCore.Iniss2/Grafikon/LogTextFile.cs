using System.Text.RegularExpressions;
using ToolsCore.Iniss.Tools;

namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Hlasenia servera - subor LogText.vb v priecinku dat (kod VB.NET, server ho preklada pri starte, ked ma zvuk).
/// </summary>
public static partial class LogTextFile
{
    /// <summary>
    /// Nazov suboru v priecinku dat.
    /// </summary>
    public const string FileName = "LogText.vb";

    /// <summary>
    /// Text suboru presne ako na disku (CP1250, konce riadkov ostanu); <see langword="null" />, ak subor chyba.
    /// </summary>
    /// <param name="dataDir">priecinok dat servera</param>
    public static string? ReadText(string dataDir)
    {
        var file = Path.Combine(dataDir, FileName);
        return File.Exists(file) ? File.ReadAllText(file, Encodings.Win1250) : null;
    }

    /// <summary>
    /// Zapise text suboru v CP1250. Znaky, ktore CP1250 nema, sa nedaju zapisat (server subor cita v CP1250).
    /// </summary>
    /// <param name="dataDir">priecinok dat servera</param>
    /// <param name="text">text suboru</param>
    /// <returns><see langword="false" />, ak subor uz mal tento obsah.</returns>
    public static bool WriteText(string dataDir, string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var file = Path.Combine(dataDir, FileName);
        if (File.Exists(file) && File.ReadAllText(file, Encodings.Win1250) == text)
            return false;

        File.WriteAllText(file, text, Encodings.Win1250);
        return true;
    }

    /// <summary>
    /// Funkcie suboru a oznacenia nad nimi (napr. <c>Dodatek</c>, <c>Vyluka</c>). Chybajuci subor = <see langword="null" />.
    /// </summary>
    /// <param name="dataDir">priecinok dat servera</param>
    public static Dictionary<string, HashSet<string>>? ReadFunctions(string dataDir)
    {
        var file = Path.Combine(dataDir, FileName);
        return File.Exists(file) ? Functions(File.ReadAllLines(file, Encodings.Win1250)) : null;
    }

    /// <summary>
    /// Funkcie a oznacenia nad nimi z riadkov kodu. Zakomentovane riadky sa preskocia, VB nerozlisuje velkost pismen.
    /// </summary>
    /// <param name="lines">riadky suboru</param>
    /// <returns>nazov funkcie → nazvy oznaceni bez <c>Attribute</c></returns>
    public static Dictionary<string, HashSet<string>> Functions(IEnumerable<string> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);

        var functions = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        var attributes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in lines.Select(line => line.Trim()))
        {
            if (line.Length == 0 || line.StartsWith('\''))
                continue;

            foreach (Match attribute in AttributePattern().Matches(line))
                attributes.Add(attribute.Groups[1].Value);

            var function = FunctionPattern().Match(line);
            if (!function.Success)
                continue;

            functions.TryAdd(function.Groups[1].Value, attributes);
            attributes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        return functions;
    }

    /// <summary>
    /// Popisy funkcii z prveho oznacenia s textom, napr. <c>&lt;Dodatek("Upozornenie na vyluku")&gt;</c>. Chybajuci subor =
    /// <see langword="null" />.
    /// </summary>
    /// <param name="dataDir">priecinok dat servera</param>
    public static Dictionary<string, string>? ReadDescriptions(string dataDir)
    {
        var file = Path.Combine(dataDir, FileName);
        return File.Exists(file) ? Descriptions(File.ReadAllLines(file, Encodings.Win1250)) : null;
    }

    /// <summary>
    /// Popisy funkcii z riadkov kodu: text prveho oznacenia nad funkciou, ktore ma text v uvodzovkach.
    /// </summary>
    /// <param name="lines">riadky suboru</param>
    /// <returns>nazov funkcie → popis</returns>
    public static Dictionary<string, string> Descriptions(IEnumerable<string> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);

        var descriptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string? description = null;

        foreach (var line in lines.Select(line => line.Trim()))
        {
            if (line.Length == 0 || line.StartsWith('\''))
                continue;

            if (description is null && DescriptionPattern().Match(line) is { Success: true } attribute)
                description = attribute.Groups[1].Value.Replace("\"\"", "\"", StringComparison.Ordinal);

            var function = FunctionPattern().Match(line);
            if (!function.Success)
                continue;

            if (description is not null)
                descriptions.TryAdd(function.Groups[1].Value, description);
            description = null;
        }

        return descriptions;
    }

    [GeneratedRegex(@"<(\w+)\(")]
    private static partial Regex AttributePattern();

    [GeneratedRegex(@"<\w+\(\s*""((?:[^""]|"""")*)""")]
    private static partial Regex DescriptionPattern();

    [GeneratedRegex(@"\b(?:Sub|Function)\s+([\p{L}\p{N}_]+)\s*\(")]
    private static partial Regex FunctionPattern();
}
