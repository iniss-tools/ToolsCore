namespace ToolsCore.CodeEditor;

/// <summary>
/// Chyba alebo upozornenie v kode, ktore editor podciarkne a popise pri podrzani mysi.
/// </summary>
/// <param name="Start">poloha zaciatku v texte editora (znaky)</param>
/// <param name="Length">dlzka; 0 = do konca slova alebo riadku</param>
/// <param name="Message">text pre obsluhu</param>
/// <param name="IsError">chyba; inak upozornenie</param>
public sealed record CodeIssue(int Start, int Length, string Message, bool IsError);
