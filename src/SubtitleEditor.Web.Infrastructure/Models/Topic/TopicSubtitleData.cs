using SubtitleEditor.Core.Models;

namespace SubtitleEditor.Web.Infrastructure.Models.Topic;

public class TopicSubtitleData
{
    public string Name { get; set; } = string.Empty;
    public string Filename { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Subtitle Subtitle { get; set; } = null!;
    public string? Transcript { get; set; }
    public double? FrameRate { get; set; }
    public int? WordLimit { get; set; }
}