namespace ToolsCore.Iniss.Grafikon;

/// <summary>
/// Co INISS urobi so spravou GTN pre vlak, ktory v stanici grafikonu nie je (pismena <c>K</c> a <c>M</c>
/// v priznakoch DirList.TXT). Vlak do grafikonu nezaklada ani v jednom pripade.
/// </summary>
public enum DirListMissingTrain
{
    /// <summary>
    /// Sprava sa len zaloguje (<c>train ... not found</c>) a zahodi.
    /// </summary>
    Ignore,

    /// <summary>
    /// <c>K</c> - stanica sa zaradi do trasy vlaku z inych stanic ako prazdne miesto a sprava sa uplatni
    /// na nasledujuce stanice trasy.
    /// </summary>
    ApplyToRoute,

    /// <summary>
    /// <c>M</c> - ako <see cref="ApplyToRoute" />, navyse INISS z priecinka grafikonu nenacita Categori.TXT.
    /// </summary>
    ApplyToRouteWithoutCategori
}
