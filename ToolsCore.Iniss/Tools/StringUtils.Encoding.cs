using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace ToolsCore.Iniss.Tools;

/// <summary>
/// Prevod textov medzi CP1250 (subory INISS) a UTF-16, odstranenie diakritiky.
/// </summary>
public static partial class StringUtils
{
    /// <summary>
    /// Skonveruje pole bytov kodovany v ANSI (Windows 1250) na UTF8.
    /// </summary>
    /// <param name="data">Pole bytov.</param>
    /// <returns>skonverovane pole bytov.</returns>
    [ExcludeFromCodeCoverage]
    public static string AnsiToUTF(this byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (data.Length == 0)
            return "";

        return Encoding.UTF8.GetString(Encoding.Convert(Encodings.Win1250, Encodings.UTF8, data));
    }

    /// <summary>
    /// Skonvertuje retazec kodovany v ANSI (Windows 1250) na UTF8.
    /// </summary>
    /// <param name="data">Retazec.</param>
    /// <returns>skonvetovany retazec.</returns>
    [ExcludeFromCodeCoverage]
    public static string AnsiToUTF(this string data)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (data.Length == 0)
            return "";

        return Encoding.UTF8.GetString(Encoding.Convert(Encodings.Win1250, Encoding.UTF8, Encodings.Win1250.GetBytes(data)));
    }

    /// <summary>
    /// Skonveruje pole bytov kodovany v UTF8 na ANSI (Windows 1250).
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
    /// Skonvertuje retazec kódovaný v UTF8 na ANSI (Windows 1250).
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
    /// Odstrani diakritiku z retazca text. <br></br>
    /// Source: https://stackoverflow.com/a/37070320/14438039
    /// </summary>
    /// <param name="text">Text.</param>
    /// <returns>text bez diakritiky.</returns>
    public static string RemoveDiacritics(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

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
}
