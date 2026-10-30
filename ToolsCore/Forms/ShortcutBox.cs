using ExControls;

namespace ToolsCore.Forms;

/// <summary>
/// Pole na zadanie klavesovej skratky stlacenim klavesov. Kazdy stlaceny kláves (okrem samotnych modifikatorov a Tab,
/// ktory presuva fokus) oznami cez <see cref="KeysCaptured" /> - text sa do pola nepise.
/// </summary>
public sealed class ShortcutBox : ExTextBox
{
    /// <summary>
    /// Stlacena kombinacia klavesov (aj neplatna ako skratka - napr. samotne pismeno alebo Esc).
    /// </summary>
    public event EventHandler<KeysEventArgs>? KeysCaptured;

    /// <summary>
    /// Ci pole zachytava klavesy; <see langword="false" /> = obycajne textove pole.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool CaptureKeys { get; set; } = true;

    /// <inheritdoc />
    protected override bool ProcessCmdKey(ref Message m, Keys keyData)
    {
        if (!CaptureKeys || keyData is Keys.Tab or (Keys.Tab | Keys.Shift))
            return base.ProcessCmdKey(ref m, keyData);

        if ((keyData & Keys.KeyCode) is not (Keys.ControlKey or Keys.ShiftKey or Keys.Menu or Keys.LWin or Keys.RWin))
            KeysCaptured?.Invoke(this, new KeysEventArgs(keyData));
        return true;
    }

    /// <inheritdoc />
    protected override void OnKeyPress(KeyPressEventArgs e)
    {
        e.Handled = CaptureKeys;
        base.OnKeyPress(e);
    }
}

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
