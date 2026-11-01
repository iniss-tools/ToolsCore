using ToolsCore.Properties;
using ToolsCore.XML;

namespace ToolsCore.Tools;

/// <summary>
/// Stlpce tabulky, ktore si program pamata: poradie (tahanim hlavicky), sirku (tahanim okraja hlavicky) a viditelnost
/// (ponuka na hlavicke s obnovenim predvolenych stlpcov). Usporiadanie sa zapise do konfiguracie hned po zmene.
/// Stlpce s automatickou sirkou z navrhu dostanu sirku podla obsahu, kym ju pouzivatel nezmeni.
/// </summary>
public sealed class GridColumns : IDisposable
{
    private readonly DataGridView _grid;
    private readonly string _id;
    private readonly Func<ConfigBase> _config;
    private readonly Action _save;
    private readonly Dictionary<DataGridViewColumn, ColumnDefault> _defaults = [];
    private readonly HashSet<DataGridViewColumn> _userWidths = [];
    private readonly ContextMenuStrip _menu = new();
    private bool _headerDown;
    private bool _resizing;

    private readonly record struct ColumnDefault(int Index, bool Visible, DataGridViewAutoSizeColumnMode Mode, int Width);

    private GridColumns(DataGridView grid, string id, Func<ConfigBase> config, Action save, IReadOnlyCollection<string> hiddenByDefault)
    {
        _grid = grid;
        _id = id;
        _config = config;
        _save = save;

        foreach (DataGridViewColumn column in grid.Columns)
        {
            var visible = column.Visible && !hiddenByDefault.Contains(column.Name);
            _defaults[column] = new ColumnDefault(column.DisplayIndex, visible, column.AutoSizeMode, column.Width);
            column.Visible = visible;
            column.HeaderCell.ContextMenuStrip = _menu;
        }

        grid.AllowUserToOrderColumns = true;
        Apply(config().GridOf(id));
        AutoSize();

        _menu.Opening += Menu_Opening;
        grid.MouseDown += Grid_MouseDown;
        grid.MouseUp += Grid_MouseUp;
        grid.ColumnWidthChanged += Grid_ColumnWidthChanged;
        grid.ColumnDividerDoubleClick += Grid_ColumnDividerDoubleClick;
        grid.DataBindingComplete += (_, e) =>
        {
            if (e.ListChangedType == System.ComponentModel.ListChangedType.Reset)
                AutoSize();
        };
        grid.Disposed += (_, _) => Dispose();
    }

    /// <summary>
    /// Zapamatane stlpce tabulky.
    /// </summary>
    /// <param name="grid">tabulka (stlpce z navrhu, nazvy stlpcov su kluce v konfiguracii)</param>
    /// <param name="id">identifikator tabulky v konfiguracii</param>
    /// <param name="config">aktualna konfiguracia programu (po ulozeni nastaveni je to iny objekt)</param>
    /// <param name="save">zapise konfiguraciu do suboru</param>
    /// <param name="hiddenByDefault">nazvy stlpcov predvolene skrytych</param>
    public static GridColumns Attach(DataGridView grid, string id, Func<ConfigBase> config, Action save, params string[] hiddenByDefault)
    {
        ArgumentNullException.ThrowIfNull(grid);
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(save);
        return new GridColumns(grid, id, config, save, hiddenByDefault);
    }

    /// <inheritdoc />
    public void Dispose() => _menu.Dispose();

    /// <summary>
    /// Stlpce sa vratili na predvolene (sirky podla obsahu treba prepocitat).
    /// </summary>
    public event EventHandler? Reset;

    /// <summary>
    /// Stlpec ma sirku nastavenu pouzivatelom - program ju nemeni.
    /// </summary>
    public bool HasUserWidth(DataGridViewColumn column) => _userWidths.Contains(column);

    /// <summary>
    /// Stlpce s automatickou sirkou z navrhu dostanu sirku podla obsahu (okrem sirky od pouzivatela a stlpca,
    /// ktory vyplna zvysok tabulky). Potom ich pouzivatel moze rozsirit ci zuzit.
    /// </summary>
    public void AutoSize()
    {
        foreach (var (column, defaults) in _defaults)
        {
            if (_userWidths.Contains(column) || column.AutoSizeMode == DataGridViewAutoSizeColumnMode.Fill || !IsAuto(defaults.Mode))
                continue;

            var width = column.GetPreferredWidth(defaults.Mode, true);
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            column.Width = Math.Max(column.MinimumWidth, width);
        }
    }

    private static bool IsAuto(DataGridViewAutoSizeColumnMode mode) => mode is DataGridViewAutoSizeColumnMode.AllCells
        or DataGridViewAutoSizeColumnMode.AllCellsExceptHeader or DataGridViewAutoSizeColumnMode.DisplayedCells
        or DataGridViewAutoSizeColumnMode.DisplayedCellsExceptHeader or DataGridViewAutoSizeColumnMode.ColumnHeader;

    private void Apply(GridLayout? layout)
    {
        if (layout is not null)
        {
            var columns = _grid.Columns.Cast<DataGridViewColumn>().ToDictionary(c => c.Name);
            var saved = layout.Columns.Where(c => columns.ContainsKey(c.Name)).OrderBy(c => c.Order).ToList();
            foreach (var state in saved)
            {
                var column = columns[state.Name];
                column.Visible = state.Visible;
                if (state.Width <= 0)
                    continue;

                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                column.Width = Math.Max(column.MinimumWidth, _grid.LogicalToDeviceUnits(state.Width));
                _userWidths.Add(column);
            }

            foreach (var state in saved)
                columns[state.Name].DisplayIndex = Math.Min(state.Order, _grid.Columns.Count - 1);

            // aspon jeden stlpec musi byt vidno
            if (!columns.Values.Any(c => c.Visible))
                foreach (var (column, defaults) in _defaults)
                    column.Visible = defaults.Visible;
        }

        _grid.FillLastVisibleColumn();
    }

    private GridLayout Capture() => new()
    {
        Id = _id,
        Columns = _grid.Columns.Cast<DataGridViewColumn>().Select(c => new GridColumnLayout
        {
            Name = c.Name,
            Order = c.DisplayIndex,
            Visible = c.Visible,
            Width = _userWidths.Contains(c) && c.AutoSizeMode != DataGridViewAutoSizeColumnMode.Fill ? c.Width * 96 / _grid.DeviceDpi : 0
        }).ToList()
    };

    private void Save()
    {
        var layout = Capture();
        var config = _config();
        if (layout.SameAs(config.GridOf(_id)))
            return;

        config.SetGrid(_id, layout);
        _save();
    }

    #region Tahanie hlavicky

    private void Grid_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left || _grid.HitTest(e.X, e.Y).Type != DataGridViewHitTestType.ColumnHeader)
            return;

        _headerDown = true;
        // nad okrajom hlavicky ma tabulka kurzor zmeny sirky
        _resizing = _grid.Cursor == Cursors.SizeWE;
    }

    private void Grid_ColumnWidthChanged(object? sender, DataGridViewColumnEventArgs e)
    {
        if (_resizing && e.Column.AutoSizeMode != DataGridViewAutoSizeColumnMode.Fill)
            _userWidths.Add(e.Column);
    }

    private void Grid_MouseUp(object? sender, MouseEventArgs e)
    {
        if (!_headerDown)
            return;

        _headerDown = _resizing = false;
        // presun stlpca moze zmenit posledny viditelny
        _grid.FillLastVisibleColumn();
        Save();
    }

    private void Grid_ColumnDividerDoubleClick(object? sender, DataGridViewColumnDividerDoubleClickEventArgs e)
    {
        var column = _grid.Columns[e.ColumnIndex];
        if (column.AutoSizeMode == DataGridViewAutoSizeColumnMode.Fill)
            return;

        // dvojklik na okraj nastavi sirku podla obsahu az po tejto udalosti
        _userWidths.Add(column);
        _grid.BeginInvoke(Save);
    }

    #endregion

    #region Ponuka na hlavicke

    private void Menu_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        foreach (var item in _menu.Items.Cast<ToolStripItem>().ToList())
            item.Dispose();
        _menu.Items.Clear();

        var columns = _grid.Columns.Cast<DataGridViewColumn>().OrderBy(c => c.DisplayIndex).ToList();
        var visibleCount = columns.Count(c => c.Visible);
        foreach (var column in columns)
        {
            var item = new ToolStripMenuItem(Title(column))
            {
                Checked = column.Visible,
                // posledny viditelny stlpec sa skryt neda
                Enabled = !(column.Visible && visibleCount == 1)
            };
            item.Click += (_, _) => ToggleVisible(column);
            _menu.Items.Add(item);
        }

        _menu.Items.Add(new ToolStripSeparator());
        var reset = new ToolStripMenuItem(Resources.Grid_ResetColumns);
        reset.Click += (_, _) => ResetColumns();
        _menu.Items.Add(reset);

        if (GlobSettings.UsingStyle is { } style)
            FormUtils.ChangeColorContextMenu(style, _menu);
        _menu.Font = GlobSettings.Fonts.Menus;
    }

    private static string Title(DataGridViewColumn column) =>
        !string.IsNullOrWhiteSpace(column.HeaderText) ? column.HeaderText
        : !string.IsNullOrWhiteSpace(column.ToolTipText) ? column.ToolTipText : column.Name;

    private void ToggleVisible(DataGridViewColumn column)
    {
        column.Visible = !column.Visible;
        _grid.FillLastVisibleColumn();
        Save();
    }

    private void ResetColumns()
    {
        _userWidths.Clear();
        foreach (var (column, defaults) in _defaults.OrderBy(d => d.Value.Index))
        {
            column.Visible = defaults.Visible;
            column.AutoSizeMode = defaults.Mode;
            if (defaults.Mode == DataGridViewAutoSizeColumnMode.None)
                column.Width = defaults.Width;
            column.DisplayIndex = defaults.Index;
        }

        _grid.FillLastVisibleColumn();
        AutoSize();
        _config().SetGrid(_id, null);
        _save();
        Reset?.Invoke(this, EventArgs.Empty);
    }

    #endregion
}
