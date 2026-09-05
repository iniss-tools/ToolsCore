using System.Drawing.Design;
using ToolsCore.XML;

namespace ToolsCore.Converters;

public class AppFontEditor : FontEditor
{
    /// <inheritdoc />
    public override object? EditValue(ITypeDescriptorContext? context, IServiceProvider provider, object? value)
    {
        return base.EditValue(context, provider, (value as AppFont)?.Font) is not Font fnt ? value : new AppFont(fnt);
    }
}