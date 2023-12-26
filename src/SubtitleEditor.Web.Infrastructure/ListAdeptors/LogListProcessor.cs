using Microsoft.EntityFrameworkCore;
using SubtitleEditor.Core.Abstract;
using SubtitleEditor.Database.Entities;
using SubtitleEditor.Infrastructure.ListAdeptors;
using SubtitleEditor.Infrastructure.ListAdeptors.OrderMap;
using SubtitleEditor.Web.Infrastructure.ListAdeptors.OrderMap;
using SubtitleEditor.Web.Infrastructure.Models.Log;

namespace SubtitleEditor.Web.Infrastructure.ListAdeptors;

public class LogListProcessor : BasicListProcessor<Log, ILogListCondition>
{
    protected override OrderMap<Log> OrderFuncMap => LogListOrderMap.OrderFuncMap;

    protected override async Task<IQueryable<Log>> QueryEntityAsync()
    {
        var adoptedStart = Condition != null && DateTime.TryParse(Condition.Start, out DateTime start) ? start : DateTime.Today;
        var adoptedEnd = (Condition != null && DateTime.TryParse(Condition.End, out DateTime end) ? end : DateTime.Today).AddDays(1);
        if (adoptedStart > adoptedEnd)
        {
            adoptedStart = adoptedEnd.AddDays(-1);
        }

        var actions = Condition != null && !string.IsNullOrWhiteSpace(Condition.Actions) && Condition.Actions != "-1" ?
            Condition.Actions.Split(new char[] { ';', ',' }).Where(o => !string.IsNullOrWhiteSpace(o)).Distinct().ToArray() :
            Array.Empty<string>();
        var filterByAction = actions.Any();

        var filterByTarget = Condition != null && !string.IsNullOrWhiteSpace(Condition.Target);
        var adoptedTarget = filterByTarget ? Condition!.Target!.Trim() : "";

        var filterByIPAddress = Condition != null && !string.IsNullOrWhiteSpace(Condition.IPAddress);
        var adoptedIPAddress = filterByIPAddress ? Condition!.IPAddress!.Trim() : "";

        var filterBySuccess = Condition != null && Condition.IsActionSuccess.HasValue;
        var adoptedSuccess = filterBySuccess && Condition!.IsActionSuccess!.Value;

        var filterByUser = Condition != null && !string.IsNullOrWhiteSpace(Condition.User);
        var userIds = filterByUser ? await FilterUserAsync(Condition!.User) : Array.Empty<Guid>();

        return Database.Logs
            .AsNoTracking()
            .Where(e =>
                e.Primary &&
                adoptedStart <= e.Time && e.Time <= adoptedEnd &&
                (!filterByAction || actions.Contains(e.ActionText)) &&
                (!filterByTarget || e.Target == adoptedTarget) &&
                (!filterByIPAddress || e.IPAddress == adoptedIPAddress) &&
                (!filterBySuccess || e.Success == adoptedSuccess) &&
                (!filterByUser || e.UserId.HasValue && userIds.Contains(e.UserId.Value))
                );
    }

    protected override async Task<IWithId[]> RetrieveDataAsync()
    {
        var entities = await Database.Logs
            .AsNoTracking()
            .Where(e => Ids.Contains(e.Id))
            .ToArrayAsync();

        var primaryActionLogIds = entities.Select(e => e.ActionId).ToArray();
        var childLogs = await Database.Logs
            .AsNoTracking()
            .Where(e => primaryActionLogIds.Contains(e.ActionId) && !e.Primary)
            .ToArrayAsync();

        var list = new List<LogListPrimaryData>();
        foreach (var entity in entities)
        {
            var primaryData = LogListPrimaryData.From(entity);
            primaryData.Children = childLogs
                .Where(e => e.ActionId == entity.ActionId)
                .Select(e => LogListData.From(e))
                .OrderByDescending(m => m.Time)
                .ToArray();

            list.Add(primaryData);
        }

        return list.ToArray();
    }

    protected virtual async Task<Guid[]> FilterUserAsync(string? userKeyword)
    {
        if (string.IsNullOrWhiteSpace(userKeyword))
        {
            return Array.Empty<Guid>();
        }

        var userId = Guid.TryParse(userKeyword, out Guid id) ? (Guid?)id : null;
        if (userId.HasValue)
        {
            return new Guid[] { userId.Value };
        }

        var adoptedKeyword = userKeyword.Trim() ?? "";
        return await Database.Users
            .AsNoTracking()
            .Where(e =>
                EF.Functions.Like(e.Account, $"%{adoptedKeyword}%") ||
                e.Name != null && EF.Functions.Like(e.Name, $"%{adoptedKeyword}%") ||
                e.Title != null && EF.Functions.Like(e.Title, $"%{adoptedKeyword}%") ||
                e.Telephone != null && EF.Functions.Like(e.Telephone, $"%{adoptedKeyword}%") ||
                e.Email != null && EF.Functions.Like(e.Email, $"%{adoptedKeyword}%") ||
                e.Description != null && EF.Functions.Like(e.Description, $"%{adoptedKeyword}%")
            )
            .Select(e => e.Id)
            .ToArrayAsync();
    }
}