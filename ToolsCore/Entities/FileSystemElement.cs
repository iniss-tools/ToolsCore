using System.Globalization;
using ToolsCore.Tools;

namespace ToolsCore.Entities;

public abstract class FileSystemElement
{
    protected FileSystemElement(string name) => Name = name;

    public string Name { get; set; }

    public DirectoryElement? Parent { get; set; }

    public static implicit operator FileSystemElement(FileSystemInfo fsi)
    {
        switch (fsi)
        {
            case DirectoryInfo di:
                return new DirectoryElement(di);
            case FileInfo fi:
                if (fi.Extension.ToUpper(CultureInfo.CurrentCulture) is SoundFileElement.WAVExt or SoundFileElement.EWAExt)
                    return new SoundFileElement(fi);
                return new OtherFileElement(fi);
            default:
                return null!;
        }
    }

    public override string ToString() => Name;
}

public class BackButtonElement : FileSystemElement
{
    public BackButtonElement(DirectoryElement parent) : base("...")
    {
        Parent = parent;
    }
}

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

public abstract class FileElement : FileSystemElement
{
    protected FileElement(FileInfo fileinfo) : base(fileinfo.Name) => FileInfo = fileinfo;

    protected FileElement(string absPath) : base(Path.GetFileName(absPath)) => FileInfo = new FileInfo(absPath);

    public FileInfo FileInfo { get; set; }
}

public class SoundFileElement : FileElement
{
    public const string WAVExt = ".WAV";
    public const string EWAExt = ".EWA";

    public SoundFileElement(FileInfo fileinfo) : base(fileinfo) => Duration = -1;

    public SoundFileElement(string file) : base(file) => Duration = -1;

    public int Duration { get; set; }

    public string DurationText => Utils.LengthIntToString(Duration);

    public FyzSound Sound { get; set; } = null!;
}

public class OtherFileElement : FileElement
{
    public OtherFileElement(FileInfo fileinfo) : base(fileinfo)
    {
    }

    public OtherFileElement(string file) : base(file)
    {
    }
}