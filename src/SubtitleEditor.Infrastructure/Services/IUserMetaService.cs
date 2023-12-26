using SubtitleEditor.Infrastructure.Models.UserMeta;

namespace SubtitleEditor.Infrastructure.Services;

public interface IUserMetaService
{
    Task<UserMetaData?> GetUserMetaDataAsync(Guid id, string key);
    Task<UserMetaData[]> GetUserMetaDataAsync(Guid id, IEnumerable<string> keys);
    Task<T?> GetUserMetaDataAsync<T>(Guid id, string key, T? defaultValue);
    Task<T?> GetUserMetaDataAsync<T>(Guid id, string key);
    Task CreateOrUpdateUserMetaAsync<T>(Guid userId, string key, T data);
    Task CreateOrUpdateUserMetaAsync(UserMetaData userMetaData);
}
