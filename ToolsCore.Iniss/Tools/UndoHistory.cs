namespace ToolsCore.Iniss.Tools;

/// <summary>
/// Kroky spat a znova nad nemennymi stavmi dokumentu: kazda zmena ulozi novy stav s popisom. Stav sa nekopiruje, preto musi byt
/// nemenny (zmena vytvori novy objekt).
/// </summary>
/// <typeparam name="T">stav dokumentu</typeparam>
public sealed class UndoHistory<T> where T : class
{
    private readonly int _capacity;
    private readonly List<(T State, string Label)> _undo = [];
    private readonly List<(T State, string Label)> _redo = [];

    /// <param name="initial">stav po otvoreni dokumentu</param>
    /// <param name="capacity">najviac krokov spat</param>
    public UndoHistory(T initial, int capacity = 50)
    {
        ArgumentNullException.ThrowIfNull(initial);
        ArgumentOutOfRangeException.ThrowIfLessThan(capacity, 1);

        Current = initial;
        _capacity = capacity;
    }

    /// <summary>Aktualny stav.</summary>
    public T Current { get; private set; }

    /// <summary>Je co vratit.</summary>
    public bool CanUndo => _undo.Count > 0;

    /// <summary>Je co zopakovat.</summary>
    public bool CanRedo => _redo.Count > 0;

    /// <summary>Popisy krokov spat od posledneho.</summary>
    public IReadOnlyList<string> UndoLabels => [.. _undo.Select(step => step.Label).Reverse()];

    /// <summary>Popisy krokov znova od najblizsieho.</summary>
    public IReadOnlyList<string> RedoLabels => [.. _redo.Select(step => step.Label).Reverse()];

    /// <summary>
    /// Zaznamena zmenu: <paramref name="state" /> sa stane aktualnym stavom a kroky znova sa zahodia. Rovnaky stav nie je zmena.
    /// </summary>
    /// <param name="state">stav po zmene</param>
    /// <param name="label">popis zmeny pre ponuku Spat</param>
    /// <returns><see langword="false" />, ak sa stav nezmenil.</returns>
    public bool Push(T state, string label)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (ReferenceEquals(state, Current))
            return false;

        _undo.Add((Current, label));
        if (_undo.Count > _capacity)
            _undo.RemoveAt(0);
        _redo.Clear();
        Current = state;
        return true;
    }

    /// <summary>
    /// Vrati <paramref name="steps" /> krokov spat.
    /// </summary>
    /// <returns>stav po navrate</returns>
    public T Undo(int steps = 1)
    {
        for (var i = 0; i < steps && _undo.Count > 0; i++)
        {
            var (state, label) = _undo[^1];
            _undo.RemoveAt(_undo.Count - 1);
            _redo.Add((Current, label));
            Current = state;
        }

        return Current;
    }

    /// <summary>
    /// Zopakuje <paramref name="steps" /> krokov.
    /// </summary>
    /// <returns>stav po zopakovani</returns>
    public T Redo(int steps = 1)
    {
        for (var i = 0; i < steps && _redo.Count > 0; i++)
        {
            var (state, label) = _redo[^1];
            _redo.RemoveAt(_redo.Count - 1);
            _undo.Add((Current, label));
            Current = state;
        }

        return Current;
    }

    /// <summary>
    /// Zacne historiu od noveho stavu (napr. po znovunacitani suborov) - kroky spat aj znova sa zahodia.
    /// </summary>
    public void Reset(T state)
    {
        ArgumentNullException.ThrowIfNull(state);
        _undo.Clear();
        _redo.Clear();
        Current = state;
    }
}
