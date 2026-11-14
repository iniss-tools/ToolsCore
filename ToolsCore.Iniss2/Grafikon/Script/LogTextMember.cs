namespace ToolsCore.Iniss2.Grafikon.Script;

/// <summary>
/// Funkcia (Sub alebo Function) triedy <c>LogText</c> a jej poloha v riadkoch skriptu. Indexy riadkov su od 0.
/// </summary>
/// <param name="Name">nazov funkcie ako v kode</param>
public sealed record LogTextMember(string Name)
{
    /// <summary>Klucove slovo: <c>Sub</c> alebo <c>Function</c>.</summary>
    public string Keyword { get; init; } = "Sub";

    /// <summary>Modifikatory pred klucovym slovom ako v kode, napr. <c>Public</c>.</summary>
    public IReadOnlyList<string> Modifiers { get; init; } = [];

    /// <summary>Text parametrov medzi zatvorkami (orezany); <see langword="null" />, ak podpis nema zatvorky.</summary>
    public string? Parameters { get; init; }

    /// <summary>Zvysok podpisu za parametrami, napr. <c>As Boolean</c>; prazdny = nic.</summary>
    public string Rest { get; init; } = "";

    /// <summary>Oznacenia nad funkciou v poradi kodu.</summary>
    public IReadOnlyList<LogTextAttribute> Attributes { get; init; } = [];

    /// <summary>Oznacenia sa dali rozlozit; <see langword="false" /> = neobvykly zapis, funkciu treba brat ako rucnu.</summary>
    public bool AttributesParsed { get; init; } = true;

    /// <summary>Prvy riadok funkcie: prve oznacenie alebo podpis. Komentare nad nim k funkcii nepatria.</summary>
    public int Start { get; init; }

    /// <summary>Posledny riadok podpisu (podpis moze pokracovat na dalsich riadkoch); telo zacina za nim.</summary>
    public int Signature { get; init; }

    /// <summary>Riadok <c>End Sub</c> alebo <c>End Function</c>.</summary>
    public int End { get; init; }

    /// <summary>Komentar za <c>End Sub</c> vratane apostrofu, napr. <c>' D1006</c>; <see langword="null" /> = bez komentara.</summary>
    public string? EndComment { get; init; }

    /// <summary>Odsadenie riadka podpisu.</summary>
    public string Indent { get; init; } = "";

    /// <inheritdoc />
    public bool Equals(LogTextMember? other) =>
        other is not null && Name == other.Name && Keyword == other.Keyword && Modifiers.SequenceEqual(other.Modifiers)
        && Parameters == other.Parameters && Rest == other.Rest && Attributes.SequenceEqual(other.Attributes)
        && AttributesParsed == other.AttributesParsed && Start == other.Start && Signature == other.Signature && End == other.End
        && EndComment == other.EndComment && Indent == other.Indent;

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Name, Start, End);
}
