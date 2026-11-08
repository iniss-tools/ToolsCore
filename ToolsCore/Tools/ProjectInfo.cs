namespace ToolsCore.Tools;

public class ProjectInfo
{
    public string Path { get; }
    public DateTime LastAccess { get; set; }

    public ProjectInfo(string path, DateTime lastAccess)
    {
        Path = path;
        LastAccess = lastAccess;
    }

    public override bool Equals(object? obj)
    {
        return obj is ProjectInfo pi && Equals(pi);
    }

    private bool Equals(ProjectInfo other)
    {
        return Path == other.Path;
    }

    public override int GetHashCode()
    {
        return Path != null ? Path.GetHashCode() : 0;
    }
}
