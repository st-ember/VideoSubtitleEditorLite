namespace SubtitleEditor.Web.Infrastructure.Models.Session;

public class UserSession
{
    public Guid UserId { get; }
    public string[] ConnectionIds { get; } = Array.Empty<string>();

    public UserSession(Guid userId, string connectionId)
    {
        UserId = userId;
        ConnectionIds = new string[] { connectionId };
    }

    public UserSession(Guid userId, IEnumerable<string> connectionIds)
    {
        UserId = userId;
        ConnectionIds = connectionIds.Distinct().ToArray();
    }

    public UserSession DuplicateAndAdd(string connectionId)
    {
        return new UserSession(UserId, ConnectionIds.Concat(new string[] { connectionId }));
    }

    public UserSession DuplicateAndRemove(string connectionId)
    {
        return new UserSession(UserId, ConnectionIds.Where(o => o != connectionId));
    }

    public bool IsLastOneConnection(string connectionId)
    {
        return ConnectionIds.Length == 1 && ConnectionIds[0] == connectionId;
    }
}
