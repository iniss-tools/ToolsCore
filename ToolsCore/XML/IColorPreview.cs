namespace ToolsCore.XML;

/// <summary>
/// Nahlad kategorie farebnej schemy na stranke schemy: kresli farby schemy a klik vyberie polozku.
/// </summary>
public interface IColorPreview
{
    /// <summary>
    /// Prvok nahladu.
    /// </summary>
    Control Control { get; }

    /// <summary>
    /// Pouzivatel klikol na cast nahladu s danou polozkou (identifikator <see cref="ColorKey.Id" />).
    /// </summary>
    event EventHandler<string>? KeyClicked;

    /// <summary>
    /// Zobrazi farby schemy.
    /// </summary>
    void ShowScheme(Style style);
}
