namespace ToolsCore.Iniss.Tools;

/// <summary>
/// Praca s cestami k suborom a priecinkom INISS.
/// </summary>
public static class PathUtils
{
    /// <summary>
    /// Skombinuje cestu k suborom/priecinkom.
    /// </summary>
    /// <param name="paths">pole retazcov s cestami k suborom/priecinkom.</param>
    /// <returns>skombinovanú cestu.</returns>
    public static string? CombinePath(params string[] paths)
    {
        if (paths.Length == 0) return null;

        if (paths[0].Length == 0) return paths[0];

        for (var i = 1; i < paths.Length; i++)
        {
            paths[i] = paths[i].TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        return Path.Combine(paths);
    }
}
