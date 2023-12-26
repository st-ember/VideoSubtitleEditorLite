using SubtitleEditor.Database.Entities;
using SubtitleEditor.Infrastructure.ListAdeptors.OrderMap;

namespace SubtitleEditor.Web.Infrastructure.ListAdeptors.OrderMap;

public class UserGroupListOrderMap
{
    public static readonly OrderMap<UserGroup> OrderFuncMap =
        new()
        {
            {
                "Name",
                (desc, query) => desc ? query.OrderByDescending(e => e.Name) : query.OrderBy(e => e.Name)
            },
            {
                "Update",
                (desc, query) => desc ? query.OrderByDescending(e => e.Update) : query.OrderBy(e => e.Update)
            },
            {
                "Create",
                (desc, query) => desc ? query.OrderByDescending(e => e.Create) : query.OrderBy(e => e.Create)
            }
        };
}