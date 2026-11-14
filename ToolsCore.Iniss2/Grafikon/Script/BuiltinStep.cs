namespace ToolsCore.Iniss2.Grafikon.Script;

/// <summary>
/// Vstavana metoda servera s jednoduchymi argumentmi, napr. <c>DruhVlaku("V4", 0, True)</c> alebo <c>Dodatky()</c>.
/// Hovori v jazyku, ktory sa prave hlasi.
/// </summary>
/// <param name="Method">nazov metody z <see cref="ScriptBuiltins" /></param>
/// <param name="Arguments">argumenty ako v kode (aj pomenovane <c>nazov:=hodnota</c>)</param>
public sealed record BuiltinStep(string Method, IReadOnlyList<string> Arguments) : ScriptStep
{
    /// <inheritdoc />
    public bool Equals(BuiltinStep? other) =>
        other is not null && Comment == other.Comment && Method == other.Method && Arguments.SequenceEqual(other.Arguments);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Method, Arguments.Count, Comment);
}
