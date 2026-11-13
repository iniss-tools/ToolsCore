using ToolsCore.Iniss.Tools;

namespace ToolsCore.Tests.Tools;

/// <summary>
/// Kroky spat a znova nad nemennymi stavmi.
/// </summary>
[TestClass]
public class UndoHistoryTests
{
    private sealed record State(int Value);

    [TestMethod]
    public void Historia_SpatAZnovaVratiStavyAPopisy()
    {
        var first = new State(1);
        var history = new UndoHistory<State>(first);
        var second = new State(2);
        var third = new State(3);

        Assert.IsTrue(history.Push(second, "a"));
        Assert.IsTrue(history.Push(third, "b"));
        CollectionAssert.AreEqual(new[] { "b", "a" }, history.UndoLabels.ToList());

        Assert.AreSame(first, history.Undo(2));
        Assert.IsFalse(history.CanUndo);
        CollectionAssert.AreEqual(new[] { "a", "b" }, history.RedoLabels.ToList());

        Assert.AreSame(second, history.Redo());
        CollectionAssert.AreEqual(new[] { "a" }, history.UndoLabels.ToList());
        CollectionAssert.AreEqual(new[] { "b" }, history.RedoLabels.ToList());
    }

    [TestMethod]
    public void Historia_NovaZmenaZahodiKrokyZnova()
    {
        var history = new UndoHistory<State>(new State(1));
        history.Push(new State(2), "a");
        history.Undo();

        history.Push(new State(3), "c");

        Assert.IsFalse(history.CanRedo);
        Assert.AreEqual(3, history.Current.Value);
    }

    [TestMethod]
    public void Historia_RovnakyStavNieJeZmena()
    {
        var state = new State(1);
        var history = new UndoHistory<State>(state);

        // rovnaka hodnota, iny objekt je zmena - historia porovnava odkazy
        Assert.IsFalse(history.Push(state, "a"));
        Assert.IsTrue(history.Push(new State(1), "b"));
        CollectionAssert.AreEqual(new[] { "b" }, history.UndoLabels.ToList());
    }

    [TestMethod]
    public void Historia_KapacitaZahodiNajstarsieKroky()
    {
        var history = new UndoHistory<State>(new State(0), capacity: 2);
        for (var i = 1; i <= 3; i++)
            history.Push(new State(i), i.ToString(System.Globalization.CultureInfo.InvariantCulture));

        CollectionAssert.AreEqual(new[] { "3", "2" }, history.UndoLabels.ToList());
        Assert.AreEqual(1, history.Undo(5).Value);
    }

    [TestMethod]
    public void Historia_ResetZahodiKroky()
    {
        var history = new UndoHistory<State>(new State(0));
        history.Push(new State(1), "a");
        history.Undo();

        var state = new State(5);
        history.Reset(state);

        Assert.AreSame(state, history.Current);
        Assert.IsFalse(history.CanUndo);
        Assert.IsFalse(history.CanRedo);
    }
}
