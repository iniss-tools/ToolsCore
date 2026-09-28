namespace ToolsCore.Tools;

/// <summary>
///     Trieda pre citanie dat z textoveho suboru.
/// </summary>
public class CsvFileReader : StreamReader
{
    /// <inheritdoc />
    public CsvFileReader(string filename) : base(filename, Encodings.Win1250)
    {
    }

    /// <summary>
    ///     Precita jeden riadok zo suboru.
    /// </summary>
    /// <param name="row"></param>
    /// <returns></returns>
    public ReadStartChar ReadRow(CsvRow row)
    {
        row.LineText = ReadLine();

        if (row.LineText == null) 
            return ReadStartChar.Eof;
        if (row.LineText == "" || string.IsNullOrWhiteSpace(row.LineText)) 
            return ReadStartChar.Empty;
        if (row.LineText.StartsWith(';')) 
            return ReadStartChar.Semicolon;
        if (row.LineText.StartsWith('/')) 
            return ReadStartChar.Slash;

        var pos = 0;
        var rows = 0;

        while (pos < row.LineText.Length)
        {
            string value;

            // Special handling for quoted field
            if (row.LineText[pos] == '"')
            {
                // Skip initial quote
                pos++;

                // Parse quoted value
                var start = pos;
                while (pos < row.LineText.Length)
                {
                    // Test for quote character
                    if (row.LineText[pos] == '"')
                    {
                        // Found one
                        pos++;

                        // If two quotes together, keep one
                        // Otherwise, indicates end of value
                        if (pos >= row.LineText.Length || row.LineText[pos] != '"')
                        {
                            pos--;
                            break;
                        }
                    }

                    pos++;
                }

                value = row.LineText.Substring(start, pos - start);
                value = value.Replace("\"\"", "\"");

                // Add field to list
                if (rows < row.Count)
                    row[rows] = value;
                else
                    row.Add(value);
                rows++;
            }
            else
            {
                // Parse unquoted value
                var start = pos;
                while (pos < row.LineText.Length && row.LineText[pos] != ',')
                    pos++;
                value = row.LineText.Substring(start, pos - start);

                // Add field to list
                if (rows < row.Count)
                    row[rows] = value.Trim();
                else
                    row.Add(value.Trim());
                rows++;
            }

            // Eat up to and including next comma
            while (pos < row.LineText.Length && row.LineText[pos] != ',')
                pos++;
            if (pos < row.LineText.Length)
                pos++;
        }

        // Delete any unused items
        while (row.Count > rows)
            row.RemoveAt(rows);

        return ReadStartChar.NonEmpty;
    }
}
