using ToolsCore.XML;

namespace ToolsCore.CodeEditor;

/// <summary>
/// Farby editora kodu VB.NET (polozky <see cref="VisualBasicColors" />): text a pozadie, zvyraznenie syntaxe, cisla riadkov
/// a podciarknutie zisteni prekladu.
/// </summary>
public sealed class VisualBasicScheme
{
    /// <summary>Bezny text a pozadie editora.</summary>
    public ColorSetting Text { get; } = new();

    /// <summary>Klucove slovo.</summary>
    public ColorSetting Keyword { get; } = new();

    /// <summary>Retazec a datum.</summary>
    public ColorSetting String { get; } = new();

    /// <summary>Cislo.</summary>
    public ColorSetting Number { get; } = new();

    /// <summary>Komentar.</summary>
    public ColorSetting Comment { get; } = new();

    /// <summary>Direktiva prekladaca (<c>#If</c>, <c>#Region</c>).</summary>
    public ColorSetting Preprocessor { get; } = new();

    /// <summary>Operator.</summary>
    public ColorSetting Operator { get; } = new();

    /// <summary>Nazov metody (volanie aj definicia).</summary>
    public ColorSetting Method { get; } = new();

    /// <summary>Cisla riadkov (text a pozadie okraja).</summary>
    public ColorSetting LineNumber { get; } = new();

    /// <summary>Podciarknutie chyby prekladu.</summary>
    public ColorSetting Error { get; } = new();

    /// <summary>Podciarknutie upozornenia (varovania kompilatora).</summary>
    public ColorSetting Warning { get; } = new();
}
