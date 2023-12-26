using SubtitleEditor.Database.Entities;
using SubtitleEditor.Infrastructure.ListAdeptors.OrderMap;

namespace SubtitleEditor.Web.Infrastructure.ListAdeptors.OrderMap;

public class TopicListOrderMap
{
    public static readonly OrderMap<Topic> OrderFuncMap =
        new()
        {
            {
                "Name",
                (desc, query) => desc ? query.OrderByDescending(e => e.Name) : query.OrderBy(e => e.Name)
            },
            {
                "OriginalSize",
                (desc, query) => desc ? query.OrderByDescending(e => e.Media.OriginalSize) : query.OrderBy(e => e.Media.OriginalSize)
            },
            {
                "Size",
                (desc, query) => desc ? query.OrderByDescending(e => e.Media.Size) : query.OrderBy(e => e.Media.Size)
            },
            {
                "Length",
                (desc, query) => desc ? query.OrderByDescending(e => e.Media.Length) : query.OrderBy(e => e.Media.Length)
            },
            {
                "TopicStatus",
                (desc, query) => desc ? query.OrderByDescending(e => e.Status) : query.OrderBy(e => e.Status)
            },
            {
                "AsrMediaStatus",
                (desc, query) => desc ? query.OrderByDescending(e => e.Media.AsrStatus) : query.OrderBy(e => e.Media.AsrStatus)
            },
            {
                "ConvertMediaStatus",
                (desc, query) => desc ? query.OrderByDescending(e => e.Media.ConvertStatus) : query.OrderBy(e => e.Media.ConvertStatus)
            },
            {
                "Create",
                (desc, query) => desc ? query.OrderByDescending(e => e.Create) : query.OrderBy(e => e.Create)
            },
            {
                "Update",
                (desc, query) => desc ? query.OrderByDescending(e => e.Update) : query.OrderBy(e => e.Update)
            }
        };
}