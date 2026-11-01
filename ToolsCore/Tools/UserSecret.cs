using System.Security.Cryptography;
using Serilog;

namespace ToolsCore.Tools;

/// <summary>
/// Tajna hodnota v configu sifrovana pre aktualneho pouzivatela Windows (DPAPI) - v subore nie je citatelna a na inom
/// pocitaci alebo pod inym pouzivatelom sa precitat neda.
/// </summary>
public static class UserSecret
{
    /// <summary>Zasifrovana hodnota (base64); prazdna hodnota = prazdny retazec.</summary>
    public static string Protect(string? value, byte[] entropy) =>
        string.IsNullOrWhiteSpace(value)
            ? ""
            : Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(value.Trim()), entropy, DataProtectionScope.CurrentUser));

    /// <summary>Desifrovana hodnota; ulozena inym pouzivatelom alebo poskodena = prazdny retazec.</summary>
    public static string Unprotect(string protectedValue, byte[] entropy)
    {
        if (protectedValue.Length == 0) return "";
        try
        {
            return Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(protectedValue), entropy, DataProtectionScope.CurrentUser));
        }
        catch (Exception e) when (e is CryptographicException or FormatException)
        {
            // hodnotu ani jej sifru nezapisovat
            Log.Warning("Tajnu hodnotu z nastaveni sa nepodarilo desifrovat ({Error}) - pouzije sa prazdna; ulozil ju iny pouzivatel alebo pocitac?", e.GetType().Name);
            return "";
        }
    }
}
