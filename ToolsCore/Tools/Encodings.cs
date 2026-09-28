using System.Diagnostics.CodeAnalysis;

namespace ToolsCore.Tools;

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
}