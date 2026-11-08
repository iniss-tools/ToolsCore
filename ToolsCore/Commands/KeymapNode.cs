namespace ToolsCore.Commands;

/// <summary>
/// Uzol stromu klavesovych skratiek: okno, ponuka, skupina alebo prikaz. Polozka ponuky s podponukou moze byt prikazom
/// aj skupinou zaroven.
/// </summary>
public sealed class KeymapNode(string text, CommandInfo? command = null)
{
    /// <summary>
    /// Text uzla (text polozky ponuky bez <c>&amp;</c> a trojbodky).
    /// </summary>
    public string Text { get; } = text;

    /// <summary>
    /// Prikaz uzla; <see langword="null" /> = len skupina.
    /// </summary>
    public CommandInfo? Command { get; } = command;

    /// <summary>
    /// Podriadene uzly v poradi ponuky.
    /// </summary>
    public List<KeymapNode> Children { get; } = [];

    /// <summary>
    /// Vsetky prikazy v podstrome (vratane tohto uzla).
    /// </summary>
    public IEnumerable<CommandInfo> Commands()
    {
        if (Command is not null)
            yield return Command;
        foreach (var command in Children.SelectMany(child => child.Commands()))
            yield return command;
    }

    /// <inheritdoc />
    public override string ToString() => Text;
}
