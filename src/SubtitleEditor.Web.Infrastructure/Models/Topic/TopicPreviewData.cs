using SubtitleEditor.Core.Contexts;

namespace SubtitleEditor.Web.Infrastructure.Models.Topic;

public class TopicPreviewData
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Filename { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public long OriginalSize { get; set; }
    public long Size { get; set; }
    public double Length { get; set; }
    public double ProcessTime { get; set; }
    public double? FrameRate { get; set; }
    public int? WordLimit { get; set; }
    public TopicStatus Status { get; set; }
    public long? AsrTaskId { get; set; }
    public string? ModelName { get; set; }
    public AsrMediaStatus AsrMediaStatus { get; set; }
    public ConvertMediaStatus ConvertMediaStatus { get; set; }
    public string? Error { get; set; }

    public string LengthText
    {
        get => TimeSpan.FromSeconds(Length).ToString("hh\\:mm\\:ss");
        set => Length = TimeSpan.TryParse(value, out var span) ? span.TotalSeconds : 0;
    }

    public string ProcessTimeText
    {
        get => TimeSpan.FromSeconds(ProcessTime).ToString("hh\\:mm\\:ss");
        set => ProcessTime = TimeSpan.TryParse(value, out var span) ? span.TotalSeconds : 0;
    }
}
