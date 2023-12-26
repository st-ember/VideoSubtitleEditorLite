using SubtitleEditor.Infrastructure.Models.SystemOption;

namespace SubtitleEditor.Web.Models.Options;

public class ListOptionModel
{
    public SystemOptionModel[] Items { get; set; } = Array.Empty<SystemOptionModel>();
    public bool AsrAccess { get; set; } = false;
}
