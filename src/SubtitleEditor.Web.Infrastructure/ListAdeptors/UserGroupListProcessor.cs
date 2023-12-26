using Microsoft.EntityFrameworkCore;
using SubtitleEditor.Core.Abstract;
using SubtitleEditor.Core.Helpers;
using SubtitleEditor.Database.Entities;
using SubtitleEditor.Infrastructure.ListAdeptors;
using SubtitleEditor.Infrastructure.ListAdeptors.OrderMap;
using SubtitleEditor.Web.Infrastructure.ListAdeptors.OrderMap;
using SubtitleEditor.Web.Infrastructure.Models.UserGroup;

namespace SubtitleEditor.Web.Infrastructure.ListAdeptors;

public class UserGroupListProcessor : BasicListProcessor<UserGroup, IUserGroupListCondition>
{
    protected override OrderMap<UserGroup> OrderFuncMap => UserGroupListOrderMap.OrderFuncMap;

    protected override Task<IQueryable<UserGroup>> QueryEntityAsync()
    {
        var filterByKeyword = Condition != null && !string.IsNullOrWhiteSpace(Condition.Keyword);
        var adoptedKeyword = filterByKeyword ? Condition!.Keyword!.Trim() : "";

        return Task.FromResult(Database.UserGroups
            .AsNoTracking()
            .Where(e =>
                !filterByKeyword || EF.Functions.Like(e.Name, $"%{adoptedKeyword}%") || e.Description != null && EF.Functions.Like(e.Description, $"%{adoptedKeyword}%")
                ));
    }

    protected override async Task<IWithId[]> RetrieveDataAsync()
    {
        var entities = await Database.UserGroups
            .AsNoTracking()
            .Include(e => e.UserGroupLinks)
            .Where(e => Ids.Contains(e.Id))
            .ToArrayAsync();

        var list = new List<UserGroupListData>();
        foreach (var e in entities)
        {
            var model = new UserGroupListData
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description,
                GroupType = e.GroupType.HasValue ? e.GroupType.Value.GetName() : "自訂",
                UserCount = e.UserGroupLinks.Count,
                Update = e.Update.ToString("yyyy/MM/dd HH:mm:ss"),
                Create = e.Create.ToString("yyyy/MM/dd HH:mm:ss")
            };

            list.Add(model);
        }

        return list.ToArray();
    }
}