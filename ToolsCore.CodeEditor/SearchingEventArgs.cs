namespace ToolsCore.CodeEditor;

/// <summary>
/// Udaje pri hladani v editore kodu.
/// </summary>
public class SearchingEventArgs : EventArgs
{
    /// <summary>
    /// Hladany text.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Nahradzany text.
    /// </summary>
    public string? NewText { get; }

    /// <summary>
    /// Pozicia zaciatku najdeneho vyrazu.
    /// </summary>
    public int StartPosition { get; }

    /// <summary>
    /// Pozicia konca najdeneho vyrazu.
    /// </summary>
    public int EndPosition { get; }

    public int Length => EndPosition - StartPosition;

    public bool Replacing => !string.IsNullOrEmpty(NewText);

    /// <summary>
    /// Konstuktor
    /// </summary>
    /// <param name="startPosition"></param>
    public SearchingEventArgs(string text, int startPosition, int endPosition, string? newText)
    {
        Text = text;
        StartPosition = startPosition;
        EndPosition = endPosition;
        NewText = newText;
    }
}
