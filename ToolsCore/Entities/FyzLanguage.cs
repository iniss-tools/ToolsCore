using JetBrains.Annotations;

namespace ToolsCore.Entities;

public class FyzLanguage
{
    /// <summary>Initializes a new instance of the <see cref="FyzLanguage" /> class.</summary>
    public FyzLanguage(string key, string name, string fileDefName, string relativePath)
    {
        Key = key;
        Name = name;
        FileDefName = fileDefName;
        RelativePath = relativePath;
    }

    /// <summary>Initializes a new instance of the <see cref="FyzLanguage" /> class.</summary>
    public FyzLanguage(string key, string name) 
        : this(key, name, FileConsts.FILE_FYZZVUK, $"{key}\\")
    {
    }

    /// <summary>Initializes a new instance of the <see cref="FyzLanguage" /> class.</summary>
    public FyzLanguage(string key, string name, string relativePath) 
        : this(key, name, FileConsts.FILE_FYZZVUK, relativePath)
    {
    }

    /// <summary>
    ///     Kluc jazyka, napr. SK.
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    ///     Nazov zvukovej banky, napr. Slovencina.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Nazov suboru s definovanymi zvukmi jazyka. Vacsinou FYZZVUK.DAT.
    /// </summary>
    public string FileDefName { get; set; }

    /// <summary>
    ///     Relativna cesta k jazyku, napr. SK\.
    /// </summary>
    public string RelativePath { get; set; }

    /// <summary>
    ///     Ci je jazyk v banke nastaveny ako hlavny (predvoleny).
    /// </summary>
    public bool IsBasic { get; set; }

    /// <summary>
    ///     This (kvoli GUI).
    /// </summary>
    [UsedImplicitly]
    public FyzLanguage This => this;

    /// <summary>
    ///     Zoznam skupin zvukov.
    /// </summary>
    public IList<FyzGroup> Groups { get; set; } = null!;

    /// <summary>
    ///     Odkaz na fyzicky priecinok banky zvukov.
    /// </summary>
    public DirectoryElement Directory { get; set; } = null!;

    /// <summary>Returns a string that represents the current object.</summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() => Name;

    public string GetAbsPath(string pathToBank)
    {
        ArgumentNullException.ThrowIfNull(pathToBank);

        var path = new StringBuilder(pathToBank);

        path.Append(RelativePath);

        return path.ToString();
    }

    /// <summary>
    ///     Vráti jazyk z listujazyk z listu podľa kľúča jazyka.
    /// </summary>
    /// <param name="langs">list zvukov</param>
    /// <param name="key">kľúč jazyka</param>
    /// <returns>jazyk z listu, resp. <see langword="null" /> ak nebola nájdená zhoda.</returns>
    public static FyzLanguage? GetLanguageFromKey(IEnumerable<FyzLanguage> langs, string key) => langs.FirstOrDefault(jazyk => jazyk.Key == key);

    /// <summary>
    ///     Vráti hlavný jazyk z listu zvukov.
    /// </summary>
    /// <param name="langs">list jazykov</param>
    /// <returns>hlavný jazyk alebo <see langword="null" /> ak zadaný list neobsahuje hlavný jazyk</returns>
    public static FyzLanguage? GetBasicLanguage(IEnumerable<FyzLanguage> langs) => langs.FirstOrDefault(jazyk => jazyk.IsBasic);

    /// <summary>
    ///     Zistí, či v zadanom poli jazykov sa nachádza prvok s rovnakým kľúčom ako zadaný kľúč.
    /// </summary>
    /// <param name="languages">list jazykov</param>
    /// <param name="key">kľúč jazyka</param>
    /// <returns>
    ///     <see langword="true" /> ak sa v poli nachádza prvok s rovnakým kľučom ako zadaný kľúč, inak
    ///     <see langword="false" />.
    /// </returns>
    public static bool ContainsKey(ICollection<FyzLanguage>? languages, string? key)
    {
        if (key == null || languages == null || languages.Count == 0) 
            return false;

        return languages.Any(language => language.Key == key);
    }
}