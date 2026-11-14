namespace ToolsCore.Iniss2.Grafikon.Script;

/// <summary>
/// Oznacenie (atribut VB.NET) nad funkciou skriptu hlaseni, napr. <c>&lt;Dodatek("Bufet")&gt;</c>.
/// </summary>
/// <param name="Name">nazov ako v kode (napr. <c>Dodatek</c>, <c>InterpreterAttribute</c>)</param>
/// <param name="Arguments">argumenty ako v kode, orezane; bez zatvoriek = ziadne</param>
public sealed record LogTextAttribute(string Name, IReadOnlyList<string> Arguments)
{
    /// <summary>
    /// Text prveho argumentu, ak je retazcovy literal (popis hlasenia); inak <see langword="null" />.
    /// </summary>
    public string? Description => Arguments.Count > 0 ? VbText.StringValue(Arguments[0]) : null;

    /// <inheritdoc />
    public bool Equals(LogTextAttribute? other) =>
        other is not null && Name == other.Name && Arguments.SequenceEqual(other.Arguments);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Name, Arguments.Count);
}
