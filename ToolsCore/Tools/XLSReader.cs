using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Excel;
using Application = Microsoft.Office.Interop.Excel.Application;

namespace ToolsCore.Tools;

/// <summary>
///     Nacitava udaje zo .XLS a .XLSX suborov.
/// </summary>
public class XlsReader : TableFileReader
{
    private readonly Application _excelApp;
    private readonly Workbook _workbook;
    private readonly Worksheet _worksheet;

    /// <summary>
    ///     Vytvori novu instanciu triedy <see cref="XlsReader"/>.
    /// </summary>
    /// <param name="fileName">Cesta k suboru.</param>
    /// <param name="worksheetID">Identifikator sheetu.</param>
    public XlsReader(string fileName, int worksheetID = 1)
    {
        _excelApp = new Application();
        _workbook = _excelApp.Workbooks.Open(fileName, 0, true, 5, "", "", true, XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
        _worksheet = (Worksheet)_workbook.Worksheets[worksheetID];

        ReadWorksheet();
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        _workbook.Close();
        _excelApp.Quit();
        Marshal.ReleaseComObject(_worksheet);
        Marshal.ReleaseComObject(_workbook);
        Marshal.ReleaseComObject(_excelApp);
    }

    private void ReadWorksheet()
    {
        var range = _worksheet.UsedRange;
        RowCount = range.Rows.Count;
        ColumnCount = range.Columns.Count;

        Data = new string[RowCount, ColumnCount];

        for (var r = 1; r <= RowCount; r++)
        {
            for (var c = 1; c <= ColumnCount; c++)
            {
                Data[r - 1, c - 1] = (range.Cells[r, c] as Microsoft.Office.Interop.Excel.Range)?.Value2.ToString() ?? "";
            }
        }
    }
}