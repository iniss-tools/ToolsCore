using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using ToolsCore.Properties;

namespace ToolsCore.Tools;

/// <summary>
/// Hlasenie chyb a navrhov cez GitHub Issues: otvori formular v repozitari programu s predvyplnenou verziou programu
/// a Windows. Nazvy formularov a identifikatory poli su v repozitari <c>iniss-tools/.github</c>
/// (<c>ISSUE_TEMPLATE/chyba.yml</c>, <c>navrh.yml</c>).
/// </summary>
public static class IssueReport
{
    // dlhsiu adresu Windows prehliadacu neodovzda
    internal const int MaxUrlLength = 2000;

    /// <summary>
    /// Adresa repozitara programu; repozitar sa vola ako program.
    /// </summary>
    public static string RepositoryUrl => LinkConsts.GitHub + Application.ProductName;

    /// <summary>
    /// Otvori formular na nahlasenie chyby.
    /// </summary>
    /// <param name="error">Text z okna chyby; dlhy sa v adrese skrati, cely ma byt v schranke.</param>
    public static void ReportBug(string? error = null) =>
        Utils.OpenShell(BugUrl(RepositoryUrl, Application.ProductVersion, RuntimeInformation.OSDescription, error));

    /// <summary>
    /// Otvori formular na navrh funkcie.
    /// </summary>
    public static void SuggestFeature() => Utils.OpenShell(FeatureUrl(RepositoryUrl));

    /// <summary>
    /// Nahradi priecinok pouzivatela Windows za <c>%USERPROFILE%</c>, aby sa vo verejnom hlaseni neukazalo jeho meno.
    /// </summary>
    public static string HideUserFolder(string text) =>
        HideUserFolder(text, Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

    internal static string HideUserFolder(string text, string userFolder) =>
        userFolder.Length == 0
            ? text
            : Regex.Replace(text, Regex.Escape(userFolder) + """(?=[\\/"'\s]|$)""", "%USERPROFILE%", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    internal static string BugUrl(string repository, string version, string system, string? error)
    {
        var url = $"{repository}/issues/new?template=chyba.yml&verzia={Uri.EscapeDataString(version)}&system={Uri.EscapeDataString(system)}";
        if (string.IsNullOrEmpty(error))
            return url;

        url += "&chyba=";
        return url + EscapeUpTo(error, MaxUrlLength - url.Length);
    }

    internal static string FeatureUrl(string repository) => repository + "/issues/new?template=navrh.yml";

    /// <summary>
    /// Text zakodovany do adresy; ked sa nezmesti, skrateny s poznamkou, ze cely je v schranke.
    /// </summary>
    private static string EscapeUpTo(string text, int length)
    {
        var escaped = Uri.EscapeDataString(text);
        if (escaped.Length <= length)
            return escaped;

        var note = Uri.EscapeDataString("\r\n" + Resources.Report_Cut);
        var result = new StringBuilder();
        foreach (var rune in text.EnumerateRunes())
        {
            var next = Uri.EscapeDataString(rune.ToString());
            if (result.Length + next.Length + note.Length > length)
                break;
            result.Append(next);
        }

        return result.Append(note).ToString();
    }
}
