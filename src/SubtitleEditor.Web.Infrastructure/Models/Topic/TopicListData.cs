using SubtitleEditor.Core.Abstract;
using SubtitleEditor.Core.Contexts;

namespace SubtitleEditor.Web.Infrastructure.Models.Topic;

public class TopicListData : IWithId<Guid>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public long? AsrTaskId { get; set; }
    public long OriginalSize { get; set; }
    public long Size { get; set; }
    public double Length { get; set; }
    public string LengthText { get; set; } = string.Empty;
    public double ProcessTime { get; set; }
    public string ProcessTimeText { get; set; } = string.Empty;
    public TopicStatus TopicStatus { get; set; }
    public string TopicStatusText { get; set; } = string.Empty;
    public StreamMediaStatus MediaStatus { get; set; }
    public AsrMediaStatus AsrMediaStatus { get; set; }
    public ConvertMediaStatus ConvertMediaStatus { get; set; } 
    public CreatedOption CreatedOption { get; set; }
    public string AsrMediaStatusText { get; set; } = string.Empty;
    public string ConvertMediaStatusText { get; set; } = string.Empty;
    public string CreatedOptionText { get; set; } = string.Empty;
    public string? MediaError { get; set; }
    public int Progress { get; set; }
    public Guid CreatorId { get; set; }
    public string Create { get; set; } = string.Empty;
    public string Update { get; set; } = string.Empty;
    public string AsrAccess { get; set; } = string.Empty;

    public object GetId()
    {
        return Id;
    }

    public bool HasSameId(object id)
    {
        return (Guid)id == Id;
    }

    public Guid GetGenericId()
    {
        return Id;
    }

    public bool HasSameId(Guid id)
    {
        return id == Id;
    }
}
