using SubtitleEditor.Core.Abstract;

namespace SubtitleEditor.Web.Infrastructure.Models.UserGroup;

public class UserGroupListData : IWithId<Guid>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string GroupType { get; set; } = string.Empty;
    public int UserCount { get; set; }
    public string Create { get; set; } = string.Empty;
    public string Update { get; set; } = string.Empty;

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