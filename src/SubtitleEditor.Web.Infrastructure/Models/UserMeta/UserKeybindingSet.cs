using SubtitleEditor.Core.Contexts;

namespace SubtitleEditor.Web.Infrastructure.Models.UserMeta;

/// <summary>
/// Keybinding 設定組。
/// </summary>
public class UserKeybindingSet
{
    /// <summary>
    /// 此設定組所有的 Keybinding。
    /// </summary>
    public UserKeybinding[] Keybindings { get; set; } = Array.Empty<UserKeybinding>();

    /// <summary>
    /// 確保目前的 Keybindings 包含所有該有的設定，並去除意外加入的設定。
    /// </summary>
    public void Ensure()
    {
        Keybindings = CreateDefault().Keybindings
            .Select(keybinding => Keybindings.Where(o => o.Action == keybinding.Action).FirstOrDefault() ?? keybinding)
            .ToArray();
    }

    /// <summary>
    /// 從多筆 Keybinding 建立出設定組。
    /// </summary>
    /// <param name="keybindings"></param>
    /// <returns></returns>
    public static UserKeybindingSet From(IEnumerable<UserKeybinding> keybindings)
    {
        return new UserKeybindingSet
        {
            Keybindings = keybindings.ToArray()
        };
    }

    /// <summary>
    /// 建立一組預設的 Keybinding。
    /// </summary>
    public static UserKeybindingSet CreateDefault()
    {
        return new UserKeybindingSet()
        {
            Keybindings = new UserKeybinding[]
            {
                new UserKeybinding(UserKeybindingActions.FinishLine) { KeyCodes = new string[] { "Enter" } },
                new UserKeybinding(UserKeybindingActions.EditLine) { KeyCodes = new string[] { "E" }, WithCtrl = true },
                new UserKeybinding(UserKeybindingActions.DeleteLine) { KeyCodes = new string[] { "Delete" } },
                new UserKeybinding(UserKeybindingActions.Play) { KeyCodes = new string[] { "P" }, WithShift = true },
                new UserKeybinding(UserKeybindingActions.Pause) { KeyCodes = new string[] { "P" }, WithShift = true },
                new UserKeybinding(UserKeybindingActions.PlayPeriod) { KeyCodes = new string[] { "P" }, WithCtrl = true },
                new UserKeybinding(UserKeybindingActions.Split) { KeyCodes = new string[] { "Enter" }, WithShift = true },
                new UserKeybinding(UserKeybindingActions.Marge) { KeyCodes = new string[] { "M" }, WithCtrl = true },
                new UserKeybinding(UserKeybindingActions.MargePrev) { KeyCodes = new string[] { "Backspace" } },
                new UserKeybinding(UserKeybindingActions.MargeNext) {  KeyCodes = new string[] { "Delete" } },
                new UserKeybinding(UserKeybindingActions.EditPrev) { KeyCodes = new string[] { "ArrowUp" }, WithCtrl = true },
                new UserKeybinding(UserKeybindingActions.EditNext) { KeyCodes = new string[] { "ArrowDown" }, WithCtrl = true },
                new UserKeybinding(UserKeybindingActions.SelectPrev) { KeyCodes = new string[] { "ArrowUp" } },
                new UserKeybinding(UserKeybindingActions.SelectNext) { KeyCodes = new string[] { "ArrowDown" } },
                new UserKeybinding(UserKeybindingActions.AddSelectPrev) { KeyCodes = new string[] { "ArrowUp" }, WithShift = true },
                new UserKeybinding(UserKeybindingActions.AddSelectNext) { KeyCodes = new string[] { "ArrowDown" }, WithShift = true },
                new UserKeybinding(UserKeybindingActions.InsertBefore) { KeyCodes = new string[] { "ArrowUp" }, WithAlt = true },
                new UserKeybinding(UserKeybindingActions.InsertAfter) { KeyCodes = new string[] { "ArrowDown" }, WithAlt = true },
                new UserKeybinding(UserKeybindingActions.SelectAll) { KeyCodes = new string[] { "A" }, WithCtrl = true },
                new UserKeybinding(UserKeybindingActions.Search) { KeyCodes = new string[] { "F" }, WithCtrl = true },
                new UserKeybinding(UserKeybindingActions.ShiftTime) { KeyCodes = new string[] { "T" }, WithShift = true },
                new UserKeybinding(UserKeybindingActions.RecoverToOriginal) { KeyCodes = new string[] { "R" }, WithCtrl = true },
                new UserKeybinding(UserKeybindingActions.Undo) { KeyCodes = new string[] { "Z" }, WithCtrl = true },
                new UserKeybinding(UserKeybindingActions.Redo) { KeyCodes = new string[] { "Y" }, WithCtrl = true },
                new UserKeybinding(UserKeybindingActions.Save) { KeyCodes = new string[] { "S" }, WithCtrl = true },
                new UserKeybinding(UserKeybindingActions.QuickCreateLine) { KeyCodes = new string[] { "Space" } },
                new UserKeybinding(UserKeybindingActions.PrevSecond) { KeyCodes = new string[] { "ArrowLeft" }, WithCtrl = true },
                new UserKeybinding(UserKeybindingActions.NextSecond) { KeyCodes = new string[] { "ArrowRight" }, WithCtrl = true }
            }
        };
    }
}