using System.Resources;

namespace ToolsCore.Converters;

public class ResDisplayNameAttribute : DisplayNameAttribute
{
    public ResDisplayNameAttribute(string key) : base(Init(typeof(GlobalResources), key))
    {
    }

    public ResDisplayNameAttribute(string key, Type type) : base(Init(type, key))
    {
    }

    private static string Init(Type type, string key)
    {
        var manager = new ResourceManager(type);
        return manager.GetString(key) ?? key;
    }
}