using System.Globalization;

namespace ToolsCore.Iniss.Registry;

/// <summary>
/// Sekcia registra INISSu (podkluc pod <c>CHAPS\&lt;aplikacia&gt;</c>).
/// </summary>
/// <param name="name">nazov sekcie; pri cislovanych zaklad (<c>Driver</c>)</param>
/// <param name="hive">vetva</param>
/// <param name="group">skupina v okne</param>
/// <param name="maxIndex">najvyssi index cislovanej sekcie (Driver 98, Volume 8); -1 = jedina sekcia</param>
/// <param name="twoDigit">cislovana sekcia ma pre indexy 0-9 aj tvar 00-09</param>
/// <param name="descriptionKey">kluc popisu</param>
/// <param name="settings">nastavenia sekcie</param>
public sealed class RegSection(string name, RegHive hive, RegGroup group, int maxIndex, bool twoDigit, string descriptionKey, IReadOnlyList<RegSetting> settings)
{
    /// <summary>Nazov sekcie.</summary>
    public string Name { get; } = name;

    /// <summary>Vetva.</summary>
    public RegHive Hive { get; } = hive;

    /// <summary>Skupina v okne.</summary>
    public RegGroup Group { get; } = group;

    /// <summary>Najvyssi index cislovanej sekcie; -1 = jedina sekcia.</summary>
    public int MaxIndex { get; } = maxIndex;

    /// <summary>Pre indexy 0-9 aj dvojciferny tvar.</summary>
    public bool TwoDigit { get; } = twoDigit;

    /// <summary>Kluc popisu.</summary>
    public string DescriptionKey { get; } = descriptionKey;

    /// <summary>Nastavenia.</summary>
    public IReadOnlyList<RegSetting> Settings { get; } = settings;

    /// <summary>Cislovana sekcia (Driver, Volume).</summary>
    public bool IsNumbered => MaxIndex >= 0;

    /// <summary>Popis v jazyku UI.</summary>
    public string Description => RegTexts.Get(DescriptionKey);

    /// <summary>
    /// Nazvy podklucov v poradi, v akom ich INISS hlada: zaklad, potom 0..max a pri dvojcifernom tvare 00..09.
    /// </summary>
    public IEnumerable<string> InstanceNames()
    {
        if (!IsNumbered)
        {
            yield return Name;
            yield break;
        }

        yield return Name;
        for (var i = 0; i <= MaxIndex; i++)
        {
            yield return Name + i.ToString(CultureInfo.InvariantCulture);
            if (TwoDigit && i <= 9) yield return Name + i.ToString("00", CultureInfo.InvariantCulture);
        }
    }

    /// <summary>Nastavenie podla nazvu (bez rozlisenia velkosti pismen, ako register).</summary>
    public RegSetting? Find(string name) => Settings.FirstOrDefault(s => s.Kind == RegNameKind.Fixed && string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));

    /// <inheritdoc />
    public override string ToString() => Name;
}
