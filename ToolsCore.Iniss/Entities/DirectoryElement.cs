namespace ToolsCore.Iniss.Entities;

public class DirectoryElement : FileSystemElement
{
    public DirectoryElement(DirectoryInfo dirinfo) : base(dirinfo.Name)
    {
        DirInfo = dirinfo;
        var fis = dirinfo.GetFileSystemInfos();
        Children = new List<FileSystemElement>(fis.Length);
        foreach (FileSystemElement element in fis)
        {
            Children.Add(element);
            element.Parent = this;
        }
    }

    public DirectoryElement(string absPath) : this(new DirectoryInfo(absPath))
    {
    }

    public DirectoryInfo DirInfo { get; set; }

    public List<FileSystemElement> Children { get; }

    public FyzGroup Group { get; set; } = null!;
}
