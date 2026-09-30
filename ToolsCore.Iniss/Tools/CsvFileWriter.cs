namespace ToolsCore.Iniss.Tools;

/// <summary>
/// Trieda pre ulozenie dat do textoveho suboru.
/// </summary>
public class CsvFileWriter : StreamWriter
{
    /// <inheritdoc />
    public CsvFileWriter(string filename) : base(filename, false, Encodings.Win1250)
    {
    }

    /// <summary>
    /// Zapise jeden riadok do suboru.
    /// </summary>
    /// <param name="row">Riadok k zapisaniu.</param>
    public void WriteRow(CsvRow row)
    {
        var builder = new StringBuilder();
        var firstColumn = true;
        foreach (var value in row)
        {
            // Add separator if this isn't the first value
            if (!firstColumn)
                builder.Append(',');

            builder.Append(value);
            firstColumn = false;
        }

        row.LineText = builder.ToString();
        WriteLine(row.LineText);
    }

    /// <summary>
    /// Zapise komentar do suboru.
    /// </summary>
    /// <param name="row">Komentar k zapisaniu.</param>
    /// <param name="commentIndicator">Indikator komentaru, ktory sa zadava do suboru pred komentar.</param>
    public void WriteComment(string row, char commentIndicator = ';')
    {
        WriteLine(commentIndicator + row);
    }
}
