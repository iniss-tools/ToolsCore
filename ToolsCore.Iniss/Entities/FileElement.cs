namespace ToolsCore.Iniss.Entities;

public abstract class FileElement : FileSystemElement
{
    protected FileElement(FileInfo fileinfo) : base(fileinfo.Name) => FileInfo = fileinfo;

    protected FileElement(string absPath) : base(Path.GetFileName(absPath)) => FileInfo = new FileInfo(absPath);

    public FileInfo FileInfo { get; set; }
}
