namespace ToolsCore.Commands;

/// <summary>
/// Popis prikazu programu nezavisly od okna: identifikator (zaroven nazov prvku v config.xml), text pre nastavenia
/// klavesovych skratiek a predvolena skratka.
/// </summary>
/// <param name="Id">Identifikator prikazu, pouzity aj ako nazov XML prvku skratky.</param>
/// <param name="Text">Nazov prikazu zobrazeny v nastaveniach skratiek.</param>
/// <param name="DefaultShortcut">Predvolena klavesova skratka.</param>
public sealed record CommandInfo(string Id, string Text, Shortcut DefaultShortcut);
