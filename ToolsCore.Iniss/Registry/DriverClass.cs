using System.Globalization;
using ToolsCore.Iniss.Properties;

namespace ToolsCore.Iniss.Registry;

/// <summary>
/// Trieda linky k tabuliam (<c>TableClass</c>) - rodina protokolov a vyrobcovia tabul, ktori na linku patria.
/// </summary>
/// <param name="Class">hodnota TableClass</param>
/// <param name="Manufacturers">ciselne kody vyrobcov (<c>MANUFACTURER_KEY</c> katalogu) tabul tejto rodiny</param>
public sealed record DriverClass(int Class, IReadOnlyList<int> Manufacturers)
{
    /// <summary>Nazov rodiny v jazyku UI.</summary>
    public string Name => Resources.ResourceManager.GetString($"Reg_Class_{Class}", CultureInfo.CurrentUICulture) ?? Class.ToString(CultureInfo.CurrentCulture);

    /// <summary>Text do zoznamu: <c>4 – ELEN</c>.</summary>
    public override string ToString() => $"{Class} – {Name}";
}
