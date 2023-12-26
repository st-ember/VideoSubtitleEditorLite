using SubtitleEditor.Core.Contexts;

namespace SubtitleEditor.Web.Infrastructure.Models.Topic;

public class TopicCreationModel
{
    public string Filename { get; set; } = string.Empty;
    public string Ticket { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TopicCreateType CreateType { get; set; }
    public string? SubtitleTicket { get; set; }
    public string? TranscriptTicket { get; set; }
    public double? FrameRate { get; set; }
    public int? WordLimit { get; set; }
    public string? ModelName { get; set; }
}
