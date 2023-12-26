using SubtitleEditor.Core.Abstract;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Core.Helpers;

namespace SubtitleEditor.Infrastructure.Models.UserGroup;

public class UserGroupData : IUserGroup
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PermissionType? GroupType { get; set; }
    public string Permission { get; set; } = string.Empty;

    public string? GroupTypeText
    {
        get => GroupType.HasValue ? GroupType.Value.ToString() : null;
        set => GroupType = !string.IsNullOrWhiteSpace(value) && PermissionType.SystemAdmin.TryParse(value, out var permissionType) ? permissionType : null;
    }

    public SystemAction[] GetPermissions()
    {
        return !string.IsNullOrWhiteSpace(Permission) ?
            Permission.Split(';')
                .Select(o => SystemAction.Unknown.Parse(o))
                .Where(o => o != SystemAction.Unknown)
                .Distinct()
                .ToArray() : 
            Array.Empty<SystemAction>();
    }

    public void SetPermission(IEnumerable<SystemAction> actions)
    {
        Permission = actions.Any() ?
            string.Join(';', actions.Where(o => o != SystemAction.Unknown).Distinct().Select(o => o.ToString())) : string.Empty;
    }

    public static UserGroupData From(IUserGroup group)
    {
        return new UserGroupData
        {
            Id = group.Id,
            Name = group.Name,
            Description = group.Description,
            GroupType = group.GroupType,
            Permission = group.Permission
        };
    }
}