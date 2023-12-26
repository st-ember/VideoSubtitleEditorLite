namespace SubtitleEditor.Infrastructure.Services;

public interface ICacheService
{
    bool ContainsKey(string key);

    TData? Get<TData>(string key);

    void Set<TData>(string key, TData value);

    void Set<TData>(string key, TimeSpan? lifeSpan, TData value);

    void Remove(string key);

    TData GetOrCreate<TData>(string key, Func<TData> func);

    TData GetOrCreate<TData>(string key, TimeSpan lifeSpan, Func<TData> func);

    TData GetOrCreate<TData>(string key, TimeOnly expireTime, Func<TData> func);

    TData GetOrCreate<TData>(string key, DateTime absoluteExporeTime, Func<TData> func);

    Task<TData> GetOrCreateAsync<TData>(string key, Func<Task<TData>> func);

    Task<TData> GetOrCreateAsync<TData>(string key, TimeSpan lifeSpan, Func<Task<TData>> func);

    Task<TData> GetOrCreateAsync<TData>(string key, TimeOnly expireTime, Func<Task<TData>> func);

    Task<TData> GetOrCreateAsync<TData>(string key, DateTime absoluteExporeTime, Func<Task<TData>> func);
}
