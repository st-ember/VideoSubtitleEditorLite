namespace SubtitleEditor.Web.Infrastructure.Models.Topic;

public class TopicUpdateModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public double? FrameRate { get; set; }
    public int? WordLimit { get; set; }
    public string? ModelName { get; set; }
}
