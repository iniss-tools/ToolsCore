using System.Collections;
using ToolsCore.XML;

namespace ToolsCore.Commands;

/// <summary>
/// Prikazy hlavneho okna: spolocna obnova stavu, klavesove skratky z nastaveni a ich spracovanie.
/// </summary>
public sealed class CommandSet : IEnumerable<AppCommand>
{
    private readonly List<AppCommand> _commands = [];

    /// <summary>
    /// Prida prikaz.
    /// </summary>
    /// <returns>pridany prikaz (na naviazanie prvkov cez <see cref="AppCommand.Bind" />).</returns>
    public AppCommand Add(CommandInfo info, Action execute, Func<bool>? canExecute = null) =>
        Add(new AppCommand(info, execute, canExecute));

    /// <summary>
    /// Prida prikaz, ktoreho akcia dostane prvok, cez ktory bol vyvolany (<see langword="null" /> = klavesova skratka).
    /// </summary>
    public AppCommand Add(CommandInfo info, Action<ToolStripItem?> execute, Func<bool>? canExecute = null) =>
        Add(new AppCommand(info, execute, canExecute));

    private AppCommand Add(AppCommand command)
    {
        if (_commands.Any(c => c.Id == command.Id))
            throw new ArgumentException($"Prikaz {command.Id} uz existuje.", nameof(command));

        _commands.Add(command);
        return command;
    }

    /// <summary>
    /// Prikaz podla identifikatora.
    /// </summary>
    public AppCommand this[string id] => _commands.First(c => c.Id == id);

    /// <summary>
    /// Povoli alebo zakaze prvky vsetkych prikazov podla ich aktualneho stavu.
    /// </summary>
    public void UpdateStates()
    {
        foreach (var command in _commands)
            command.UpdateState();
    }

    /// <summary>
    /// Nastavi prikazom skratky z nastaveni; prikaz bez zaznamu dostane predvolenu skratku.
    /// </summary>
    public void ApplyShortcuts(ShortcutMap map)
    {
        foreach (var command in _commands)
            command.SetShortcut((Keys)map.Get(command.Info));
    }

    /// <summary>
    /// Vykona prikaz s klavesovou skratkou <paramref name="keyData" />, ak sa da vykonat. Volat z
    /// <see cref="Control.ProcessCmdKey" /> - skratky tak funguju aj pri skrytej ponuke a pri polozkach s podponukou.
    /// </summary>
    /// <returns><see langword="true" />, ak sa prikaz vykonal (kláves je spracovany).</returns>
    public bool ProcessShortcut(Keys keyData)
    {
        if (keyData == Keys.None)
            return false;

        var command = _commands.FirstOrDefault(c => c.ShortcutKeys == keyData && c.CanExecute());
        return command is not null && command.Execute();
    }

    /// <inheritdoc />
    public IEnumerator<AppCommand> GetEnumerator() => _commands.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
