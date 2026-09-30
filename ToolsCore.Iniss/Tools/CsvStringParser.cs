namespace ToolsCore.Iniss.Tools;

/// <summary>
/// Class to read data from a CSV string.
/// </summary>
public class CsvStringReader : TableFileReader
{
    /// <summary>
    /// Vytvori novu instanciu triedy <see cref="CsvStringReader"/>.
    /// </summary>
    /// <param name="text">Text v tvare .CSV suboru.</param>
    /// <param name="linesep">Separator riadkov.</param>
    /// <param name="rowsep">Separator buniek.</param>
    public CsvStringReader(string text, char linesep = '\n', char rowsep = ';')
    {
        text = text.Replace("\r", "");
        var rows = text.Split([linesep], StringSplitOptions.RemoveEmptyEntries);

        RowCount = rows.Length;

        foreach (var row in rows)
        {
            var count = row.Split(rowsep).Length;
            if (ColumnCount < count) ColumnCount = count;
        }

        Data = new string[RowCount, ColumnCount];

        for (var i = 0; i < rows.Length; i++)
        {
            var pos = 0;
            var col = 0;

            while (pos < rows[i].Length)
            {
                string value;

                // Special handling for quoted field
                if (rows[i][pos] == '"')
                {
                    // Skip initial quote
                    pos++;

                    // Parse quoted value
                    var start = pos;
                    while (pos < rows[i].Length)
                    {
                        // Test for quote character
                        if (rows[i][pos] == '"')
                        {
                            // Found one
                            pos++;

                            // If two quotes together, keep one
                            // Otherwise, indicates end of value
                            if (pos >= rows[i].Length || rows[i][pos] != '"')
                            {
                                pos--;
                                break;
                            }
                        }

                        pos++;
                    }

                    value = rows[i].Substring(start, pos - start);
                    value = value.Replace("\"\"", "\"");
                }
                else
                {
                    // Parse unquoted value
                    var start = pos;
                    while (pos < rows[i].Length && rows[i][pos] != rowsep)
                        pos++;
                    value = rows[i].Substring(start, pos - start);
                }

                // Add field to list
                Data[i, col] = value;

                col++;

                // Eat up to and including next comma
                while (pos < rows[i].Length && rows[i][pos] != rowsep)
                    pos++;
                if (pos < rows[i].Length)
                    pos++;
            }
        }
    }
}
