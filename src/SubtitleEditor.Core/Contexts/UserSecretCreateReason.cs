using SubtitleEditor.Core.Attributes;

namespace SubtitleEditor.Core.Contexts;

/// <summary>
/// 使用者秘密更新的原因
/// </summary>
public enum UserSecretCreateReason
{
    /// <summary>
    /// 系統套用。是由系統運作所需，在執行過程中自動產生或變更的。
    /// </summary>
    [Name("系統套用")]
    SystemApply,

    /// <summary>
    /// 使用者變更。是帳號的擁有者自行變更的。
    /// </summary>
    [Name("使用者變更")]
    UserChange,

    /// <summary>
    /// 管理員變更。是由管理員代為操作所變更。
    /// </summary>
    [Name("管理員變更")]
    AdminChange,

    /// <summary>
    /// 其他原因
    /// </summary>
    [Name("其他原因")]
    Other
}
