using System.Drawing;

namespace ToolsCore.Iniss.Grafikon;

/// <summary>
/// Trida obsahujuca informácie o priečinku s grafikonom.
/// </summary>
public sealed class DirList
{
    /// <summary>
    /// Vrati alebo nastavi názov priečinka.
    /// </summary>
    public string DirName { get; set; } = null!;

    /// <summary>
    /// Vrati alebo nastavi celá cestu k priečinku s grafikonom.
    /// </summary>
    public string FullPath { get; set; } = null!;

    /// <summary>
    /// Grafikon leží priamo v priečinku DATA – starší zápis bez <c>DirList.TXT</c>, keď INISS berie
    /// dátový priečinok ako jedinú položku zoznamu. Taký záznam sa do <c>DirList.TXT</c> nezapisuje.
    /// </summary>
    public bool IsDataRoot => string.IsNullOrEmpty(DirName);

    /// <summary>
    /// Vrati alebo nastavi port pre vzdialené ovládanie tabúľ.
    /// </summary>
    public int? TablePort { get; set; }

    /// <summary>
    /// Vrati alebo nastavi port pre zabezpečenie vzdialeného hlásenia.
    /// </summary>
    public int? ReportPort { get; set; }

    /// <summary>
    /// Vrati alebo nastavi príznaky priečinka (4. stĺpec súboru <c>DirList.TXT</c>).
    /// </summary>
    /// <remarks>
    /// INISS v poli hľadá jednotlivé znaky bez ohľadu na poradie a veľkosť písmen:
    /// <c>Z</c> (šírenie externej správy na nasledujúce stanice trasy), <c>O</c> (ktorý
    /// koľajový údaj správa prepisuje), <c>K</c> (do grafikonu sa smú zakladať vlaky
    /// z externých správ), <c>M</c> (ako <c>K</c>, navyše sa z priečinka nenačíta
    /// <c>Categori.TXT</c>) a číslicu <c>1</c>–<c>9</c> (položka je aktívna len pri
    /// spustení INISSu s prepínačom <c>/N</c>).
    /// Rozložené príznaky vracia <see cref="DirListFlags.Parse" />; GVDEditor text prepíše, len keď
    /// používateľ príznaky zmení, inak ho zachová tak, ako bol v súbore.
    /// </remarks>
    public string? Flags { get; set; }

    /// <summary>
    /// Vrati alebo nastavi farbu zobrazujúcu v INISS ako farba pozadia vlaku
    /// na pracovnej ploche ako odlíšenie od vlakov iných staníc.
    /// </summary>
    public Color? BackColor { get; set; }
}