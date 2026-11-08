namespace ToolsCore.XML;

/// <summary>
/// Farebne schemy programu a vyber pouzivanej schemy (pevne zvolena alebo podla svetleho/tmaveho rezimu Windows).
/// </summary>
public interface IStyleSet : IReadOnlyList<Style>
{
    /// <summary>
    /// Zvolena schema (ak sa neriadi rezimom Windows).
    /// </summary>
    string Active { get; set; }

    /// <summary>
    /// Schema sa vyberie podla svetleho alebo tmaveho rezimu Windows.
    /// </summary>
    bool FollowSystem { get; set; }

    /// <summary>
    /// Schema pre svetly rezim Windows.
    /// </summary>
    string LightScheme { get; set; }

    /// <summary>
    /// Schema pre tmavy rezim Windows.
    /// </summary>
    string DarkScheme { get; set; }

    /// <summary>
    /// Schema podla nazvu.
    /// </summary>
    Style? Find(string name);

    /// <summary>
    /// Pouzivana schema.
    /// </summary>
    /// <param name="windowsDark">Windows je v tmavom rezime</param>
    Style Current(bool windowsDark);

    /// <summary>
    /// Nova schema typu programu bez zmien.
    /// </summary>
    Style Create(string name, SchemeBase schemeBase);

    /// <summary>
    /// Prida schemu (typu programu).
    /// </summary>
    void Add(Style style);

    /// <summary>
    /// Odstrani schemu.
    /// </summary>
    bool Remove(Style style);
}
