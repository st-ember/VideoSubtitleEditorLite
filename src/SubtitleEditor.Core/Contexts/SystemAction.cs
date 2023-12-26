using SubtitleEditor.Core.Attributes;

namespace SubtitleEditor.Core.Contexts;

public enum SystemAction
{
    [Name("不明的操作")]
    Unknown,

    [IsPermissionAction("允許使用者使用登入畫面登入系統。", Category = "帳號")]
    [ActionMessage("帳號「$target」登入")]
    [Name("帳號登入")]
    Login,

    [ActionMessage("帳號「$target」登出")]
    [Name("帳號登出")]
    Logout,

    [ActionMessage("帳號「$target」定期更新密碼")]
    [Name("帳號定期更新密碼")]
    RenewPassword,

    [ActionMessage("自我取得登入的帳號「$target」資料")]
    [Name("取得登入帳號資料")]
    GetSelfModifyData,

    [ActionMessage("自我取得登入的帳號「$target」的群組資料")]
    [Name("取得登入帳號的群組資料")]
    GetSelfModifyGroupData,

    [IsPermissionAction("允許使用者自行更新帳號資料。", Category = "帳號")]
    [ActionMessage("帳號「$target」自我更新資料")]
    [Name("帳號自我更新資料")]
    SelfUpdateUser,

    [IsPermissionAction("允許使用者自行更新帳號的密碼。", Category = "帳號")]
    [ActionMessage("帳號「$target」自我更新密碼")]
    [Name("帳號自我更新密碼")]
    SelfUpdateUserPassword,

    [ActionMessage("取得登入的帳號「$target」權限清單")]
    [Name("取得登入的帳號權限清單")]
    GetSelfPermissionData,

    [Name("取得登入帳號的快速鍵設定")]
    GetSelfKeybinding,

    [IsPermissionAction("允許使用者修改字幕編輯器的快速鍵設定。", Category = "帳號")]
    [Name("設定登入帳號的快速鍵")]
    SaveSelfKeybinding,

    [IsPermissionAction("允許使用者重設字幕編輯器的快速鍵設定。", Category = "帳號")]
    [Name("重設登入帳號的快速鍵")]
    RecoverSelfKeybinding,

    [Name("取得登入帳號的喜好設定")]
    GetSelfOptions,

    [IsPermissionAction("允許使用者修改帳號的喜好設定。", Category = "帳號")]
    [Name("設定登入帳號的喜好設定")]
    SaveSelfOptions,

    [Name("列出 ASR Model")]
    ListAsrModel,

    #region [使用者管理]

    [IsPermissionAction(PermissionType.UserManager)]
    [Name("進入使用者管理畫面")]
    ShowUserListView,

    [Name("取得使用者")]
    GetUser,

    [Name("列出使用者")]
    ListUser,

    [ActionMessage("取得使用者是否存在「$target」")]
    [Name("取得使用者是否存在")]
    IsAccountExist,

    //[IsPermissionAction(PermissionType.UserManager)]
    //[Name("列出使用者登入紀錄")]
    //ListLoginRecord,

    [IsPermissionAction(PermissionType.UserManager)]
    [ActionMessage("建立使用者「$target」")]
    [Name("建立使用者")]
    CreateUser,

    [IsPermissionAction(PermissionType.UserManager)]
    [ActionMessage("修改使用者「$target」的「$field」從「$before」改為「$after」")]
    [Name("修改使用者")]
    UpdateUser,

    [IsPermissionAction(PermissionType.UserManager)]
    [ActionMessage("修改使用者「$target」的狀態從「$before」改為「$after」")]
    [Name("修改使用者狀態")]
    UpdateUserStatus,

    [IsPermissionAction(PermissionType.UserManager)]
    [ActionMessage("修改使用者「$target」的密碼")]
    [Name("修改使用者密碼")]
    UpdatePassword,

    [IsPermissionAction(PermissionType.UserManager)]
    [ActionMessage("更新使用者的權限群組")]
    [Name("更新使用者的權限群組")]
    UpdateUsersUserGroup,

    [ActionMessage("將使用者「$target」加入群組「$field」")]
    [Name("將使用者加入群組")]
    AddUserToUserGroup,

    [ActionMessage("將使用者「$target」從群組「$field」中移除")]
    [Name("將使用者從群組中移除")]
    RemoveUserFromUserGroup,

    [IsPermissionAction(PermissionType.UserManager)]
    [ActionMessage("移除使用者「$target」")]
    [Name("移除使用者")]
    RemoveUser,

    #endregion

    #region [權限群組管理]

    [IsPermissionAction(PermissionType.UserGroupManager)]
    [Name("進入權限群組管理畫面")]
    ShowUserGroupListView,

    [Name("列出權限群組")]
    ListUserGroup,

    [ActionMessage("取得權限群組「$target」")]
    [Name("取得權限群組")]
    GetUserGroup,

    [IsPermissionAction(PermissionType.UserGroupManager)]
    [ActionMessage("建立權限群組「$target」")]
    [Name("建立權限群組")]
    CreateUserGroup,

    [IsPermissionAction(PermissionType.UserGroupManager)]
    [ActionMessage("修改權限群組「$target」的「$field」從「$before」改為「$after」")]
    [Name("修改權限群組")]
    UpdateUserGroup,

    [ActionMessage("將權限「$target」加入群組")]
    [Name("將權限加入群組")]
    AddPermissionToUserGroup,

    [ActionMessage("將權限「$target」從群組移除")]
    [Name("將權限從群組移除")]
    DeletePermissionFromUserGroup,

    [IsPermissionAction(PermissionType.UserGroupManager)]
    [ActionMessage("再製權限群組「$target」")]
    [Name("再製權限群組")]
    DuplicateUserGroup,

    [IsPermissionAction(PermissionType.UserGroupManager)]
    [ActionMessage("刪除權限群組「$target」")]
    [Name("刪除權限群組")]
    DeleteUserGroup,

    #endregion

    #region [系統選項管理]

    [IsPermissionAction(PermissionType.LogViewer)]
    [Name("列出系統紀錄")]
    ListLog,

    [IsPermissionAction(PermissionType.OptionManager)]
    [Name("列出系統選項")]
    ListOption,

    [IsPermissionAction(PermissionType.OptionManager)]
    [ActionMessage("將選項「$target」的「$field」從「$before」改為「$after」")]
    [Name("更新系統選項")]
    UpdateOptions,

    #endregion

    #region [單集與字幕管理]

    [IsPermissionAction(PermissionType.TopicManager)]
    [Name("進入單集管理畫面")]
    ShowTopicListView,

    [ActionMessage("取得單集「$target」的資訊")]
    [Name("取得單集資訊")]
    GetTopic,

    [ActionMessage("取得單集清單的特定項目「$target」資訊")]
    [Name("取得單集清單的特定項目資訊")]
    GetTopicListItem,

    [ActionMessage("取得單集「$target」的字幕")]
    [Name("取得單集字幕")]
    GetTopicSubtitle,

    [IsPermissionAction(PermissionType.TopicManager)]
    [ActionMessage("匯出單集「$target」的字幕")]
    [Name("匯出字幕")]
    ExportSubtitle,

    [IsPermissionAction(PermissionType.TopicManager)]
    [ActionMessage("匯出單集「$target」的逐字稿")]
    [Name("匯出逐字稿")]
    ExportTranscript,

    [IsPermissionAction(PermissionType.TopicManager)]
    [ActionMessage("下載單集「$target」的原始檔案")]
    [Name("下載原始檔案")]
    DownloadRawFile,

    [Name("列出單集")]
    ListTopic,

    [IsPermissionAction(PermissionType.TopicManager)]
    [ActionMessage("建立單集「$target」")]
    [Name("建立單集")]
    CreateTopic,

    [IsPermissionAction(PermissionType.TopicManager)]
    [ActionMessage("批次建立單集")]
    [Name("批次建立單集")]
    CreateTopics,

    [IsPermissionAction(PermissionType.TopicManager)]
    [ActionMessage("將單集「$target」的「$field」從「$before」改為「$after」")]
    [Name("修改單集")]
    UpdateTopic,

    [ActionMessage("將單集「$target」的字數限制從「$before」改為「$after」")]
    [Name("修改單集字數限制")]
    UpdateTopicWordLimit,

    [IsPermissionAction(PermissionType.TopicManager)]
    [Name("修改字幕")]
    UpdateTopicSubtitle,

    [IsPermissionAction(PermissionType.TopicManager)]
    [Name("修改逐字稿")]
    UpdateTopicTranscript,

    [Name("暫停單集")]
    PauseTopic,

    [Name("繼續單集")]
    ResumeTopic,

    [IsPermissionAction(PermissionType.TopicManager)]
    [ActionMessage("重新執行單集「$target」的流程")]
    [Name("重新執行單集流程")]
    ReExecuteTopic,

    [IsPermissionAction(PermissionType.TopicManager)]
    [ActionMessage("復原單集「$target」狀態回到正常")]
    [Name("復原單集狀態")]
    SetTopicStatusToNormal,

    [IsPermissionAction(PermissionType.TopicManager)]
    [ActionMessage("封存單集「$target」")]
    [Name("封存單集")]
    ArchiveTopic,

    [IsPermissionAction(PermissionType.TopicManager)]
    [ActionMessage("移除單集「$target」")]
    [Name("移除單集")]
    RemoveTopic,

    [IsPermissionAction(PermissionType.TopicManager)]
    [ActionMessage("重新執行單集「$target」的字幕辨識")]
    [Name("重新執行字幕辨識")]
    ReproduceSubtitle,

    [ActionMessage("重新載入單集「$target」的字幕辨識結果")]
    [Name("重新載入字幕辨識結果")]
    ReloadSubtitle,

    [IsPermissionAction(PermissionType.TopicManager)]
    [ActionMessage("重新建立單集「$target」的時間戳記")]
    [Name("重新建立時間戳記")]
    RecreateTimecode,

    [IsPermissionAction(PermissionType.TopicManager)]
    [ActionMessage("將單集「$target」還原回初始辨識結果")]
    [Name("還原回初始辨識結果")]
    RecoverToOriginal,

    [IsPermissionAction(PermissionType.TopicManager)]
    [ActionMessage("重新上傳單集「$target」的字幕")]
    [Name("重新上傳字幕")]
    ReuploadSubtitle,

    [IsPermissionAction(PermissionType.TopicManager)]
    [ActionMessage("重新上傳單集「$target」的逐字稿")]
    [Name("重新上傳逐字稿")]
    ReuploadTranscript,

    [IsPermissionAction(PermissionType.TopicManager)]
    [ActionMessage("執行單集「$target」的轉檔測試")]
    [Name("執行單集轉檔測試")]
    DoTopicConversionBenchmark,

    #endregion

    #region [勘誤表]

    [IsPermissionAction(PermissionType.FixBookManager)]
    [Name("進入勘誤表畫面")]
    ShowFixBookView,

    [Name("取得勘誤表")]
    GetFixBook,

    [IsPermissionAction(PermissionType.FixBookManager)]
    [Name("儲存勘誤表")]
    SaveFixBook,

    #endregion

    #region [系統狀態]

    [IsPermissionAction("允許使用者進入系統狀態畫面", Category = "系統狀態")]
    [Name("進入系統狀態畫面")]
    ShowSystemStatusView,

    [Name("取得系統狀態")]
    GetSystemStatus,

    [Name("設定系統啟用金鑰")]
    SetActivationKey,

    [Name("清除系統啟用金鑰")]
    ClearActivationKey,

    #endregion

    #region [背景工作]

    [Name("處理 ASR 工作")]
    ProcessAsr,

    [Name("處理影片轉串流工作")]
    ProcessStreamConvert,

    [Name("處理過期的封存單集工作")]
    ProcessClearTopic,

    #endregion
}
