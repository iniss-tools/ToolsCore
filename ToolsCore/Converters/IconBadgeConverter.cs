using ToolsCore.Icons;

namespace ToolsCore.Converters;

/// <summary>
/// Ponuka nazvov odznakov banky <see cref="AppIcons" /> v okne vlastnosti navrhara.
/// </summary>
public class IconBadgeConverter : StringConverter
{
    /// <inheritdoc />
    public override bool GetStandardValuesSupported(ITypeDescriptorContext? context) => true;

    /// <inheritdoc />
    public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext? context) => new(AppIcons.Badges.ToArray());
}
