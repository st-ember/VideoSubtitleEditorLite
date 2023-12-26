using Microsoft.EntityFrameworkCore;
using SubtitleEditor.Core.Abstract;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Database;
using SubtitleEditor.Database.Entities;
using SubtitleEditor.Infrastructure.Models.SystemOption;
using SubtitleEditor.Infrastructure.Services;

namespace SubtitleEditor.Infrastructure.ServiceImplements;

public class DBSystemOptionService : ISystemOptionService
{
    private readonly EditorContext _database;
    private readonly ILogService _logService;
    private readonly IEncryptService _encryptService;
    private readonly ICacheService _cacheService;

    private const string _cache_key = "system_option_cache_";
    private const string _cache_key_list = "system_option_cache_list";

    public DBSystemOptionService(
        EditorContext database,
        ILogService logService,
        IEncryptService encryptService,
        ICacheService cacheService
        )
    {
        _database = database;
        _logService = logService;
        _encryptService = encryptService;
        _cacheService = cacheService;
    }

    public async Task<SystemOptionModel?> GetAsync(string name)
    {
        var context = await ListAsync(name);
        return context.FirstOrDefault();
    }

    public async Task<string?> GetContentAsync(string name)
    {
        var systemOption = await GetAsync(name);
        return systemOption?.Content;
    }

    public async Task<int?> GetIntAsync(string name)
    {
        var systemOption = await GetAsync(name);
        return systemOption?.ToInt();
    }

    public async Task<long?> GetLongAsync(string name)
    {
        var systemOption = await GetAsync(name);
        return systemOption?.ToLong();
    }

    public async Task<bool?> GetBooleanAsync(string name)
    {
        var systemOption = await GetAsync(name);
        return systemOption?.ToBoolean();
    }

    public Task<SystemOptionContext> ListAsync(params string[] names)
    {
        return _listAsync(names, visibled: null);
    }

    public Task<SystemOptionContext> ListAsync(IEnumerable<string> names)
    {
        return _listAsync(names, visibled: null);
    }

    public Task<SystemOptionContext> ListVisibledAsync(params string[] names)
    {
        return _listAsync(names, visibled: true);
    }

    public Task<SystemOptionContext> ListVisibledAsync(IEnumerable<string> names)
    {
        return _listAsync(names, visibled: true);
    }

    public async Task SetAsync(ISystemOption systemOptionModel)
    {
        _logService.Target = systemOptionModel.Name;

        var originalSystemOption = await _getEntityAsync(systemOptionModel.Name);
        var entity = _set(systemOptionModel, originalSystemOption);

        await _database.SaveChangesAsync();
        _database.Detach(entity);
        _resetCache();
    }

    public async Task RestoreToDefaultAsync(string name)
    {
        _logService.Target = name;

        var defaultEntity = await _database.SystemOptions
            .AsNoTracking()
            .Where(e => e.Name == name)
            .OrderBy(e => e.Create)
            .FirstOrDefaultAsync();

        if (defaultEntity != null)
        {
            await SetAsync(defaultEntity);
        }
        else
        {
            throw new KeyNotFoundException($"找不到名為 {name} 的系統參數。");
        }
    }

    public async Task InitializeSystemOptionsAsync(IEnumerable<SystemOptionModel> systemOptions)
    {
        var existNames = await _database.SystemOptions
            .AsNoTracking()
            .GroupBy(e => e.Name)
            .Select(e => e.Key)
            .ToArrayAsync();

        var createdEntities = new List<SystemOption>();
        foreach (var name in SystemOptionNames.AllSystemOptionName.Where(o => !existNames.Contains(o)))
        {
            var systemOption = systemOptions.Any(o => o.Name == name) ? systemOptions.First(o => o.Name == name) : null;
            var entity = new SystemOption
            {
                Name = name,
                Content = systemOption?.Content ?? string.Empty,
                Description = systemOption?.Description ?? string.Empty,
                Visible = !SystemOptionNames.InvisibleSystemOptionName.Contains(name),
                Encrypted = SystemOptionNames.EncrypedSystemOptionName.Contains(name),
                Type = systemOption?.Type
            };

            if (entity.Encrypted)
            {
                entity.Content = _encryptService.Encrypt(entity.Content);
            }

            _database.SystemOptions.Add(entity);
            createdEntities.Add(entity);
        }

        await _database.SaveChangesAsync();
        _database.Detach(entities: createdEntities);
    }

    private async Task<SystemOption?> _getEntityAsync(string name)
    {
        var entity = await _database.SystemOptions
            .AsNoTracking()
            .Where(e => e.Name == name)
            .OrderByDescending(e => e.Create)
            .FirstOrDefaultAsync();

        if (entity != null && entity.Encrypted)
        {
            entity.Content = _encryptService.Decrypt(entity.Content);
        }

        return entity;
    }

    private async Task<SystemOptionContext> _listAsync(IEnumerable<string>? names, bool? visibled)
    {
        var key = $"{_cache_key}{(names != null ? string.Join(",", names) : string.Empty)}{(visibled.HasValue ? visibled.Value.ToString() : string.Empty)}";
        if (_cacheService.ContainsKey(key))
        {
            return _cacheService.Get<SystemOptionContext>(key)!;
        }

        var array = await _database.SystemOptions
            .AsNoTracking()
            .Where(e =>
                (names == null || names.Count() == 0 || names.Contains(e.Name)) &&
                (!visibled.HasValue || visibled.Value == e.Visible)
            )
            .ToArrayAsync();

        foreach (var item in array.Where(e => e.Encrypted))
        {
            item.Content = _encryptService.Decrypt(item.Content);
        }

        var list = array
            .GroupBy(e => e.Name)
            .Select(g => g.OrderByDescending(e => e.Create).First())
            .Select(e => new { Index = Array.FindIndex(SystemOptionNames.AllSystemOptionName, o => o == e.Name), Model = new SystemOptionModel(e) })
            .OrderBy(o => o.Index)
            .Select(o => o.Model);

        var data = new SystemOptionContext(list);

        _cacheService.Set(key, data);
        _addKey(key);

        return data;
    }

    private SystemOption? _set(ISystemOption systemOption, SystemOption? originalSystemOption)
    {
        if (originalSystemOption?.Content != systemOption.Content)
        {
            var entity = new SystemOption
            {
                Name = systemOption.Name,
                Content = systemOption.Content,
                Visible = true
            };

            if (originalSystemOption != null)
            {
                entity.Description = originalSystemOption.Description;
                entity.Visible = originalSystemOption.Visible;
                entity.Encrypted = originalSystemOption.Encrypted;
            }

            if (entity.Encrypted)
            {
                entity.Content = _encryptService.Encrypt(entity.Content);
            }

            _logService.SystemInfo("", entity.Name, nameof(SystemOption.Content), originalSystemOption?.Content ?? "-", entity.Content ?? "-");

            _database.SystemOptions.Add(entity);

            return entity;
        }

        return null;
    }

    private void _addKey(string key)
    {
        var list = _cacheService.GetOrCreate(_cache_key_list, () => new List<string>());
        if (!list.Contains(key))
        {
            list.Add(key);
            _cacheService.Set(_cache_key_list, list.Distinct().ToList());
        }
    }

    private void _resetCache()
    {
        var list = _cacheService.GetOrCreate(_cache_key_list, () => new List<string>());
        if (list.Any())
        {
            foreach (var item in list)
            {
                _cacheService.Remove(item);
            }
            _cacheService.Set(_cache_key_list, new List<string>());
        }
    }
}