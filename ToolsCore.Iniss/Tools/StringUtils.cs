namespace ToolsCore.Iniss.Tools;

/// <summary>
/// Rozsirenia retazcov a kolekcii.
/// </summary>
public static partial class StringUtils
{
    /// <summary>
    /// Vrati retazec v uvodzovkach. Ak je text <see langword="null"/>, vrati "\"\"".
    /// </summary>
    /// <param name="text">Povodny retazec.</param>
    /// <returns>retazec v uvodzovkach.</returns>
    public static string Quote(this string text) => text == null ? "\"\"" : $"\"{text}\"";

    /// <summary>
    /// Zisti, ci zoznam obsahuje vsetky prvky ineho zoznamu.
    /// </summary>
    /// <param name="containingList">Vacsi zoznam, ktory kontrolujeme.</param>
    /// <param name="lookupList">Zoznam, ktory treba vyhladat v zozname.</param>
    /// <returns><see langword="true"/> ak obsahuje vsetky prvky, inak <see langword="false"/>.</returns>
    public static bool ContainsAllItems<T>(this IEnumerable<T> containingList, IEnumerable<T> lookupList) => !lookupList.Except(containingList).Any();

    /// <summary>
    /// Vrati retazec, ktory sa nachadza na pozicii <paramref name="index"/> pola/listu <paramref name="source"/>.<br></br>
    /// Ak je <paramref name="index"/> mimo rozsahu pola alebo je retazec na indexe
    /// <see langword="null"/>, vrati predvoleny retazec urceny parametrom <paramref name="def"/>.
    /// </summary>
    /// <param name="source">Pole/list retazcov.</param>
    /// <param name="index">Pozicia prvku.</param>
    /// <param name="def">Predvoleny retazec.</param>
    /// <returns>Retazec, alebo predvoleny retazec <paramref name="def"/>.</returns>
    public static string ElementAtOrDefaultStr(this IEnumerable<string> source, int index, string def = "") => source.ElementAtOrDefault(index) ?? def;

    /// <summary>
    /// Porovna retazce, pricom ignoruje velkost pismen (VELKE/male).
    /// </summary>
    /// <param name="str1">Prvy retazec na porovnavanie.</param>
    /// <param name="str2">Druhy retazec na porovnavanie.</param>
    /// <returns>ci sa retazce zhoduju.</returns>
    public static bool EqualsIgnoreCase(this string str1, string str2) => str1 != null && str1.Equals(str2, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Returns a new string in which all occurrences of a specified string in the current instance are replaced with
    /// another
    /// specified string according the type of search to use for the specified string.<br></br>
    /// Source: https://stackoverflow.com/questions/6275980/string-replace-ignoring-case
    /// </summary>
    /// <param name="str">The string performing the replace method.</param>
    /// <param name="oldValue">The string to be replaced.</param>
    /// <param name="newValue">
    /// The string replace all occurrences of <paramref name="oldValue" />.
    /// If value is equal to <c>null</c>, than all occurrences of <paramref name="oldValue" /> will be removed from the
    /// <paramref name="str" />.
    /// </param>
    /// <param name="comparisonType">One of the enumeration values that specifies the rules for the search.</param>
    /// <returns>
    /// A string that is equivalent to the current string except that all instances of <paramref name="oldValue" /> are
    /// replaced with <paramref name="newValue" />.
    /// If <paramref name="oldValue" /> is not found in the current instance, the method returns the current instance
    /// unchanged.
    /// </returns>
    public static string Replace(this string str, string oldValue, string newValue, StringComparison comparisonType)
    {
        // Check inputs.
        ArgumentNullException.ThrowIfNull(str);
        if (str.Length == 0)
            return str;
        ArgumentNullException.ThrowIfNull(oldValue);
        if (oldValue.Length == 0)
            throw new ArgumentException("String cannot be of zero length.");

        // Prepare string builder for storing the processed string.
        // Note: StringBuilder has a better performance than String by 30-40%.
        var resultStringBuilder = new StringBuilder(str.Length);

        // Analyze the replacement: replace or remove.
        var isReplacementNullOrEmpty = string.IsNullOrEmpty(newValue);

        // Replace all values.
        const int valueNotFound = -1;
        int foundAt;
        var startSearchFromIndex = 0;
        while ((foundAt = str.IndexOf(oldValue, startSearchFromIndex, comparisonType)) != valueNotFound)
        {
            // Append all characters until the found replacement.
            var charsUntilReplacment = foundAt - startSearchFromIndex;
            var isNothingToAppend = charsUntilReplacment == 0;
            if (!isNothingToAppend)
                resultStringBuilder.Append(str, startSearchFromIndex, charsUntilReplacment);

            // Process the replacement.
            if (!isReplacementNullOrEmpty)
                resultStringBuilder.Append(newValue);

            // Prepare start index for the next search.
            // This needed to prevent infinite loop, otherwise method always start search 
            // from the start of the string. For example: if an oldValue == "EXAMPLE", newValue == "example"
            // and comparisonType == "any ignore case" will conquer to replacing:
            // "EXAMPLE" to "example" to "example" to "example" … infinite loop.
            startSearchFromIndex = foundAt + oldValue.Length;
            if (startSearchFromIndex == str.Length)
                // It is end of the input string: no more space for the next search.
                // The input string ends with a value that has already been replaced. 
                // Therefore, the string builder with the result is complete and no further action is required.
                return resultStringBuilder.ToString();
        }

        // Append the last part to the result.
        var charsUntilStringEnd = str.Length - startSearchFromIndex;
        resultStringBuilder.Append(str, startSearchFromIndex, charsUntilStringEnd);

        return resultStringBuilder.ToString();
    }

    public static bool StartsWithAny(this string text, params char[] values)
    {
        if (string.IsNullOrEmpty(text))
            return false;
        foreach (var c in values)
        {
            if (text[0] == c)
                return true;
        }

        return false;
    }
}
