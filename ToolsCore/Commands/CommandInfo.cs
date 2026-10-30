namespace ToolsCore.Commands;

/// <summary>
/// Popis prikazu programu nezavisly od okna: identifikator (zaroven nazov prvku v config.xml), text pre nastavenia
/// klavesovych skratiek a predvolena skratka.
/// </summary>
/// <param name="Id">Identifikator prikazu, pouzity aj ako nazov XML prvku skratky.</param>
/// <param name="Text">Nazov prikazu v nastaveniach skratiek, ak prikaz nema polozku v ponuke okna.</param>
/// <param name="DefaultKeys">Predvolena klavesova skratka.</param>
/// <param name="Group">Skupina v strome skratiek pre prikaz bez polozky v hlavnej ponuke (napr. kontextova ponuka
/// zoznamu); <see langword="null" /> = priamo pod oknom.</param>
/// <param name="Window">Okno, v ktorom skratka plati (v nom sa hladaju konflikty); <see langword="null" /> = hlavne okno.</param>
public sealed record CommandInfo(string Id, string Text, Keys DefaultKeys, string? Group = null, string? Window = null)
{
    /// <summary>
    /// Popis prikazu s predvolenou skratkou z <see cref="Shortcut" />.
    /// </summary>
    // nazvy parametrov ako pri hlavnom konstruktore - pomenovany argument Window: plati pre oba
    // ReSharper disable InconsistentNaming
    public CommandInfo(string Id, string Text, Shortcut DefaultShortcut, string? Group = null, string? Window = null)
        : this(Id, Text, (Keys)DefaultShortcut, Group, Window)
    // ReSharper restore InconsistentNaming
    {
    }
}
