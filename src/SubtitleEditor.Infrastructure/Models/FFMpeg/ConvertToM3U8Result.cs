namespace SubtitleEditor.Infrastructure.Models.FFMpeg;

public class ConvertToM3U8Result
{
    public string[] OutputFilePaths { get; set; } = Array.Empty<string>();
    public string Output { get; set; } = string.Empty;
}
