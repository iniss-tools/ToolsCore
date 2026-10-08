using ToolsCore.Iniss.Properties;
using ToolsCore.Iniss.Tools;

namespace ToolsCore.Iniss.Tables;

/// <summary>
/// Druh tabule.
/// </summary>
public sealed class TableViewType : Enumeration<TableViewType>
{
    private TableViewType(string key, string name) : base(key, name)
    {
    }

    /// <summary>
    /// Tento objekt (použitie pre GUI).
    /// </summary>
    public TableViewType This => this;

    /// <summary>
    /// Konveruje kľúč druhu tabule na objekt.
    /// </summary>
    /// <param name="s">kľúč druhu tabule</param>
    /// <returns>druh tabule alebo <see langword="null" /> ak sa nenašla zhoda</returns>
    public new static TableViewType? Parse(string s)
    {
        return s switch
        {
            "Tabule_Odjezdova" => Odchodova,
            "Tabule_Prijezdova" => Prichodova,
            "Tabule_Nastupistni" => Nastupistna,
            "Tabule_Podchodova" => Podchodova,
            "Tabule_Reklamni" => Reklamna,
            "Tabule_JinyDruh" => Ina,
            _ => null
        };
    }

    #region VALUES

    /// <summary>
    /// Odchodová tabuľa.
    /// </summary>
    public static readonly TableViewType Odchodova = new("Tabule_Odjezdova", Resources.ViewType_Odchodova);

    /// <summary>
    /// Príchodová tabuľa.
    /// </summary>
    public static readonly TableViewType Prichodova = new("Tabule_Prijezdova", Resources.ViewType_Prichodova);

    /// <summary>
    /// Nástupištná tabuľa.
    /// </summary>
    public static readonly TableViewType Nastupistna = new("Tabule_Nastupistni", Resources.ViewType_Nastupistna);

    /// <summary>
    /// Podchodová tabuľa.
    /// </summary>
    public static readonly TableViewType Podchodova = new("Tabule_Podchodova", Resources.ViewType_Podchodova);

    /// <summary>
    /// Reklamná tabuľa.
    /// </summary>
    public static readonly TableViewType Reklamna = new("Tabule_Reklamni", Resources.ViewType_Reklamna);

    /// <summary>
    /// Iný druh tabule.
    /// </summary>
    public static readonly TableViewType Ina = new("Tabule_JinyDruh", Resources.ViewType_Ina);

    #endregion
}