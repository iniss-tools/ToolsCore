namespace ToolsCore.Forms;

/// <summary>
/// Stlacena kombinacia klavesov.
/// </summary>
public sealed class KeysEventArgs(Keys keys) : EventArgs
{
    /// <summary>
    /// Klavesy vratane modifikatorov.
    /// </summary>
    public Keys Keys { get; } = keys;
}
