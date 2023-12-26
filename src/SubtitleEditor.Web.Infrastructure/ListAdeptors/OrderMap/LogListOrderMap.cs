using SubtitleEditor.Database.Entities;
using SubtitleEditor.Infrastructure.ListAdeptors.OrderMap;

namespace SubtitleEditor.Web.Infrastructure.ListAdeptors.OrderMap;

public class LogListOrderMap
{
    public static readonly OrderMap<Log> OrderFuncMap =
        new()
        {
            {
                "Time",
                (desc, query) => desc ? query.OrderByDescending(e => e.Time) : query.OrderBy(e => e.Time)
            }
        };
}