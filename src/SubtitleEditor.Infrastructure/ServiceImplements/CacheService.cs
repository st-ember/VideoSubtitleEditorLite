using Microsoft.Extensions.Caching.Memory;
using SubtitleEditor.Infrastructure.Services;

namespace SubtitleEditor.Infrastructure.ServiceImplements;

public class CacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;

    public CacheService(
        IMemoryCache memoryCache
        )
    {
        _memoryCache = memoryCache;
    }

    public bool ContainsKey(string key)
    {
        return _memoryCache.TryGetValue(key, out var _);
    }

    public TData? Get<TData>(string key)
    {
        return _memoryCache.TryGetValue<TData>(key, out var value) ? value : default;
    }

    public void Set<TData>(string key, TData value)
    {
        Set(key, null, value);
    }

    public void Set<TData>(string key, TimeSpan? lifeSpan, TData value)
    {
        if (_memoryCache.TryGetValue(key, out var _))
        {
            _memoryCache.Remove(key);
        }

        if (lifeSpan.HasValue)
        {
            _memoryCache.Set(key, value, lifeSpan.Value);
        }
        else
        {
            _memoryCache.Set(key, value);
        }
    }

    public void Remove(string key)
    {
        _memoryCache.Remove(key);
    }

    public TData GetOrCreate<TData>(string key, Func<TData> func)
    {
        return _memoryCache.GetOrCreate(key, _ => func());
    }

    public TData GetOrCreate<TData>(string key, TimeSpan lifeSpan, Func<TData> func)
    {
        return _memoryCache.GetOrCreate(key, cacheEntry =>
        {
            cacheEntry.SetAbsoluteExpiration(lifeSpan);
            return func();
        });
    }

    public TData GetOrCreate<TData>(string key, TimeOnly expireTime, Func<TData> func)
    {
        return _memoryCache.GetOrCreate(key, cacheEntry =>
        {
            var now = DateTime.Now;
            var due = new DateTime(now.Year, now.Month, now.Day, expireTime.Hour, expireTime.Minute, expireTime.Second);
            if (due < now)
            {
                due = due.AddDays(1);
            }

            cacheEntry.SetAbsoluteExpiration(due - now);
            return func();
        });
    }

    public TData GetOrCreate<TData>(string key, DateTime absoluteExporeTime, Func<TData> func)
    {
        return _memoryCache.GetOrCreate(key, cacheEntry =>
        {
            cacheEntry.SetAbsoluteExpiration(DateTime.Now - absoluteExporeTime);
            return func();
        });
    }

    public Task<TData> GetOrCreateAsync<TData>(string key, Func<Task<TData>> func)
    {
        return _memoryCache.GetOrCreate(key, _ => func());
    }

    public Task<TData> GetOrCreateAsync<TData>(string key, TimeSpan lifeSpan, Func<Task<TData>> func)
    {
        return _memoryCache.GetOrCreate(key, cacheEntry =>
        {
            cacheEntry.SetAbsoluteExpiration(lifeSpan);
            return func();
        });
    }

    public Task<TData> GetOrCreateAsync<TData>(string key, TimeOnly expireTime, Func<Task<TData>> func)
    {
        return _memoryCache.GetOrCreate(key, cacheEntry =>
        {
            var now = DateTime.Now;
            var due = new DateTime(now.Year, now.Month, now.Day, expireTime.Hour, expireTime.Minute, expireTime.Second);
            if (due < now)
            {
                due = due.AddDays(1);
            }

            cacheEntry.SetAbsoluteExpiration(due - now);
            return func();
        });
    }

    public Task<TData> GetOrCreateAsync<TData>(string key, DateTime absoluteExporeTime, Func<Task<TData>> func)
    {
        return _memoryCache.GetOrCreate(key, cacheEntry =>
        {
            cacheEntry.SetAbsoluteExpiration(DateTime.Now - absoluteExporeTime);
            return func();
        });
    }
}