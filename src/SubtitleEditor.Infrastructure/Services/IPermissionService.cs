using SubtitleEditor.Infrastructure.Models.Permission;

namespace SubtitleEditor.Infrastructure.Services;

public interface IPermissionService
{
    Task<PermissionContext> GetLoginUserPermissionAsync();
    Task<PermissionContext> GetUserPermissionAsync(Guid id);
    void ClearUserPermissionCache(Guid id);
    Task ClearUserGroupPermissionCacheAsync(Guid id);
}
