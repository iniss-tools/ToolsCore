namespace ToolsCore.Iniss.Entities;

public class BackButtonElement : FileSystemElement
{
    public BackButtonElement(DirectoryElement parent) : base("...")
    {
        Parent = parent;
    }
}
