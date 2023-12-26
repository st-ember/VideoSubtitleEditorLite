using SubtitleEditor.Infrastructure.Services;
using SubtitleEditor.Web.Infrastructure.Models.Session;
using SubtitleEditor.Web.Infrastructure.Services;
using System.Collections.Concurrent;

namespace SubtitleEditor.Web.Infrastructure.ServiceImplements;

public class SessionService : ISessionService
{
    private readonly ConcurrentDictionary<Guid, UserSession> _userSessions = new();

    public string[] ListConnectionId(Guid userId)
    {
        return _userSessions.TryGetValue(userId, out var session) ? session.ConnectionIds : Array.Empty<string>();
    }

    public void Connect(Guid userId, string connectionId)
    {
        _userSessions.AddOrUpdate(userId, new UserSession(userId, connectionId), (_, existedSession) => existedSession.DuplicateAndAdd(connectionId));
    }

    public void Disconnect(Guid userId, string connectionId)
    {
        if (_userSessions.ContainsKey(userId) && _userSessions.TryGetValue(userId, out var session))
        {
            if (session.IsLastOneConnection(connectionId))
            {
                _retry(() => _userSessions.TryRemove(userId, out _), "清除 Session");
            }
            else
            {
                _retry(() => _userSessions.TryUpdate(userId, session.DuplicateAndRemove(connectionId), session), "刪除 Connection");
            }
        }
    }

    public int GetCurrentSessionCount()
    {
        return _userSessions.Keys.Count;
    }

    private static void _retry(Func<bool> func, string name, int retryCount = 5)
    {
        var count = 0;
        while (count < retryCount)
        {
            if (func())
            {
                return;
            }

            count++;
        }

        throw new Exception($"處理 Session 時失敗，執行的作業為「{name}」。");
    }
}
