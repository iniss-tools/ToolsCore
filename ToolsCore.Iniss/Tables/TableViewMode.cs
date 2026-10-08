using ToolsCore.Iniss.Properties;
using ToolsCore.Iniss.Tools;

namespace ToolsCore.Iniss.Tables;

/// <summary>
/// Definuje mód zobrazenia udaju na tabuli.
/// </summary>
public sealed class TableViewMode : Enumeration<TableViewMode>
{
    private TableViewMode(string key, string name) : base(key, name)
    {
    }

    /// <summary>
    /// Odkaz na seba, pre potreby DataSource.
    /// </summary>
    public TableViewMode This => this;

    /// <summary>
    /// Skonvertuje mód zobrazenia podla kluca na objekt.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public new static TableViewMode? Parse(string s)
    {
        return s switch
        {
            "LVM_Nothing" => Nothing,
            "LVM_Vlak" => Vlak,
            "LVM_VlakZpozdenyPrijezd" => VlakZmeskanyPrichod,
            "LVM_VlakZpozdenyOdjezd" => VlakZmeskanyOdchod,
            "LVM_VlakZpozdeny" => VlakZmeskany,
            "LVM_Text" => Text,
            _ => null
        };
    }

    #region VALUES

    public static readonly TableViewMode Nothing = new("LVM_Nothing", Resources.ViewMode_Nothing);
    public static readonly TableViewMode Vlak = new("LVM_Vlak", Resources.ViewMode_Vlak);
    public static readonly TableViewMode VlakZmeskanyPrichod = new("LVM_VlakZpozdenyPrijezd", Resources.ViewMode_VlakZmeskanyPrichod);
    public static readonly TableViewMode VlakZmeskanyOdchod = new("LVM_VlakZpozdenyOdjezd", Resources.ViewMode_VlakZmeskanyOdchod);
    public static readonly TableViewMode VlakZmeskany = new("LVM_VlakZpozdeny", Resources.ViewMode_VlakZmeskany);
    public static readonly TableViewMode Text = new("LVM_Text", "Text");

    #endregion
}