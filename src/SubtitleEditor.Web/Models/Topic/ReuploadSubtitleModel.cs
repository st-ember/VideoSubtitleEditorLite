namespace SubtitleEditor.Web.Models.Topic;

public class ReuploadSubtitleModel
{
    public Guid Id { get; set; }
    public string Ticket { get; set; } = string.Empty;
    public double? FrameRate { get; set; }
}
