namespace ToolsCore.Tools;

/// <summary>
///     Trieda pre ulozenie CSV riadku.
/// </summary>
public class CsvRow : List<string>
{
    /// <inheritdoc />
    public CsvRow()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CsvRow" /> class that is empty and
    ///     has the default initial capacity.
    /// </summary>
    public CsvRow(int initCount) : base(initCount)
    {
    }

    /// <summary>
    ///     Nespracovany text riadku.
    /// </summary>
    public string? LineText { get; set; }
}
