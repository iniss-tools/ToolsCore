namespace ToolsCore.Iniss2.Grafikon.Script;

/// <summary>
/// Vysledok citania funkcie skriptu: hlasenie upravitelne v editore alebo dovod, preco ostava rucnym kodom.
/// </summary>
/// <param name="Member">funkcia v skripte</param>
/// <param name="Report">hlasenie; <see langword="null" />, ak je funkcia rucna</param>
/// <param name="Reason">dovod rucnej funkcie; <see cref="ScriptManualReason.None" /> pri hlaseni</param>
/// <param name="Line">riadok, ktory dovod sposobil (index od 0); pri hlaseni prvy riadok funkcie</param>
public sealed record ScriptReadResult(LogTextMember Member, ScriptReport? Report, ScriptManualReason Reason, int Line);
