namespace ToolsCore.Iniss.Tools;

/// <summary>
/// Typ zapisu prvku do slovnika.
/// </summary>
public enum WriteType
{
    /// <summary>
    /// Zapis cisla (hodnota bez uvodzoviek).
    /// </summary>
    WriteNumber,

    /// <summary>
    /// Zapis retazca UTF8 (v pripade ze je prvok <see langword="null" />, vyhodi vynimku).
    /// </summary>
    WriteStringUTF8,

    /// <summary>
    /// Zapis retazca UTF8 (v pripade ze je prvok <see langword="null" />, vlozi do slovnika "").
    /// </summary>
    WriteStringUTF8Nullable,

    /// <summary>
    /// Zapis retazca ANSI (v pripade ze je prvok <see langword="null" />, vyhodi vynimku).
    /// </summary>
    WriteStringANSI,

    /// <summary>
    /// Zapis retazca ANSI (v pripade ze je prvok <see langword="null" />, vlozi do slovnika "").
    /// </summary>
    WriteStringANSINullable
}
