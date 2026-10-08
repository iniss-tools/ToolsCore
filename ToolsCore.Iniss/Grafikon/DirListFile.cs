using System.Drawing;
using System.Globalization;
using ToolsCore.Iniss.Properties;
using ToolsCore.Iniss.Tools;
using static ToolsCore.Iniss.Grafikon.GvdFileConsts;
using static ToolsCore.Iniss.Tools.ParseUtils;
using static ToolsCore.Iniss.Tools.PathUtils;

namespace ToolsCore.Iniss.Grafikon;

/// <summary>
/// Zoznam grafikonov instalacie (DirList.txt).
/// </summary>
public static class DirListFile
{
    /// <summary>
    /// Vrati zoznam pouzivanych priecinkov s GVD.
    /// </summary>
    /// <param name="dataDir">priecinok DATA instalacie INISS</param>
    /// <returns>priecinky s GVD</returns>
    public static List<DirList> Read(string dataDir)
    {
        var fileDirList = CombinePath(dataDir, FileDirlist)!;

        var dirs = new List<DirList>();

        // bez DirList.TXT berie INISS ako jediny GVD samotny priecinok DATA (starsi zapis s jednym grafikonom)
        if (!File.Exists(fileDirList))
        {
            if (File.Exists(CombinePath(dataDir, FileGrafikon)))
                dirs.Add(new DirList { DirName = "", FullPath = dataDir });
            return dirs;
        }

        using var dirlistF = new CsvFileReader(fileDirList);
        var riadok = 1;
        var row = new CsvRow();
        while (true)
        {
            var status = dirlistF.ReadRow(row);
            if (LineIsEmpty(status))
            {
                riadok++;
                continue;
            }

            if (LineIsEof(status))
                break;

            try
            {
                var dirList = new DirList();
                var dirName = row[0];
                dirList.DirName = dirName;
                dirList.FullPath = dataDir + Path.DirectorySeparatorChar + dirName;

                dirList.TablePort = ParseIntOrNull(row.ElementAtOrDefault(1));
                dirList.ReportPort = ParseIntOrNull(row.ElementAtOrDefault(2));
                dirList.Flags = row.ElementAtOrDefault(3);
                dirList.BackColor = ParseColor(row.ElementAtOrDefault(4));

                dirs.Add(dirList);
            }
            catch (Exception e)
            {
                throw new FormatException(string.Format(CultureInfo.InvariantCulture, Resources.FormatCommon_Error, FileDirlist, riadok) + e.Message, e);
            }

            riadok++;
        }

        return dirs;
    }

    /// <summary>
    /// Zapise zoznam pouzivanych priecinkoch s GVD.
    /// </summary>
    /// <param name="dataDir">priecinok DATA instalacie INISS</param>
    /// <param name="dirs">priecinky s GVD</param>
    public static void Write(string dataDir, IEnumerable<DirList> dirs)
    {
        WriteFile(CombinePath(dataDir, FileDirlist)!, dirs);
    }

    /// <summary>
    /// Zapise zoznam priecinkov s GVD do daneho suboru. Subor nezalozi ani neprepise, ked by v nom neostal
    /// ziadny riadok a zaroven existuje grafikon priamo v DATA alebo subor este neexistuje.
    /// </summary>
    /// <param name="fileDirList">cesta k DirList.TXT</param>
    /// <param name="dirs">priecinky s GVD</param>
    /// <returns>true, ak sa subor zapisal</returns>
    public static bool WriteFile(string fileDirList, IEnumerable<DirList> dirs)
    {
        var all = dirs.ToList();
        // grafikon priamo v DATA sa v DirList.TXT zapisat neda - INISS ho vidi len vtedy, ked sa subor neda otvorit.
        // GVDEditor taky grafikon pri otvoreni ponuka presunut do vlastneho priecinka
        var toWrite = all.Where(d => !d.IsDataRoot).ToList();
        var hasDataRoot = toWrite.Count != all.Count;

        if (toWrite.Count == 0 && (hasDataRoot || !File.Exists(fileDirList)))
            return false;

        if (hasDataRoot)
            Log.Warning(Resources.DirList_RootGrafikon);

        using var dirlistF = new CsvFileWriter(fileDirList);
        foreach (var dir in toWrite)
        {
            var row = new CsvRow();
            row.Insert(0, Field(dir.DirName));
            if (dir.TablePort.HasValue && dir.TablePort != 0)
                row.Insert(1, dir.TablePort.Value.ToString(CultureInfo.InvariantCulture));
            else
                row.Insert(1, "");

            if (dir.ReportPort.HasValue && dir.ReportPort != 0)
                row.Insert(2, dir.ReportPort.Value.ToString(CultureInfo.InvariantCulture));
            else
                row.Insert(2, "");

            row.Insert(3, Field(dir.Flags ?? ""));
            row.Insert(4, dir.BackColor.HasValue ? ColorText(dir.BackColor.Value) : "");

            dirlistF.WriteRow(row);
        }

        return true;
    }

    // INISS cita hodnotu s ciarkou len v uvodzovkach - napr. rucne zapisane priznaky "Z, K"
    private static string Field(string value) => value.Contains(',', StringComparison.Ordinal) ? value.Quote() : value;

    /// <summary>Farba z textu 0xBBGGRR (poradie modra, zelena, cervena ako COLORREF vo Windows) alebo null.</summary>
    public static Color? ParseColor(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var hex = text.Trim();
        if (hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) hex = hex[2..];
        else if (hex.StartsWith('#')) hex = hex[1..];
        // kratky zapis BGR po jednej cifre
        if (hex.Length == 3) hex = string.Concat(hex.Select(c => new string(c, 2)));
        if (hex.Length != 6 || !int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var value)) return null;
        return Color.FromArgb(value & 0xFF, (value >> 8) & 0xFF, (value >> 16) & 0xFF);
    }

    /// <summary>Farba ako text 0xBBGGRR.</summary>
    public static string ColorText(Color color) =>
        "0x" + color.B.ToString("X2", CultureInfo.InvariantCulture) + color.G.ToString("X2", CultureInfo.InvariantCulture) + color.R.ToString("X2", CultureInfo.InvariantCulture);
}
