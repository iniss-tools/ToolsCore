using ToolsCore.Iniss.Tools;

namespace ToolsCore.Iniss.Entities;

public class SoundFileElement : FileElement
{
    public const string WAVExt = ".WAV";
    public const string EWAExt = ".EWA";

    public SoundFileElement(FileInfo fileinfo) : base(fileinfo) => Duration = -1;

    public SoundFileElement(string file) : base(file) => Duration = -1;

    public int Duration { get; set; }

    public string DurationText => ParseUtils.LengthIntToString(Duration);

    public FyzSound Sound { get; set; } = null!;
}
