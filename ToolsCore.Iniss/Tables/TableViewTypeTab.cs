using System.Collections;

namespace ToolsCore.Iniss.Tables;

/// <summary>
/// Definuje typ, mod zobrazenia a pocet riadkov na zaznam katalogovej tabuli.
/// </summary>
public sealed class TableViewTypeTab : IEnumerable<TableTypeModeItem>
{
    /// <summary>
    /// Konstruktor
    /// </summary>
    public TableViewTypeTab()
    {
        TypeModeItems = [];
    }

    /// <summary>
    /// Typ zobrazenia.
    /// </summary>
    public TableViewType ViewType { get; set; } = null!;

    /// <summary>
    /// Pocet riadkov, kolko ma 1 zaznam na tabuli.
    /// </summary>
    public string CountLinesRecord { get; set; } = null!;

    /// <summary>
    /// Mody zobrazenia zaznamu.
    /// </summary>
    public List<TableTypeModeItem> TypeModeItems { get; }

    /// <summary>Returns an enumerator that iterates through a collection.</summary>
    /// <returns>An <see cref="System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
    public IEnumerator<TableTypeModeItem> GetEnumerator() => TypeModeItems.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}