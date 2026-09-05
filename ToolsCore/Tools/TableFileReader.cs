namespace ToolsCore.Tools;

/// <summary>
///     Podkladova trieda pre spracovanie tabuliek hodnot (stlpce a riadky).
/// </summary>
public abstract class TableFileReader : IDisposable
{
    /// <summary>
    ///     Data.
    /// </summary>
    protected string[,] Data = null!;

    /// <summary>
    ///     Pocet riadkov.
    /// </summary>
    public int RowCount { get; protected set; }

    /// <summary>
    ///     Pocet stlpcov.
    /// </summary>
    public int ColumnCount { get; protected set; }

    /// <summary>
    ///     Vrati prvok na specifikovanom riadku a stlpci.
    /// </summary>
    /// <param name="row">Riadok tabulky.</param>
    /// <param name="column">Stlpec tabulky.</param>
    /// <exception cref="IndexOutOfRangeException">Ak zadany index riadku alebo stlpca je vacsi ako pocet riadkov resp. stlpcov.</exception>
    public string this[int row, int column]
    {
        get
        {
            if (column >= ColumnCount)
                throw new IndexOutOfRangeException($"Column index {column} is out out of range. Column count = {ColumnCount}.");

            if (row >= RowCount)
                throw new IndexOutOfRangeException($"Row index {row} is out out of range. Row count = {RowCount}.");

            return Data[row, column];
        }
    }

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public abstract void Dispose();
}