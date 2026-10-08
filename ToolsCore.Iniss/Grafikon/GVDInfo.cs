namespace ToolsCore.Iniss.Grafikon;

/// <summary>
/// Informácie o grafikone
/// </summary>
public sealed record GVDInfo
{
    /// <summary>
    /// Kategória
    /// </summary>
    public int Category { get; set; }

    /// <summary>
    /// Podkategória
    /// </summary>
    public int Subcat { get; set; }

    /// <summary>
    /// Reprezentuje stanicu, pre ktorú bol grafikon robený
    /// </summary>
    public Station ThisStation { get; set; } = null!;

    /// <summary>
    /// Počet vlakov
    /// </summary>
    public int TrainCount { get; set; }

    /// <summary>
    /// Začiatok platnosti rozvrhu vlakov
    /// </summary>
    public DateOnly StartValidTimeTable { get; set; }

    /// <summary>
    /// Koniec platnosti rozvrhu vlakov
    /// </summary>
    public DateOnly EndValidTimeTable { get; set; }

    /// <summary>
    /// Začiatok platnosti dát
    /// </summary>
    public DateOnly StartValidData { get; set; }

    /// <summary>
    /// Koniec platnosti dát
    /// </summary>
    public DateOnly EndValidData { get; set; }

    /// <summary>
    /// Dátum vytvorenia/úpravy grafikonu
    /// </summary>
    public DateOnly CreateData { get; set; }

    /// <summary>
    /// TTIndex
    /// </summary>
    public int TTIndex { get; set; }

    /// <summary>
    /// VLIndex
    /// </summary>
    public int VlIndex { get; set; }

    /// <summary>
    /// STIndex
    /// </summary>
    public int StIndex { get; set; }

    /// <summary>
    /// IsRegionText
    /// </summary>
    public bool IsRegionText { get; set; }

    /// <summary>
    /// OnlyCityVLIndex
    /// </summary>
    public int OnlyCityVlIndex { get; set; }
}