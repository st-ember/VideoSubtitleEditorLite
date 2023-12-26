using SubtitleEditor.Database.Entities;
using SubtitleEditor.Infrastructure.ListAdeptors.OrderMap;

namespace SubtitleEditor.Web.Infrastructure.ListAdeptors.OrderMap;

public class UserListOrderMap
{
    public static readonly OrderMap<User> OrderFuncMap =
        new()
        {
            {
                "Account",
                (desc, query) => desc ? query.OrderByDescending(e => e.Account) : query.OrderBy(e => e.Account)
            },
            {
                "Name",
                (desc, query) => desc ? query.OrderByDescending(e => e.Name).ThenByDescending(e => e.Account) : query.OrderBy(e => e.Name).ThenBy(e => e.Account)
            },
            {
                "Title",
                (desc, query) => desc ? query.OrderByDescending(e => e.Title).ThenByDescending(e => e.Account) : query.OrderBy(e => e.Title).ThenBy(e => e.Account)
            },
            {
                "Telphone",
                (desc, query) => desc ? query.OrderByDescending(e => e.Telephone).ThenByDescending(e => e.Account) : query.OrderBy(e => e.Telephone).ThenBy(e => e.Account)
            },
            {
                "Email",
                (desc, query) => desc ? query.OrderByDescending(e => e.Email).ThenByDescending(e => e.Account) : query.OrderBy(e => e.Email).ThenBy(e => e.Account)
            },
            {
                "Create",
                (desc, query) => desc ? query.OrderByDescending(e => e.Create) : query.OrderBy(e => e.Create)
            }
        };
}
