using ToolsCore.Iniss.Properties;
using ToolsCore.Iniss.Tools;

namespace ToolsCore.Iniss.Tables;

/// <summary>
/// Zarovnanie tabule
/// </summary>
public sealed class TableAlign : Enumeration<TableAlign>
{
    private TableAlign(int id, string name) : base(id, name)
    {
    }

    /// <summary>
    /// Konveruje identifikátor tabule ako <see cref="int" /> na <see cref="TableAlign" />
    /// </summary>
    /// <param name="s"></param>
    /// <returns><see cref="TableAlign" /> alebo <see langword="null" /> ak sa nenašla žiadna zhoda</returns>
    /// <remarks>
    /// Cisla zodpovedaju sekcii [ALIGN] v ModeTabs.TXT tak, ako ju pozna INISS:
    /// 0 = vlavo, 1 = vpravo, 2 = doprostred.
    /// </remarks>
    public static TableAlign? Parse(int s)
    {
        return s switch
        {
            0 => Left,
            1 => Right,
            2 => Center,
            _ => null
        };
    }

    #region VALUES

    /// <summary>
    /// Zarovnanie vľavo
    /// </summary>
    public static readonly TableAlign Left = new(0, Resources.Align_Left);

    /// <summary>
    /// Zarovnanie vpravo
    /// </summary>
    public static readonly TableAlign Right = new(1, Resources.Align_Right);

    /// <summary>
    /// Zarovnanie na stred
    /// </summary>
    public static readonly TableAlign Center = new(2, Resources.Align_Center);

    #endregion
}