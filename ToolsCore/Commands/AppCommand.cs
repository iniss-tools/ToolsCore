namespace ToolsCore.Commands;

/// <summary>
/// Prikaz programu naviazany naraz na tlacidlo panela nastrojov aj polozky ponuk. Stav (povolene/zakazane) a klavesova
/// skratka sa nastavuju na jednom mieste pre vsetky naviazane prvky.
/// </summary>
public sealed class AppCommand
{
    private readonly Action<ToolStripItem?> _execute;
    private readonly Func<bool>? _canExecute;
    private readonly List<ToolStripItem> _items = [];

    /// <summary>
    /// Vytvori prikaz.
    /// </summary>
    /// <param name="info">Popis prikazu.</param>
    /// <param name="execute">Akcia prikazu.</param>
    /// <param name="canExecute">Ci sa prikaz da vykonat; <see langword="null" /> = vzdy.</param>
    public AppCommand(CommandInfo info, Action execute, Func<bool>? canExecute = null)
        : this(info, _ => execute(), canExecute)
    {
        ArgumentNullException.ThrowIfNull(execute);
    }

    /// <summary>
    /// Vytvori prikaz, ktoreho akcia dostane prvok, cez ktory bol vyvolany (<see langword="null" /> = klavesova skratka).
    /// </summary>
    /// <param name="info">Popis prikazu.</param>
    /// <param name="execute">Akcia prikazu.</param>
    /// <param name="canExecute">Ci sa prikaz da vykonat; <see langword="null" /> = vzdy.</param>
    public AppCommand(CommandInfo info, Action<ToolStripItem?> execute, Func<bool>? canExecute = null)
    {
        ArgumentNullException.ThrowIfNull(info);
        ArgumentNullException.ThrowIfNull(execute);
        Info = info;
        _execute = execute;
        _canExecute = canExecute;
        ShortcutKeys = (Keys)info.DefaultShortcut;
    }

    /// <summary>
    /// Popis prikazu.
    /// </summary>
    public CommandInfo Info { get; }

    /// <summary>
    /// Identifikator prikazu.
    /// </summary>
    public string Id => Info.Id;

    /// <summary>
    /// Aktualna klavesova skratka prikazu.
    /// </summary>
    public Keys ShortcutKeys { get; private set; }

    /// <summary>
    /// Prvky naviazane na prikaz.
    /// </summary>
    public IReadOnlyList<ToolStripItem> Items => _items;

    /// <summary>
    /// Ci sa prikaz da prave vykonat.
    /// </summary>
    public bool CanExecute() => _canExecute?.Invoke() ?? true;

    /// <summary>
    /// Vykona prikaz, ak sa da vykonat.
    /// </summary>
    /// <param name="source">Prvok, cez ktory bol prikaz vyvolany; <see langword="null" /> = klavesova skratka.</param>
    /// <returns><see langword="true" />, ak sa prikaz vykonal.</returns>
    public bool Execute(ToolStripItem? source = null)
    {
        if (!CanExecute())
            return false;

        _execute(source);
        return true;
    }

    /// <summary>
    /// Naviaze prikaz na prvky: kliknutie ho vykona, polozka ponuky dostane jeho skratku. Pri tlacidle s rozbalovacou
    /// ponukou (<see cref="ToolStripSplitButton" />) sa prikaz vykona klikom na samotne tlacidlo.
    /// </summary>
    public AppCommand Bind(params ToolStripItem[] items)
    {
        foreach (var item in items)
        {
            if (item is ToolStripSplitButton split)
                split.ButtonClick += (_, _) => Execute(split);
            else
                item.Click += (sender, _) => Execute(sender as ToolStripItem);

            _items.Add(item);
            ApplyShortcut(item);
        }

        return this;
    }

    /// <summary>
    /// Naviaze prvky len na stav a skratku - kliknutie spracuvaju samy (napr. prepinac s
    /// <see cref="ToolStripButton.CheckOnClick" />, ktory by klik prikazu prepol druhy raz).
    /// </summary>
    public AppCommand BindState(params ToolStripItem[] items)
    {
        foreach (var item in items)
        {
            _items.Add(item);
            ApplyShortcut(item);
        }

        return this;
    }

    /// <summary>
    /// Povoli alebo zakaze naviazane prvky podla <see cref="CanExecute" />.
    /// </summary>
    public void UpdateState()
    {
        var enabled = CanExecute();
        foreach (var item in _items)
            item.Enabled = enabled;
    }

    /// <summary>
    /// Nastavi klavesovu skratku prikazu a zobrazi ju v naviazanych polozkach ponuky.
    /// </summary>
    public void SetShortcut(Keys keys)
    {
        ShortcutKeys = keys;
        foreach (var item in _items)
            ApplyShortcut(item);
    }

    private void ApplyShortcut(ToolStripItem item)
    {
        // polozka s podponukou skratku sama nespracuje - vykona ju CommandSet.ProcessShortcut
        if (item is ToolStripMenuItem menuItem)
            menuItem.ShortcutKeys = ShortcutKeys;
    }
}
