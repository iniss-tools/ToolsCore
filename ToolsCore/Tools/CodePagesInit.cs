using System.Runtime.CompilerServices;

namespace ToolsCore.Tools;

/// <summary>
///     Registruje poskytovateľa legacy code-page kódovaní (napr. Windows-1250 používané v <see cref="Encodings"/>).
///     Od .NET 5 už nie sú tieto kódovania súčasťou runtime a bez registrácie <see cref="Encoding.GetEncoding(int)"/>
///     vyhadzuje <see cref="NotSupportedException"/>. <see cref="ModuleInitializerAttribute"/> zaručuje spustenie
///     pred prvým použitím čohokoľvek z tejto zostavy (aj pred statickým konštruktorom <see cref="Encodings"/>).
/// </summary>
internal static class CodePagesInit
{
    // CA2255: ModuleInitializer sa neodporúča v knižniciach, lebo spustí kód len tým, že ho niekto referencuje.
    // ToolsCore je interná knižnica tohto solution (nepublikuje sa ako NuGet balík), pričom registrácia je
    // idempotentná a bezpečná pre kohokoľvek - zámerné potlačenie.
#pragma warning disable CA2255
    [ModuleInitializer]
    internal static void Register() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#pragma warning restore CA2255
}
