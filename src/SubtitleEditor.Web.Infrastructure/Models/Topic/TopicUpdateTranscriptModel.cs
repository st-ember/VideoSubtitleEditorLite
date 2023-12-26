namespace SubtitleEditor.Web.Infrastructure.Models.Topic;

public class TopicUpdateTranscriptModel
{
    public Guid Id { get; set; }
    public string Transcript { get; set; } = string.Empty;
}