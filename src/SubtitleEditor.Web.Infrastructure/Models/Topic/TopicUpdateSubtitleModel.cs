using SubtitleEditor.Core.Models;

namespace SubtitleEditor.Web.Infrastructure.Models.Topic;

public class TopicUpdateSubtitleModel
{
    public Guid Id { get; set; }
    public SubtitleLine[] Lines { get; set; } = Array.Empty<SubtitleLine>();
    public SubtitleModifiedState[] ModifiedStates { get; set; } = Array.Empty<SubtitleModifiedState>();
}
