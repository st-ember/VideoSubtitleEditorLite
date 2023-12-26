namespace SubtitleEditor.Web.Infrastructure.Services;

public interface ISessionService
{
    string[] ListConnectionId(Guid userId);
    void Connect(Guid userId, string connectionId);
    void Disconnect(Guid userId, string connectionId);
    int GetCurrentSessionCount();
}
