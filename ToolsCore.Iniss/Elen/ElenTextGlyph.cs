namespace ToolsCore.Iniss.Elen;

/// <summary>
/// Znak textu vykresleneho pismom ELEN (<see cref="ElenTextRaster" />).
/// </summary>
/// <param name="Index">poradie znaku v texte</param>
/// <param name="Code">kod znaku v pisme, ktorym sa kreslil (pri vysokych cisliciach nahradny kod)</param>
/// <param name="X">prvy stlpec bodov znaku</param>
/// <param name="Width">sirka znaku v bodoch; 0 = znak v pisme nie je</param>
public readonly record struct ElenTextGlyph(int Index, byte Code, int X, int Width)
{
    /// <summary>Znak v pisme nie je (sirka 0) - panel ho preskoci.</summary>
    public bool IsMissing => Width == 0;
}
