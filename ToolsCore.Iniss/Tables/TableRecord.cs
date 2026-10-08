using System.Collections;

namespace ToolsCore.Iniss.Tables;

/// <summary>
/// Reprezentuje zaznam tabule.
/// </summary>
public sealed class TableRecord : IEnumerable<TablePosition>
{
    /// <summary>
    /// Pozicie zaznamu tabule.
    /// </summary>
    public List<TablePosition> Positions { get; set; } = [];

    /// <summary>Returns an enumerator that iterates through a collection.</summary>
    /// <returns>An <see cref="System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
    public IEnumerator<TablePosition> GetEnumerator() => Positions.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}