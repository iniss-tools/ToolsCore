namespace ToolsCore.CodeEditor;

/// <summary>
/// Farebna schema programu s farbami editora kodu VB.NET - program ju ma v zozname <c>ColorKeys</c> spolu
/// s <see cref="VisualBasicColors.Keys" />.
/// </summary>
public interface IVisualBasicStyle
{
    /// <summary>Farby editora kodu VB.NET.</summary>
    VisualBasicScheme VisualBasicScheme { get; }
}
