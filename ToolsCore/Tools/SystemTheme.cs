using Microsoft.Win32;

namespace ToolsCore.Tools;

/// <summary>
/// Svetly alebo tmavy rezim aplikacii vo Windows (Nastavenia - Prisposobenie - Farby).
/// </summary>
public static class SystemTheme
{
    private const string PersonalizeKey = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

    /// <summary>
    /// Windows je v tmavom rezime aplikacii.
    /// </summary>
    public static bool IsDark
    {
        get
        {
            using var key = Registry.CurrentUser.OpenSubKey(PersonalizeKey);
            return key?.GetValue("AppsUseLightTheme") is int value && value == 0;
        }
    }
}
