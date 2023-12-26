using Microsoft.EntityFrameworkCore;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Database;
using SubtitleEditor.Infrastructure.Models.Permission;
using SubtitleEditor.Infrastructure.Models.UserGroup;
using SubtitleEditor.Infrastructure.Services;

namespace SubtitleEditor.Infrastructure.ServiceImplements;

public class PermissionService : IPermissionService
{
    private readonly EditorContext _database;
    private readonly ICacheService _cacheService;
    private readonly IAccountService _accountService;
    private readonly ILogService _logService;
    private readonly IActivationService _activationService;

    private const string _cache_key = "permission-cache-key-";

    public PermissionService(
        EditorContext database,
        ICacheService cacheService,
        IAccountService accountService,
        ILogService logService,
        IActivationService activationService
        )
    {
        _database = database;
        _cacheService = cacheService;
        _accountService = accountService;
        _logService = logService;
        _activationService = activationService;
    }

    public async Task<PermissionContext> GetLoginUserPermissionAsync()
    {
        var userId = _accountService.GetLoginUserId();
        _logService.Target = userId.HasValue ? userId.Value.ToString() : string.Empty;
        return userId.HasValue ? await GetUserPermissionAsync(userId.Value) : new PermissionContext(default);
    }

    public async Task<PermissionContext> GetUserPermissionAsync(Guid id)
    {
        var permissionContext = await _cacheService.GetOrCreateAsync($"{_cache_key}{id}", async () =>
        {
            var userExists = await _database.Users
                .AsNoTracking()
                .AnyAsync(e => e.Id == id && e.Status == UserStatus.Enabled);

            if (!userExists)
            {
                return new PermissionContext(id);
            }

            var userGroups = await _database.UserGroupLinks
                .AsNoTracking()
                .Include(l => l.UserGroup)
                .Where(l => l.UserId == id)
                .Select(l => l.UserGroup)
                .ToArrayAsync();

            var userGroupDatas = userGroups
                .Select(e => UserGroupData.From(e))
                .ToArray();

            return new PermissionContext(id, userGroupDatas.Select(o => o.GroupType), userGroupDatas.SelectMany(o => o.GetPermissions()));
        });

        if (!await _activationService.IsActivatedAsync())
        {
            var actions = new List<SystemAction>()
            {
                SystemAction.Login,
                SystemAction.Logout,
                SystemAction.RenewPassword,
                SystemAction.GetSelfModifyData,
                SystemAction.GetSelfModifyGroupData,
                SystemAction.SelfUpdateUser,
                SystemAction.SelfUpdateUserPassword,
                SystemAction.SaveSelfKeybinding,
                SystemAction.RecoverSelfKeybinding,
                SystemAction.SaveSelfOptions
            };

            if (permissionContext.Contains(SystemAction.ListLog))
            {
                actions.Add(SystemAction.ListLog);
            }

            if (permissionContext.Contains(SystemAction.ShowSystemStatusView))
            {
                actions.Add(SystemAction.ShowSystemStatusView);
            }

            return new PermissionContext(id, Array.Empty<PermissionType?>(), actions);
        }

        return permissionContext;
    }

    public void ClearUserPermissionCache(Guid id)
    {
        _cacheService.Remove($"{_cache_key}{id}");
    }

    public async Task ClearUserGroupPermissionCacheAsync(Guid id)
    {
        var userIds = await _database.UserGroupLinks
            .AsNoTracking()
            .Where(l => l.UserGroupId == id)
            .Select(l => l.UserId)
            .ToArrayAsync();

        foreach (var userId in userIds)
        {
            _cacheService.Remove($"{_cache_key}{userId}");
        }
    }
}
