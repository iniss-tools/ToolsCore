using System.Collections;

namespace ToolsCore.Iniss.Tables;

/// <summary>
/// </summary>
public sealed class TableTypeModeItem : IEnumerable<string>
{
    /// <summary>
    /// Konstruktor
    /// </summary>
    public TableTypeModeItem()
    {
        ItemsKeys = [];
    }

    /// <summary>
    /// Mod zobrazenia polozky.
    /// </summary>
    public TableViewMode ViewMode { get; set; } = null!;

    /// <summary>
    /// </summary>
    public List<string> ItemsKeys { get; set; }

    /// <summary>Returns an enumerator that iterates through a collection.</summary>
    /// <returns>An <see cref="System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
    public IEnumerator<string> GetEnumerator() => ItemsKeys.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}