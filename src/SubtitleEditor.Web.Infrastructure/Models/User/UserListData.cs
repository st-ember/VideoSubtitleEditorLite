using SubtitleEditor.Core.Abstract;
using SubtitleEditor.Core.Contexts;

namespace SubtitleEditor.Web.Infrastructure.Models.User;

public class UserListData : IWithId<Guid>
{
    public Guid Id { get; set; }
    public string Account { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Title { get; set; }
    public string? Telephone { get; set; }
    public string? Email { get; set; }
    public string? Description { get; set; }
    public UserStatus Status { get; set; }
    public string[] UserGroups { get; set; } = Array.Empty<string>();
    public string Update { get; set; } = string.Empty;
    public string Create { get; set; } = string.Empty;

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