namespace ToolsCore.Iniss2.Grafikon.Script;

/// <summary>
/// Blok krokov hlasenia: kroky v jazykovom bloku <c>If (Jazyk = "CZ") Then … End If</c> a v dalsich vnorenych
/// podmienkach. Susedne bloky, ktorych podmienky sa zhoduju na zaciatku, maju spolocne vonkajsie <c>If</c>; podmienka
/// s <see cref="ScriptCondition.Else" /> hned za tou istou podmienkou je jej vetva <c>Else</c>.
/// </summary>
/// <param name="Language">jazyk vonkajsieho bloku; <see langword="null" /> = bez jazykovej podmienky</param>
/// <param name="Steps">kroky v poradi kodu</param>
public sealed record ScriptBlock(string? Language, IReadOnlyList<ScriptStep> Steps)
{
    /// <summary>Dalsie podmienky od vonkajsej po vnutornu (vo vnutri jazykoveho bloku); prazdne = ziadne.</summary>
    public IReadOnlyList<ScriptCondition> Conditions { get; init; } = [];

    /// <summary>
    /// Vsetky podmienky bloku od vonkajsej: jazyk ako <c>(Jazyk = "CZ")</c> a dalsie podmienky.
    /// </summary>
    public IEnumerable<ScriptCondition> Chain =>
        Language is null ? Conditions : Conditions.Prepend(new ScriptCondition(ScriptReportWriter.LanguageCondition(Language)));

    /// <inheritdoc />
    public bool Equals(ScriptBlock? other) =>
        other is not null && Language == other.Language && Steps.SequenceEqual(other.Steps) && Conditions.SequenceEqual(other.Conditions);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Language, Steps.Count, Conditions.Count);
}
