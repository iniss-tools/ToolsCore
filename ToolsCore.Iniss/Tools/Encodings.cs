using System.Diagnostics.CodeAnalysis;

namespace ToolsCore.Iniss.Tools;

/// <summary>
/// Trieda obsahujuca najdolezitejsie kodovania textu.
/// </summary>
[ExcludeFromCodeCoverage]
public static class Encodings
{
    /// <summary>
    /// Kodovanie Windows-1250.
    /// </summary>
    public static readonly Encoding Win1250 = Encoding.GetEncoding(1250);

    /// <summary>
    /// Kodovanie UTF-8.
    /// </summary>
    public static readonly Encoding UTF8 = Encoding.UTF8;

    /// <summary>
    /// Kod Kamenickych (tabule ELEN, ELENOLD, ELEN16Kam).
    /// </summary>
    public static readonly Encoding Kamenicky = new KamenickyEncoding();
}