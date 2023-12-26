using SubtitleEditor.Core.Attributes;

namespace SubtitleEditor.Core.Contexts;

/// <summary>
/// 權限類型
/// </summary>
public enum PermissionType
{
    /// <summary>
    /// 系統管理員
    /// </summary>
    [Name("系統管理員", "系統的最高權限，一旦擁有此權限則代表同時獲得其他所有權限。")]
    SystemAdmin,

    /// <summary>
    /// 系統選項管理
    /// </summary>
    [Name("系統選項管理", "可進入系統選項頁並修改系統選項。")]
    OptionManager,

    /// <summary>
    /// 系統紀錄
    /// </summary>
    [Name("系統紀錄檢視", "可以檢視系統紀錄畫面。")]
    LogViewer,

    /// <summary>
    /// 使用者管理
    /// </summary>
    [Name("使用者管理", "可以檢視使用者清單，並對使用者執行新增、修改與刪除操作。")]
    UserManager,

    /// <summary>
    /// 使用者群組管理
    /// </summary>
    [Name("使用者群組管理", "可以檢視使用者群組清單，並對使用者群組執行新增、修改與刪除操作。")]
    UserGroupManager,

    /// <summary>
    /// 單集管理
    /// </summary>
    [Name("單集管理", "可以使用單集清單與編輯器內的所有功能。")]
    TopicManager,

    /// <summary>
    /// 勘誤表管理
    /// </summary>
    [Name("勘誤表管理", "可以進入勘誤表畫面並執行新增與移除勘誤表項目的功能。")]
    FixBookManager
}
