using System.Globalization;

namespace ToolsCore.Iniss.Entities;

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
