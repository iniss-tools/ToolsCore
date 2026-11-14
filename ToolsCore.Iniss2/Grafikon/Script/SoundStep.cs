namespace ToolsCore.Iniss2.Grafikon.Script;

/// <summary>
/// Zvuk z banky: <c>Zvuk("CZ/Vety/bezpec", 0, 200)</c>. Zvuk ineho jazyka nez prave hlaseneho server preskoci aj s
/// pauzami, zvuk bez jazyka hra v kazdom jazyku, ktory ho v banke ma.
/// </summary>
/// <param name="Path">kluc zvuku <c>jazyk/skupina/zvuk</c> alebo <c>skupina/zvuk</c></param>
/// <param name="PauseBefore">pauza pred zvukom v ms</param>
/// <param name="PauseAfter">pauza po zvuku v ms</param>
public sealed record SoundStep(string Path, int PauseBefore = 0, int PauseAfter = 0) : ScriptStep
{
    /// <summary>
    /// Jazyk z kluca zvuku (prva cast z troch); <see langword="null" />, ak kluc jazyk nema.
    /// </summary>
    public string? Language => Path.Split('/') is { Length: 3 } parts ? parts[0] : null;
}
