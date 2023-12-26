using SubtitleEditor.Infrastructure.Models.SystemOption;

namespace SubtitleEditor.Web.Models.Options;

public class UpdateOptionModel
{
    public SystemOptionModel[] Items { get; set; } = Array.Empty<SystemOptionModel>();
}