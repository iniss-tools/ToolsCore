using System.Globalization;
using System.Resources;

namespace ToolsCore.Converters;

/// <summary>
/// Kategoria vlastnosti v PropertyGrid s nazvom z resources (predvolene <see cref="GlobalResources" />).
/// </summary>
/// <param name="key">kluc textu v resources</param>
/// <param name="type">trieda resources; <see langword="null" /> = <see cref="GlobalResources" /></param>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ResCategoryAttribute(string key, Type? type = null) : CategoryAttribute(key)
{
    /// <summary>
    /// Trieda resources, z ktorej sa berie nazov kategorie.
    /// </summary>
    public Type ResourceType { get; } = type ?? typeof(GlobalResources);

    /// <inheritdoc />
    protected override string? GetLocalizedString(string value) => new ResourceManager(ResourceType).GetString(value, CultureInfo.CurrentUICulture) ?? value;
}
