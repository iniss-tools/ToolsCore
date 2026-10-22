using System.Reflection;

namespace ToolsCore.Iniss.Updates;

/// <summary>
/// Odkial program zistuje aktualizacie: jeho nazov (kluc v indexe), nainstalovana verzia a adresa indexu.
/// </summary>
/// <param name="Program">nazov programu - nazov zostavy</param>
/// <param name="Version">verzia programu bez revizie</param>
/// <param name="Index">adresa <c>update.json</c> posledneho vydania (<c>.../releases/latest/download/update.json</c>)</param>
public sealed record UpdateSource(string Program, Version Version, Uri Index)
{
    /// <summary>Kluc <see cref="AssemblyMetadataAttribute" /> s adresou indexu (ToolsCore\Build\AppRelease.props).</summary>
    public const string IndexMetadataKey = "UpdateIndex";

    /// <summary>
    /// Zdroj aktualizacii programu podla jeho zostavy. Adresu indexu ma len zostava z vydania - lokalna zostava vrati
    /// <c>null</c> a aktualizacie nekontroluje.
    /// </summary>
    public static UpdateSource? FromAssembly(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        var index = assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(a => string.Equals(a.Key, IndexMetadataKey, StringComparison.Ordinal))?.Value;
        if (!Uri.TryCreate(index, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
            return null;
        var name = assembly.GetName();
        if (string.IsNullOrEmpty(name.Name) || name.Version is null)
            return null;
        return new UpdateSource(name.Name, ReleaseVersion.Normalize(name.Version), uri);
    }
}
