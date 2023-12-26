using SubtitleEditor.Core.Attributes;

namespace SubtitleEditor.Core.Contexts;

/// <summary>
/// 使用者狀態
/// </summary>
public enum UserStatus
{
    /// <summary>
    /// 正常
    /// </summary>
    [Name("正常")]
    Enabled,

    /// <summary>
    /// 停用
    /// </summary>
    [Name("停用")]
    Disabled,

    /// <summary>
    /// 移除
    /// </summary>
    [Name("移除")]
    Removed
}
