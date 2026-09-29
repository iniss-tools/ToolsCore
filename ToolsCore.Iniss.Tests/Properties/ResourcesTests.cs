using System.Globalization;
using ToolsCore.Iniss.Properties;

namespace ToolsCore.Tests.Properties;

/// <summary>
/// Hlasenia domeny su v slovencine aj cestine (satelitna zostava cs).
/// </summary>
[TestClass]
public class ResourcesTests
{
    [TestMethod]
    [DataRow("sk", "Diagram nemá žiadnu kategóriu vlakov")]
    [DataRow("cs", "Diagram nemá žádnou kategorii vlaků")]
    public void Resources_PodlaKultury_VratiText(string culture, string expected) =>
        Assert.AreEqual(expected, Resources.ResourceManager.GetString(nameof(Resources.Sdv_NoCategories), new CultureInfo(culture)));
}
