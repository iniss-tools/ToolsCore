namespace ToolsCore.Iniss.Tools;

/// <summary>
/// Typ znaku na zaciatku riadku.
/// </summary>
public enum ReadStartChar
{
    /// <summary>
    /// Neprazdny riadok.
    /// </summary>
    NonEmpty,

    /// <summary>
    /// Prazdny riadok.
    /// </summary>
    Empty,

    /// <summary>
    /// Bodkociarka.
    /// </summary>
    Semicolon,

    /// <summary>
    /// Mriezka (#).
    /// </summary>
    Slash,

    /// <summary>
    /// Znak konca suboru.
    /// </summary>
    Eof
}
