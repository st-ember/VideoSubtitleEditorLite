using SubtitleEditor.Database.Entities;
using SubtitleEditor.Infrastructure.Models.UserGroup;

namespace SubtitleEditor.Infrastructure.Helpers;

public static class UserGroupDataHelper
{
    public static UserGroupData ToUserGroupData(this UserGroup entity)
    {
        return new UserGroupData
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            GroupType = entity.GroupType,
            Permission = entity.Permission
        };
    }
}