using Microsoft.EntityFrameworkCore;
using SubtitleEditor.Core.Abstract;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Core.Helpers;
using SubtitleEditor.Core.Models;
using SubtitleEditor.Database;
using SubtitleEditor.Database.Entities;
using SubtitleEditor.Infrastructure.Models.UserGroup;
using SubtitleEditor.Infrastructure.Services;

namespace SubtitleEditor.Infrastructure.ServiceImplements;

public class UserGroupService : IUserGroupService
{
    private readonly EditorContext _database;
    private readonly ILogService _logService;
    private readonly IPermissionService _permissionService;

    public UserGroupService(
        EditorContext database,
        ILogService logService,
        IPermissionService permissionService
        )
    {
        _database = database;
        _logService = logService;
        _permissionService = permissionService;
    }

    public async Task<UserGroup> GetAsync(Guid id)
    {
        _logService.Target = id.ToString();

        if (id == default)
        {
            throw new Exception("無效的 ID");
        }

        var userGroup = await _database.UserGroups
            .AsNoTracking()
            .Where(e => e.Id == id)
            .SingleOrDefaultAsync();

        return userGroup ?? throw new Exception("找不到指定的使用者群組。");
    }

    public async Task<UserGroup[]> ListAsync()
    {
        return await _database.UserGroups
            .AsNoTracking()
            .OrderBy(e => e.Name).ThenBy(e => e.Create)
            .ToArrayAsync();
    }

    public async Task<UserGroup[]> ListByUserAsync(Guid id)
    {
        return await _database.UserGroupLinks
            .AsNoTracking()
            .Include(l => l.UserGroup)
            .Where(l => l.UserId == id)
            .Select(l => l.UserGroup)
            .ToArrayAsync();
    }

    public ISimpleResult Check(IUserGroup userGroup)
    {
        if (string.IsNullOrEmpty(userGroup.Name))
        {
            return SimpleResult.IsFailed("名稱不可空白。");
        }

        return SimpleResult.IsSuccess();
    }

    public async Task CreateAsync(IUserGroup userGroup)
    {
        _logService.Target = userGroup.Name;

        var entity = new UserGroup
        {
            Name = userGroup.Name,
            Description = userGroup.Description,
            GroupType = userGroup.GroupType,
            Permission = userGroup.Permission
        };

        _database.UserGroups.Add(entity);
        await _database.SaveChangesAsync();
        _database.Detach(entity);
    }

    public async Task UpdateAsync(IUserGroup userGroup)
    {
        _logService.Target = userGroup.Id.ToString();

        var entity = await _database.UserGroups
            .Where(e => e.Id == userGroup.Id)
            .SingleAsync();

        _logService.Target = entity.Name;

        var modified = false;

        var adoptedDescription = userGroup.Description.Trim();
        if (entity.Description != adoptedDescription)
        {
            AddInfo(entity.Name, nameof(UserGroup.Description), entity.Description, adoptedDescription);
            entity.Description = adoptedDescription;
            modified = true;
        }

        var adoptedGroupType = userGroup.GroupType;
        if (entity.GroupType != adoptedGroupType)
        {
            AddInfo(entity.Name, nameof(UserGroup.GroupType), entity.GroupType?.GetName() ?? "-", adoptedGroupType?.GetName() ?? "-");
            entity.GroupType = adoptedGroupType;
            modified = true;
        }

        var adoptedPermission = UserGroupData.From(userGroup).GetPermissions();
        var originalPermission = UserGroupData.From(entity).GetPermissions();
        if (adoptedPermission.Length != originalPermission.Length || adoptedPermission.Any(p => !originalPermission.Contains(p)) || originalPermission.Any(o => !adoptedPermission.Contains(o)))
        {
            foreach (var o in adoptedPermission.Where(p => !originalPermission.Contains(p)))
            {
                var log = _logService.GenerateNewLog();
                log.SetAction(SystemAction.AddPermissionToUserGroup);
                log.Target = o.ToString();
            }

            foreach (var o in originalPermission.Where(o => !adoptedPermission.Contains(o)))
            {
                var log = _logService.GenerateNewLog();
                log.SetAction(SystemAction.DeletePermissionFromUserGroup);
                log.Target = o.ToString();
            }

            entity.Permission = userGroup.Permission;
            modified = true;
        }

        var adoptedName = userGroup.Name.Trim();
        if (entity.Name != adoptedName)
        {
            AddInfo(entity.Name, nameof(UserGroup.Name), entity.Name, adoptedName);
            entity.Name = adoptedName;
            modified = true;
        }

        if (modified)
        {
            entity.Update = DateTime.Now;
            await _database.SaveChangesAsync();
            await _permissionService.ClearUserGroupPermissionCacheAsync(userGroup.Id);
        }

        _database.Detach(entity);
    }

    public async Task DuplicateAsync(Guid id)
    {
        _logService.Target = id.ToString();

        if (id == default)
        {
            throw new Exception("無效的 ID");
        }

        var entity = await _database.UserGroups
            .AsNoTracking()
            .Where(e => e.Id == id)
            .SingleAsync();

        entity.Name = $"{entity.Name} (複製)";

        await CreateAsync(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        if (!await _database.UserGroups.AnyAsync(e => e.Id == id))
        {
            throw new Exception("找不到指定的使用者群組。");
        }

        _database.Remove(new UserGroup { Id = id });
        await _database.SaveChangesAsync();
        await _permissionService.ClearUserGroupPermissionCacheAsync(id);
    }

    public async Task EnsureDefaultGroupAsync()
    {
        if (await _database.UserGroups.AnyAsync())
        {
            return;
        }

        var systemAdminGroup = new UserGroup
        {
            Name = "系統管理員",
            Description = "系統預設的系統管理員群組，此群組成員擁有系統內所有權限。",
            GroupType = PermissionType.SystemAdmin
        };

        var topicManagerGroup = new UserGroup
        {
            Name = "單集管理員",
            Description = "系統預設的單集管理員群組，此群組成員可以進入單集畫面，並獲得新增、編輯、刪除等所有單集管理權限。",
            GroupType = PermissionType.TopicManager
        };

        _database.UserGroups.Add(systemAdminGroup);
        _database.UserGroups.Add(topicManagerGroup);

        await _database.SaveChangesAsync();
        _database.Detach(systemAdminGroup);
        _database.Detach(topicManagerGroup);
    }

    protected virtual void AddInfo(string target, string field, string before = "", string after = "")
    {
        _logService.SystemInfo("", target, field, before, after);
    }
}