using ToolsCore.Iniss.Properties;
using ToolsCore.Iniss.Tools;

namespace ToolsCore.Iniss.Tables;

/// <summary>
/// Definuje sposob zadavania informacii do sekcie katalogovej tabule.
/// </summary>
public sealed class TableDivType : Enumeration<TableDivType>
{
    private TableDivType(int id, string name) : base(id, name)
    {
    }

    /// <summary>
    /// Skonvertuje ID DivType na objekt
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static TableDivType? Parse(int s)
    {
        return s switch
        {
            0 => Free,
            1 => Table,
            2 => TableTime,
            3 => Translate,
            4 => Char,
            _ => null
        };
    }

    #region VALUES

    /// <summary>
    /// Text ide na tabulu bez prekodovania; TAB1 a TAB2 sa nepouziju.
    /// </summary>
    public static readonly TableDivType Free = new(0, Resources.DivType_Free);

    /// <summary>
    /// Text sa cely hlada v TAB1 a pouzije sa len najdeny preklad - inak stlpec ostane prazdny.
    /// </summary>
    public static readonly TableDivType Table = new(1, Resources.DivType_Table);

    /// <summary>
    /// Cas HH:MM po castiach: hodiny v TAB1, desiatky a jednotky minut v TAB2.
    /// </summary>
    public static readonly TableDivType TableTime = new(2, Resources.DivType_TableTime);

    /// <summary>
    /// Text sa hlada v TAB1; ak sa nenajde, posle sa nezmeneny.
    /// </summary>
    public static readonly TableDivType Translate = new(3, Resources.DivType_Translate);

    /// <summary>
    /// Text sa prekoduje znak po znaku podla TAB1; neznamy znak nahradi medzera.
    /// </summary>
    public static readonly TableDivType Char = new(4, Resources.DivType_Char);

    #endregion
}