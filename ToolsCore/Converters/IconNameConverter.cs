using ToolsCore.Icons;

namespace ToolsCore.Converters;

/// <summary>
/// Ponuka nazvov ikon banky <see cref="AppIcons" /> v okne vlastnosti navrhara.
/// </summary>
public class IconNameConverter : StringConverter
{
    /// <inheritdoc />
    public override bool GetStandardValuesSupported(ITypeDescriptorContext? context) => true;

    /// <inheritdoc />
    public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext? context) => new(AppIcons.Names.ToArray());
}
