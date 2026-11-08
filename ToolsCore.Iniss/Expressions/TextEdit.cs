namespace ToolsCore.Iniss.Expressions;

/// <summary>
/// Jedna uprava textu: nahradenie useku <paramref name="Start"/>..<paramref name="Start"/>+<paramref name="Length"/> textom <paramref name="NewText"/>.
/// </summary>
public sealed record TextEdit(int Start, int Length, string NewText)
{
    /// <summary>Posunie upravu o <paramref name="offset"/> znakov.</summary>
    public TextEdit Shift(int offset) => this with { Start = Start + offset };
}
