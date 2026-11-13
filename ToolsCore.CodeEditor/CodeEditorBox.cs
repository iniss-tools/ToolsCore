using System.ComponentModel;
using ScintillaNET;

namespace ToolsCore.CodeEditor;

/// <summary>
/// Editor kodu Scintilla s vlastnymi posuvnikmi (tmavy rezim posuvnikov).
/// </summary>
public partial class CodeEditorBox : UserControl
{
    /// <summary>
    /// Editor kodu Scintilla s vlastnymi posuvnikmi (tmavy rezim posuvnikov).
    /// </summary>
    public CodeEditorBox()
    {
        InitializeComponent();

        VScrollBarControl.Scroll += VScrollBarControlOnScroll;
        HScrollBarControl.Scroll += HScrollBarControlOnScroll;
    }

    private void VScrollBarControlOnScroll(object? sender, ScrollEventArgs e)
    {
        scintillaEditor.FirstVisibleLine = e.NewValue;
    }

    private void HScrollBarControlOnScroll(object? sender, ScrollEventArgs e)
    {
        scintillaEditor.XOffset = e.NewValue;
    }

    private void Scintilla_UpdateUI(object sender, UpdateUIEventArgs e)
    {
        if (IsChange(e.Change, UpdateChange.VScroll))
        {
            VScrollBarControl.Minimum = 0;
            VScrollBarControl.Maximum = scintillaEditor.Lines.Count;
            VScrollBarControl.LargeChange = scintillaEditor.LinesOnScreen;
            VScrollBarControl.SmallChange = 1;
            VScrollBarControl.Value = scintillaEditor.FirstVisibleLine;
        }

        if (IsChange(e.Change, UpdateChange.HScroll))
        {
            HScrollBarControl.Minimum = 0;
            HScrollBarControl.Maximum = scintillaEditor.ScrollWidth;
            HScrollBarControl.LargeChange = scintillaEditor.Width;
            HScrollBarControl.SmallChange = 10;
            HScrollBarControl.Value = scintillaEditor.XOffset;
        }

        if (IsChange(e.Change, UpdateChange.Content))
        {
            ChangeScrollWidth();

            if (SingleLine || scintillaEditor.Lines.Count < scintillaEditor.LinesOnScreen)
            {
                pVertical.Visible = false;
            }
            else
            {
                pVertical.Visible = true;
                VScrollBarControl.Minimum = 0;
                VScrollBarControl.Maximum = scintillaEditor.Lines.Count;
                VScrollBarControl.LargeChange = scintillaEditor.LinesOnScreen;
                VScrollBarControl.SmallChange = 1;
                VScrollBarControl.Value = scintillaEditor.FirstVisibleLine;
            }

            if (SingleLine || scintillaEditor.ScrollWidth < scintillaEditor.Width)
            {
                pHorizontal.Visible = false;
            }
            else
            {
                pHorizontal.Visible = true;
                HScrollBarControl.Minimum = 0;
                HScrollBarControl.Maximum = scintillaEditor.ScrollWidth;
                HScrollBarControl.LargeChange = scintillaEditor.Width;
                HScrollBarControl.SmallChange = 10;
                HScrollBarControl.Value = scintillaEditor.XOffset;
            }
        }
    }

    private static bool IsChange(UpdateChange changes, UpdateChange expected)
    {
        return (changes & expected) != 0;
    }

    private int LargestLine()
    {
        int index = 0;
        int len = 0;
        foreach (var line in scintillaEditor.Lines)
        {
            if (line.Length > len)
            {
                index = line.Index;
                len = line.Length;
            }
        }

        return index;
    }

    private void ChangeScrollWidth()
    {
        var index = LargestLine();
        if (scintillaEditor.Lines[index].EndPosition - 2 >= 0)
        {
            int point = scintillaEditor.PointXFromPosition(scintillaEditor.Lines[index].EndPosition - 2);
            scintillaEditor.ScrollWidth = point + 100;
        }
    }

    /// <summary>
    /// Dokument bol vymeneny.
    /// </summary>
    public void SwitchedDocument()
    {
        Scintilla_UpdateUI(scintillaEditor, new UpdateUIEventArgs(UpdateChange.Content));
    }

    /// <summary>
    /// Gets the Scintilla control.
    /// </summary>
    [Browsable(true)]
    public Scintilla Scintilla => scintillaEditor;

    /// <summary>Occurs when the user enters a text character.</summary>
    [Category("Notifications")]
    [Description("Occurs when the user types a character.")]
    public event EventHandler<CharAddedEventArgs> CharAdded
    {
        add => scintillaEditor.CharAdded += value;
        remove => scintillaEditor.CharAdded -= value;
    }

    /// <summary>
    /// Occurs when the control is about to display or print text and requires styling.
    /// </summary>
    /// <remarks>
    /// This event is only raised when <see cref="ScintillaNET.Scintilla.Lexer" /> is set to <see cref="ScintillaNET.Lexer.Container" />.
    /// The last position styled correctly can be determined by calling <see cref="ScintillaNET.Scintilla.GetEndStyled" />.
    /// </remarks>
    /// <seealso cref="ScintillaNET.Scintilla.GetEndStyled" />
    [Category("Notifications")]
    [Description("Occurs when the text needs styling.")]
    public event EventHandler<StyleNeededEventArgs> StyleNeeded
    {
        add => scintillaEditor.StyleNeeded += value;
        remove => scintillaEditor.StyleNeeded -= value;
    }

    /// <summary>
    /// Occurs when the control UI is updated as a result of changes to text (including styling),
    /// selection, and/or scroll positions.
    /// </summary>
    [Category("Notifications")]
    [Description("Occurs when the control UI is updated.")]
    public event EventHandler<UpdateUIEventArgs> UpdateUI
    {
        add => scintillaEditor.UpdateUI += value;
        remove => scintillaEditor.UpdateUI -= value;
    }

    /// <inheritdoc cref="TextChanged"/>
    public new event EventHandler TextChanged
    {
        add => scintillaEditor.TextChanged += value;
        remove => scintillaEditor.TextChanged -= value;
    }

    /// <inheritdoc cref="KeyPress"/>
    public new event KeyPressEventHandler KeyPress
    {
        add => scintillaEditor.KeyPress += value;
        remove => scintillaEditor.KeyPress -= value;
    }

    /// <inheritdoc cref="MouseDown"/>
    public new event MouseEventHandler MouseDown
    {
        add => scintillaEditor.MouseDown += value;
        remove => scintillaEditor.MouseDown -= value;
    }
}