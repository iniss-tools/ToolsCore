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

/// <summary>
/// Strom klavesovych skratiek pre nastavenia: hlavne okno v poradi jeho ponuky, prikazy bez polozky v ponuke v skupinach
/// podla <see cref="CommandInfo.Group" /> a dalsie okna podla <see cref="CommandInfo.Window" />.
/// </summary>
public static class KeymapTree
{
    /// <summary>
    /// Zostavi strom.
    /// </summary>
    /// <param name="catalog">Vsetky prikazy programu v poradi katalogu.</param>
    /// <param name="bound">Prikazy hlavneho okna naviazane na prvky (ponuka, kontextove ponuky, panel nastrojov);
    /// <see langword="null" /> = bez okna, prikazy sa zoradia len podla katalogu.</param>
    /// <param name="mainWindowText">Nazov korena s prikazmi hlavneho okna.</param>
    public static List<KeymapNode> Build(IEnumerable<CommandInfo> catalog, IEnumerable<AppCommand>? bound, string mainWindowText)
    {
        var commands = catalog.ToList();
        var byId = commands.ToDictionary(c => c.Id, StringComparer.Ordinal);
        var itemCommands = new Dictionary<ToolStripItem, CommandInfo>();
        var itemsOf = new Dictionary<string, IReadOnlyList<ToolStripItem>>(StringComparer.Ordinal);
        foreach (var command in bound ?? [])
        {
            if (!byId.TryGetValue(command.Id, out var info))
                continue;
            itemsOf[info.Id] = command.Items;
            foreach (var item in command.Items)
                itemCommands.TryAdd(item, info);
        }

        var placed = new HashSet<string>(StringComparer.Ordinal);
        var main = new KeymapNode(mainWindowText);
        foreach (var menu in MainMenus(itemCommands.Keys))
            AddMenu(menu.Items, main, itemCommands, placed);

        var roots = new List<KeymapNode> { main };
        foreach (var command in commands.Where(c => !placed.Contains(c.Id)))
        {
            var root = command.Window is null ? main : RootFor(roots, command.Window);
            var parent = command.Group is null ? root : GroupFor(root, command.Group);
            var text = itemsOf.TryGetValue(command.Id, out var items) && items.Count > 0 ? ItemText(items[0]) : command.Text;
            parent.Children.Add(new KeymapNode(text.Length > 0 ? text : command.Text, command));
        }

        return roots.Where(r => r.Children.Count > 0).ToList();
    }

    /// <summary>
    /// Text polozky ponuky pre strom - bez znaku klavesu <c>&amp;</c> a trojbodky na konci.
    /// </summary>
    public static string ItemText(ToolStripItem item)
    {
        var text = (string.IsNullOrWhiteSpace(item.Text) ? item.ToolTipText : item.Text) ?? "";
        text = text.Replace("&&", "\0", StringComparison.Ordinal).Replace("&", "", StringComparison.Ordinal)
            .Replace("\0", "&", StringComparison.Ordinal).Trim();
        if (text.EndsWith("...", StringComparison.Ordinal))
            text = text[..^3];
        return text.TrimEnd('…').TrimEnd();
    }

    // hlavne ponuky okna, v ktorych su naviazane polozky (kontextova ponuka nie je MenuStrip)
    private static List<MenuStrip> MainMenus(IEnumerable<ToolStripItem> items)
    {
        var menus = new List<MenuStrip>();
        foreach (var item in items)
        {
            var owner = item.Owner;
            while (owner is ToolStripDropDown { OwnerItem: { } ownerItem })
                owner = ownerItem.Owner;
            if (owner is MenuStrip menu && !menus.Contains(menu))
                menus.Add(menu);
        }

        return menus;
    }

    private static void AddMenu(ToolStripItemCollection items, KeymapNode parent, Dictionary<ToolStripItem, CommandInfo> itemCommands,
        HashSet<string> placed)
    {
        foreach (ToolStripItem item in items)
        {
            if (item is ToolStripSeparator)
                continue;

            var command = itemCommands.TryGetValue(item, out var info) && placed.Add(info.Id) ? info : null;
            var node = new KeymapNode(ItemText(item), command);
            if (item is ToolStripDropDownItem { HasDropDownItems: true } dropDown)
                AddMenu(dropDown.DropDownItems, node, itemCommands, placed);

            if (node.Command is not null || node.Children.Count > 0)
                parent.Children.Add(node);
        }
    }

    private static KeymapNode RootFor(List<KeymapNode> roots, string window)
    {
        var root = roots.FirstOrDefault(r => r.Text == window);
        if (root is null)
        {
            root = new KeymapNode(window);
            roots.Add(root);
        }

        return root;
    }

    private static KeymapNode GroupFor(KeymapNode root, string group)
    {
        var node = root.Children.FirstOrDefault(c => c.Command is null && c.Text == group);
        if (node is null)
        {
            node = new KeymapNode(group);
            root.Children.Add(node);
        }

        return node;
    }
}
