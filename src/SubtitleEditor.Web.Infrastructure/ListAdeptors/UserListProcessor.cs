using Microsoft.EntityFrameworkCore;
using SubtitleEditor.Core.Abstract;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Core.Helpers;
using SubtitleEditor.Database.Entities;
using SubtitleEditor.Infrastructure.ListAdeptors;
using SubtitleEditor.Infrastructure.ListAdeptors.OrderMap;
using SubtitleEditor.Web.Infrastructure.ListAdeptors.OrderMap;
using SubtitleEditor.Web.Infrastructure.Models.User;

namespace SubtitleEditor.Web.Infrastructure.ListAdeptors;

public class UserListProcessor : BasicListProcessor<User, IUserListCondition>
{
    protected override OrderMap<User> OrderFuncMap => UserListOrderMap.OrderFuncMap;

    protected override Task<IQueryable<User>> QueryEntityAsync()
    {
        var filterByKeyword = Condition != null && !string.IsNullOrWhiteSpace(Condition.Keyword);
        var adoptedKeyword = filterByKeyword ? Condition!.Keyword!.Trim() : "";

        var filterByStatus = Condition != null && !string.IsNullOrWhiteSpace(Condition.Status) && Condition.Status != "-1";
        var adoptedStatus = filterByStatus ? UserStatus.Enabled.Parse(Condition!.Status) : UserStatus.Enabled;

        return Task.FromResult(Database.Users
            .AsNoTracking()
            .Where(e =>
                e.Status != UserStatus.Removed &&
                (!filterByKeyword ||
                    EF.Functions.Like(e.Account, $"%{adoptedKeyword}%") ||
                    e.Name != null && EF.Functions.Like(e.Name, $"%{adoptedKeyword}%") ||
                    e.Title != null && EF.Functions.Like(e.Title, $"%{adoptedKeyword}%") ||
                    e.Telephone != null && EF.Functions.Like(e.Telephone, $"%{adoptedKeyword}%") ||
                    e.Email != null && EF.Functions.Like(e.Email, $"%{adoptedKeyword}%") ||
                    e.Description != null && EF.Functions.Like(e.Description, $"%{adoptedKeyword}%")
                    ) &&
                (!filterByStatus || e.Status == adoptedStatus)
                ));
    }

    protected override async Task<IWithId[]> RetrieveDataAsync()
    {
        var entities = await Database.Users
            .AsNoTracking()
            .Include(e => e.UserGroupLinks).ThenInclude(l => l.UserGroup)
            .Where(e => Ids.Contains(e.Id))
            .ToArrayAsync();

        var list = new List<UserListData>();
        foreach (var e in entities)
        {
            var model = new UserListData
            {
                Id = e.Id,
                Account = e.Account,
                Name = e.Name,
                Title = e.Title,
                Telephone = e.Telephone,
                Email = e.Email,
                Description = e.Description,
                Status = e.Status,
                UserGroups = e.UserGroupLinks.Select(l => l.UserGroup.Name).Distinct().ToArray(),
                Update = e.Update.ToString("yyyy/MM/dd HH:mm:ss"),
                Create = e.Create.ToString("yyyy/MM/dd HH:mm:ss")
            };

            list.Add(model);
        }

        return list.ToArray();
    }
}