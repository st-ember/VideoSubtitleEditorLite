using SubtitleEditor.Core.Contexts;

namespace SubtitleEditor.Web.Infrastructure.Models.UserMeta;

/// <summary>
/// Keybinding，記錄了特定一組快速鍵的按法。
/// </summary>
public class UserKeybinding
{
    /// <summary>
    /// 此快速鍵代表的操作名稱。
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// 快速鍵包含的所有一般案鍵代碼。因為是要給前端用的快速鍵，所以代碼也是使用瀏覽器上的清單來記錄。可使用的快速鍵有一些限制，Esc、Meta 鍵不可使用。
    /// 代碼可參考文件：<see href="https://developer.mozilla.org/en-US/docs/Web/API/UI_Events/Keyboard_event_key_values"/>
    /// </summary>
    public string[] KeyCodes { get; set; } = Array.Empty<string>();

    /// <summary>
    /// 此快速鍵是否需要先按下 Ctrl 鍵來觸發。
    /// </summary>
    public bool WithCtrl { get; set; }

    /// <summary>
    /// 此快速鍵是否需要先按下 Shift 鍵來觸發。
    /// </summary>
    public bool WithShift { get; set; }

    /// <summary>
    /// 此快速鍵是否需要先按下 Alt 鍵來觸發。
    /// </summary>
    public bool WithAlt { get; set; }

    public UserKeybinding() { }

    public UserKeybinding(UserKeybindingActions action)
    {
        Action = action.ToString();
    }
}
