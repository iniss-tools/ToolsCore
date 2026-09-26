using ToolsCore.Tools;
// ReSharper disable UnusedMember.Global

namespace ToolsCore.Entities;

public class FyzSound
{
    /// <summary>Initializes a new instance of the <see cref="FyzSound" /> class. Don't use this overload of constructor!</summary>
    public FyzSound()
    {
        Duration = 0;
    }

    /// <summary>Initializes a new instance of the <see cref="FyzSound" /> class.</summary>
    public FyzSound(FyzGroup grp, string key, string name, string fileName, string addiRelativePath, string text, int duration)
    {
        Group = grp;
        Key = key;
        Name = name;
        FileName = fileName;
        AdditionalRelativePath = addiRelativePath;
        Text = text;
        Duration = duration;
    }

    /// <summary>
    ///     Kluc zvuku (v ZvukBase sa neda menit).
    /// </summary>
    public string Key { get; set; } = null!;

    /// <summary>
    ///     Nazov zvuku (v ZvukBase sa da menit).
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    ///     Nazov suboru zvuku (s priponou).
    /// </summary>
    public string FileName { get; set; } = null!;

    /// <summary>
    ///     Doplnkova relativna cesta k suboru, ak sa subor nenachadza.
    /// </summary>
    public string AdditionalRelativePath { get; set; } = null!;

    /// <summary>
    ///     Text hlasenia.
    /// </summary>
    public string Text { get; set; } = null!;

    /// <summary>
    ///     Dlzka zvuku v milisekundach (ms).
    /// </summary>
    public int Duration { get; set; }

    /// <summary>
    ///     Dlzka zvuku ako retazec v tvare mm:ss.
    /// </summary>
    public string DurationText => Utils.LengthIntToString(Duration);

    /// <summary>
    ///     Skupina zvukov, do ktorej patri tento zvuk.
    /// </summary>
    public FyzGroup Group { get; set; } = null!;

    /// <summary>
    ///     Jazyk, do ktoreho patri tento zvuk.
    /// </summary>
    public FyzLanguage Language => Group.Language;

    /// <summary>
    ///     Odkaz na fyzicky subor zvuku.
    /// </summary>
    public SoundFileElement File { get; set; } = null!;

    /// <summary>Returns a string that represents the current object.</summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() => Name;

    /// <summary>
    ///     Cesta k suboru zvuku: banka + priecinok jazyka + priecinok skupiny + pridavna cesta + nazov suboru.
    /// </summary>
    /// <remarks>
    ///     Pridavna cesta je relativna k priecinku skupiny - realne banky maju napr. v skupine N5\ subor
    ///     ..\N5\01.WAV alebo v R1\ subor ..\C9\..\Poz7\..\Poz1\ZALOK.WAV a subory lezia v CZ\N5\ a SK\Poz1\.
    ///     Pri absolutnej ceste sa ".." vyhodnotia, aby sa cesta dala porovnat s cestami suborov na disku.
    /// </remarks>
    /// <param name="pathToBank">priecinok banky (RAWBANK\) alebo "" pre cestu relativnu k banke.</param>
    public string GetAbsPath(string pathToBank)
    {
        ArgumentNullException.ThrowIfNull(Group);
        ArgumentNullException.ThrowIfNull(Group.Language);
        ArgumentNullException.ThrowIfNull(pathToBank);

        var path = new StringBuilder(pathToBank);

        path.Append(Group.Language.RelativePath);
        path.Append(Group.RelativePath);
        if (!RawBankParser.AdditionalPathIsEmpty(AdditionalRelativePath))
            path.Append(AdditionalRelativePath);
        path.Append(FileName);

        var result = path.ToString();
        return Path.IsPathRooted(result) ? Path.GetFullPath(result) : result;
    }
}