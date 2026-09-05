using System.Collections;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using ToolsCore.XML;

namespace ToolsCore.Converters;

public class AppFontConverter : FontConverter
{
    /// <inheritdoc />
    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (destinationType == typeof(string) && value is AppFont afs)
        {
            var converter = TypeDescriptor.GetConverter(typeof(Font));
            return converter.ConvertTo(afs.Font ?? SystemFonts.DefaultFont, typeof(string));
        }

        if (destinationType == typeof(InstanceDescriptor) && value is AppFont af)
        {
            var ctor = typeof(AppFont).GetConstructor([typeof(Font)]);
            return new InstanceDescriptor(ctor, new object[] { af.Font ?? SystemFonts.DefaultFont});
        }

        return base.ConvertTo(context, culture, value, destinationType);
    }

    /// <inheritdoc />
    public override bool GetPropertiesSupported(ITypeDescriptorContext? context) => false;

    /// <inheritdoc />
    public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        return value is not string ? base.ConvertFrom(context, culture, value)! : new AppFont((base.ConvertFrom(context, culture, value) as Font)!);
    }

    /// <inheritdoc />
    public override object CreateInstance(ITypeDescriptorContext? context, IDictionary propertyValues)
        => new AppFont((base.CreateInstance(context, propertyValues) as Font)!);
}