namespace ToolsCore.Iniss.Tools;

/// <summary>
/// Class to read data from a CSV string.
/// </summary>
public class CsvStringReader : TableFileReader
{
    /// <summary>
    /// Oddelovac buniek podla prveho riadku: tabulator (kopia z Excelu), inak bodkociarka, inak ciarka.
    /// </summary>
    /// <param name="text">Text v tvare .CSV suboru.</param>
    public static char DetectSeparator(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var end = text.IndexOf('\n', StringComparison.Ordinal);
        var firstLine = end == -1 ? text : text[..end];
        if (firstLine.Contains('\t', StringComparison.Ordinal)) return '\t';
        return firstLine.Contains(';', StringComparison.Ordinal) || !firstLine.Contains(',', StringComparison.Ordinal) ? ';' : ',';
    }

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

        // pocet stlpcov az z rozlozenych hodnot - oddelovac v uvodzovkach stlpec nepridava
        var values = new List<string>[rows.Length];

        for (var i = 0; i < rows.Length; i++)
        {
            var pos = 0;
            values[i] = [];

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
                values[i].Add(value);

                // Eat up to and including next comma
                while (pos < rows[i].Length && rows[i][pos] != rowsep)
                    pos++;
                if (pos < rows[i].Length)
                {
                    pos++;

                    // oddelovac na konci riadku - posledna bunka je prazdna
                    if (pos == rows[i].Length)
                        values[i].Add("");
                }
            }
        }

        RowCount = rows.Length;
        ColumnCount = values.Length == 0 ? 0 : values.Max(v => v.Count);
        Data = new string[RowCount, ColumnCount];
        for (var i = 0; i < RowCount; i++)
            for (var j = 0; j < ColumnCount; j++)
                Data[i, j] = j < values[i].Count ? values[i][j] : "";
    }
}
