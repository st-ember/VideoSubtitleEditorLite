using Microsoft.EntityFrameworkCore;
using SubtitleEditor.Database;
using SubtitleEditor.Database.Entities;
using SubtitleEditor.Infrastructure.Helpers;
using SubtitleEditor.Infrastructure.Models.UserMeta;
using SubtitleEditor.Infrastructure.Services;

namespace SubtitleEditor.Infrastructure.ServiceImplements;

public class UserMetaService : IUserMetaService
{
    private readonly EditorContext _database;
    private readonly ILogService _logService;

    public UserMetaService(
        EditorContext database,
        ILogService logService
        )
    {
        _database = database;
        _logService = logService;
    }

    public virtual async Task<UserMetaData?> GetUserMetaDataAsync(Guid id, string key)
    {
        _logService.Target = _getTarget(id, key);

        var adoptedKey = !string.IsNullOrWhiteSpace(key) ? key.Trim().ToUpper() : "";
        var userMeta = await _database.UserMetas
            .AsNoTracking()
            .Where(e => e.UserId == id && e.Key == adoptedKey)
            .FirstOrDefaultAsync();

        return userMeta?.ToUserMetaData();
    }

    public virtual async Task<UserMetaData[]> GetUserMetaDataAsync(Guid id, IEnumerable<string> keys)
    {
        var userMetaDatas = new List<UserMetaData?>();
        foreach (var key in keys)
        {
            _logService.Target = _getTarget(id, key);

            var adoptedKey = !string.IsNullOrWhiteSpace(key) ? key.Trim().ToUpper() : "";
            var userMeta = await _database.UserMetas
                .AsNoTracking()
                .Where(e => e.UserId == id && e.Key == adoptedKey)
                .FirstOrDefaultAsync();

            userMetaDatas.Add(userMeta?.ToUserMetaData());
        }

        return userMetaDatas.Where(m => m != null).Cast<UserMetaData>().ToArray();
    }

    public virtual async Task<T?> GetUserMetaDataAsync<T>(Guid id, string key, T? defaultValue)
    {
        var userMetaData = await GetUserMetaDataAsync(id, key);
        return userMetaData != null ? userMetaData.GetData(defaultValue) : default;
    }

    public virtual Task<T?> GetUserMetaDataAsync<T>(Guid id, string key)
    {
        return GetUserMetaDataAsync<T>(id, key, default);
    }

    public virtual Task CreateOrUpdateUserMetaAsync<T>(Guid userId, string key, T data)
    {
        var userMetaData = new UserMetaData
        {
            UserId = userId,
            Key = key
        };

        userMetaData.SetData(data);

        return CreateOrUpdateUserMetaAsync(userMetaData);
    }

    public virtual async Task CreateOrUpdateUserMetaAsync(UserMetaData userMetaData)
    {
        _logService.Target = _getTarget(userMetaData);

        _checkUserMetaData(userMetaData);

        var adoptedKey = userMetaData.Key.Trim().ToUpper();
        var userMeta = await _database.UserMetas
            .Where(e => e.UserId == userMetaData.UserId && e.Key == adoptedKey)
            .FirstOrDefaultAsync();

        if (userMeta != null)
        {
            await _updateAsync(userMetaData, userMeta);
        }
        else
        {
            await _createAsync(userMetaData);
        }
    }

    private static string _getTarget(UserMetaData userMetaData)
    {
        return _getTarget(userMetaData.UserId, userMetaData.Key);
    }

    private static string _getTarget(Guid userId, string key)
    {
        return $"{userId}-{key}";
    }

    private static void _checkUserMetaData(UserMetaData userMetaData)
    {
        if (string.IsNullOrWhiteSpace(userMetaData.Key))
        {
            throw new Exception("Key 不可以為 NULL 或空白");
        }
    }

    private async Task _createAsync(UserMetaData userMetaData)
    {
        var userMeta = new UserMeta
        {
            UserId = userMetaData.UserId,
            Key = userMetaData.Key.Trim().ToUpper(),
            Data = userMetaData.Data
        };

        _database.UserMetas.Add(userMeta);
        await _database.SaveChangesAsync();
        _database.Detach(userMeta);
    }

    private async Task _updateAsync(UserMetaData userMetaData, UserMeta userMeta)
    {
        if (userMeta == null)
        {
            throw new Exception("找不到使用者內部資料");
        }

        var edited = false;
        var target = _getTarget(userMeta.UserId, userMeta.Key);
        _logService.Target = target;

        if (userMeta.Data != userMetaData.Data)
        {
            AddInfo(target, nameof(userMeta.Data), userMeta.Data, userMetaData.Data);
            userMeta.Data = userMetaData.Data;
            edited = true;
        }

        if (edited)
        {
            userMeta.Update = DateTime.Now;
        }

        await _database.SaveChangesAsync();
        _database.Detach(userMeta);
    }

    protected virtual void AddInfo(string? target, string? field, string? before = "", string? after = "")
    {
        _logService.SystemInfo("", target, field, before, after);
    }
}
