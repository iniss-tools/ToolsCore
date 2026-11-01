using Microsoft.VisualBasic.FileIO;
using Serilog;
using ToolsCore.Iniss.Tools;
using Vanara.Windows.Shell;
using SearchOption = System.IO.SearchOption;

namespace ToolsCore.Tools;

/// <summary>
/// Subory, priecinky a kos.
/// </summary>
public static partial class Utils
{
    /// <summary>
    /// Kopiruje obsah priecinka, ak je aspon 1 z parametov <see cref="string.Empty"/> alebo <see langword="null"/>, nic sa nevykona.
    /// </summary>
    /// <param name="sourcePath">zdrojovy priecinok</param>
    /// <param name="destinationPath">cielovy priecinok</param>
    public static void CopyDirectory(string sourcePath, string destinationPath)
    {
        if (string.IsNullOrEmpty(sourcePath) || string.IsNullOrEmpty(destinationPath)) return;

        // cielovy priecinok aj ked zdroj nema podpriecinky
        Directory.CreateDirectory(destinationPath);
        foreach (var dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(Path.Combine(destinationPath, Path.GetRelativePath(sourcePath, dirPath)));

        // subory s rovnakym nazvom sa prepisu
        foreach (var file in Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories))
            File.Copy(file, Path.Combine(destinationPath, Path.GetRelativePath(sourcePath, file)), true);
    }

    /// <summary>
    /// Vrati nazov priecinka.
    /// </summary>
    /// <returns>nazov priecinka alebo <see langword="null"/> ak je vstup <see langword="null"/> alebo <see cref="string.Empty"/>.</returns>
    public static string? GetDirectoryName(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        var dir = new DirectoryInfo(path);
        return dir.Name;
    }

    /// <summary>
    /// Vytvori relativnu cestu k suboru <paramref name="filePath"/> podla absolutnej cesty <paramref name="folderPath"/>.
    /// </summary>
    /// <param name="filePath">Absolutna cesta k suboru.</param>
    /// <param name="folderPath">Cesta k priecinku, od ktoreho sa bude brat cesta k suboru ako relativna.</param>
    /// <returns>relativnu cestu k suboru.</returns>
    public static string GetRelativePath(string filePath, string folderPath)
    {
        var pathUri = new Uri(filePath);
        // Folders must end in a slash
        if (!folderPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
        {
            folderPath += Path.DirectorySeparatorChar;
        }
        var folderUri = new Uri(folderPath);
        return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
    }

    /// <summary>
    /// Odstrani subor a premiestni ho do kosa (recycle bin).
    /// </summary>
    /// <param name="path">cesta k suboru</param>
    /// /// <param name="allDialogs"></param>
    public static void DeleteFileToRecycleBin(string path, bool allDialogs = false)
    {
        FileSystem.DeleteFile(path, allDialogs ? UIOption.AllDialogs : UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
        Log.Information("Subor {Path} presunuty do Kosa", path);
    }

    /// <summary>
    /// Odstrani priecinok a premiestni ho do kosa (recycle bin).
    /// </summary>
    /// <param name="path">cesta k suboru</param>
    /// <param name="allDialogs"></param>
    public static void DeleteDirectoryToRecycleBin(string path, bool allDialogs = false)
    {
        FileSystem.DeleteDirectory(path, allDialogs ? UIOption.AllDialogs : UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
        Log.Information("Priecinok {Path} presunuty do Kosa", path);
    }

    /// <summary>
    /// Pokusi sa obnovit subor/priecinok, ktory bol premiestneneny do kosa (recycle bin).
    /// </summary>
    /// <param name="fullPath">povodna cesta k suboru</param>
    /// <returns>ci sa podarilo obnovit subor/priecinok</returns>
    public static bool TryRecoverFileOrDirFromBin(string fullPath)
    {
        var item = RecycleBin.GetItemFromOriginalPath(fullPath);
        if (item is null)
        {
            Log.Warning("Polozka {Path} sa v Kosi nenasla - neda sa obnovit", fullPath);
            return false;
        }

        RecycleBin.Restore(item, true);
        Log.Information("Polozka {Path} obnovena z Kosa", fullPath);
        return true;
    }

    /// <summary>
    /// Zisti, ci zadany nazov suboru/priecinka je platny.
    /// </summary>
    /// <param name="fullPath"></param>
    /// <param name="fileName"></param>
    /// <param name="checkIfExists"></param>
    /// <returns></returns>
    public static bool IsFileNameCorrect(string fullPath, string fileName, bool checkIfExists = true)
    {
        return !string.IsNullOrWhiteSpace(fileName) &&
               fileName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0 &&
               (!checkIfExists || !File.Exists(PathUtils.CombinePath(fullPath, fileName)));
    }
}
