namespace ToolsCore.XML;

/// <summary>
///     Predstavuje triedu, ktorá spracuváva Enumerácie serializovateľné do XML.
/// </summary>
/// <typeparam name="T"></typeparam>
public static class XmlEnum<T> where T : Enum
{
    /// <summary>
    ///     Konvertuje <see cref="string"/> <paramref name="s"/> na prvok enumeracie typu <see cref="T"/>.
    /// </summary>
    /// <param name="s">Retazec reprezentujuci prvok enumeracie.</param>
    /// <returns>prvok enumeracie typu <see cref="T"/>.</returns>
    public static T StringToEnum(string s) => (string.IsNullOrEmpty(s) ? default : (T)Enum.Parse(typeof(T), s))!;

    /// <summary>
    ///     Konvertuje prvok enumeracie typu <see cref="T"/> na <see cref="string"/>.
    /// </summary>
    /// <param name="e">Prvok enumeracie typu <see cref="T"/>.</param>
    /// <returns>retazec reprezentujuci prvok enumeracie.</returns>
    public static string EnumToString(T e) => Enum.GetName(typeof(T), e) ?? e.ToString();
}