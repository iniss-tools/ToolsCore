namespace ToolsCore.Iniss2.Grafikon.Script;

/// <summary>
/// Volanie inej funkcie skriptu (aj rucnej) s jednoduchymi argumentmi, napr. <c>CZ_Pozice_prijezd()</c>,
/// <c>Odklon(Nothing)</c> alebo vzoru so stanicami <c>CZ_Konec(aiStanice)</c>.
/// </summary>
/// <param name="Function">nazov funkcie skriptu ako v kode</param>
/// <param name="Arguments">argumenty ako v kode</param>
public sealed record CallStep(string Function, IReadOnlyList<string> Arguments) : ScriptStep
{
    /// <inheritdoc />
    public bool Equals(CallStep? other) =>
        other is not null && Comment == other.Comment && Function == other.Function && Arguments.SequenceEqual(other.Arguments);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Function, Arguments.Count, Comment);
}
