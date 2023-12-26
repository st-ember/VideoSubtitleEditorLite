namespace SubtitleEditor.Infrastructure.Models.Asr;

public class NctuAsrVersion
{
    public string AsrKernelVersion { get; set; } = string.Empty;
    public string CaptionMakerVersion { get; set; } = string.Empty;
    public string BackendVersion { get; set; } = string.Empty;
    public string ServerVersion { get; set; } = string.Empty;
}
