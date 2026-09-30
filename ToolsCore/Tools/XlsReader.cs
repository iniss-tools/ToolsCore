using System.Globalization;
using ExcelDataReader;
using ToolsCore.Iniss.Tools;

namespace ToolsCore.Tools;

/// <summary>
/// Nacitava udaje zo .XLS a .XLSX suborov (ExcelDataReader - bez nainstalovaneho Excelu).
/// </summary>
public class XlsReader : TableFileReader
{
    /// <summary>
    /// Vytvori novu instanciu triedy <see cref="XlsReader"/> a nacita obsah harku.
    /// </summary>
    /// <param name="fileName">Cesta k suboru.</param>
    /// <param name="worksheetID">Poradie harku (od 1).</param>
    public XlsReader(string fileName, int worksheetID = 1)
    {
        // stary format .xls (BIFF) pouziva kodove stranky, ktore .NET bez registracie nepozna
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        // subor moze byt otvoreny v Exceli - citanie ho nesmie zamknut ani zlyhat na zdielani
        using var stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = ExcelReaderFactory.CreateReader(stream);
        for (var i = 1; i < worksheetID; i++)
        {
            if (!reader.NextResult())
                throw new ArgumentOutOfRangeException(nameof(worksheetID), worksheetID, "Sheet does not exist.");
        }

        var rows = new List<string[]>();
        while (reader.Read())
        {
            var row = new string[reader.FieldCount];
            for (var c = 0; c < row.Length; c++)
                row[c] = CellText(reader.GetValue(c));
            rows.Add(row);
        }

        (Data, RowCount, ColumnCount) = UsedRange(rows);
    }

    /// <summary>
    /// Text bunky. Cisla a logicke hodnoty ako predtym cez Excel (<c>Value2</c>); datum a cas v tvare, ktory citaju
    /// importy (<c>HH:mm</c>, <c>dd.MM.yyyy</c>).
    /// </summary>
    internal static string CellText(object? value) => value switch
    {
        null => "",
        string s => s,
        double d => d.ToString(CultureInfo.CurrentCulture),
        // bunka len s casom ma v Exceli datum 30.12.1899 (nula dni)
        DateTime t when t.Date < new DateTime(1900, 1, 1) => t.ToString("HH:mm", CultureInfo.InvariantCulture),
        DateTime { TimeOfDay.Ticks: 0 } t => t.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture),
        DateTime t => t.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture),
        TimeSpan t => t.ToString(@"hh\:mm", CultureInfo.InvariantCulture),
        _ => Convert.ToString(value, CultureInfo.CurrentCulture) ?? ""
    };

    /// <summary>
    /// Vyreze obdlznik od prvej po poslednu neprazdnu bunku - rovnako ako <c>UsedRange</c> v Exceli.
    /// </summary>
    internal static (string[,] Data, int Rows, int Columns) UsedRange(IReadOnlyList<string[]> rows)
    {
        int top = int.MaxValue, left = int.MaxValue, bottom = -1, right = -1;
        for (var r = 0; r < rows.Count; r++)
        {
            for (var c = 0; c < rows[r].Length; c++)
            {
                if (rows[r][c].Length == 0)
                    continue;
                top = Math.Min(top, r);
                bottom = Math.Max(bottom, r);
                left = Math.Min(left, c);
                right = Math.Max(right, c);
            }
        }

        if (bottom < 0)
            return (new string[0, 0], 0, 0);

        var data = new string[bottom - top + 1, right - left + 1];
        for (var r = top; r <= bottom; r++)
            for (var c = left; c <= right; c++)
                data[r - top, c - left] = c < rows[r].Length ? rows[r][c] : "";

        return (data, bottom - top + 1, right - left + 1);
    }
}
