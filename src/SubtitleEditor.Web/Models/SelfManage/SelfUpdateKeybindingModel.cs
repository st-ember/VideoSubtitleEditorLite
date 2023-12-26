using SubtitleEditor.Web.Infrastructure.Models.UserMeta;

namespace SubtitleEditor.Web.Models.SelfManage;

public class SelfUpdateKeybindingModel
{
    public UserKeybinding[] Keybindings { get; set; } = Array.Empty<UserKeybinding>();
}
