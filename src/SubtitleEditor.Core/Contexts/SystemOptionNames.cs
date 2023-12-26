using SubtitleEditor.Core.Attributes;

namespace SubtitleEditor.Core.Contexts;

/// <summary>
/// 系統參數名稱
/// </summary>
public static class SystemOptionNames
{
    /// <summary>
    /// ASR 服務的 API 網址。
    /// </summary>
    [Index(0)]
    public const string AsrUrl = "AsrUrl";

    /// <summary>
    /// ASR 服務的使用者帳號。
    /// </summary>
    [Index(1)]
    public const string AsrUser = "AsrUser";

    /// <summary>
    /// ASR 服務的密碼。
    /// </summary>
    [Index(2)]
    public const string AsrSecret = "AsrSecret";

    /// <summary>
    /// 網站的標題文字。
    /// </summary>
    [Index(3)]
    public const string SiteTitle = "SiteTitle";

    /// <summary>
    /// 密碼有效期間長度(天數)。
    /// </summary>
    [Index(4)]
    public const string PasswordExpireDays = "PasswordExpireDays";

    /// <summary>
    /// 變更密碼不得重複的次數。
    /// </summary>
    [Index(5)]
    public const string PasswordNoneRepeatCount = "PasswordNoneRepeatCount";

    /// <summary>
    /// 原始影音檔的容量上限。
    /// </summary>
    [Index(6)]
    public const string RawFileStorageLimit = "RawFileStorageLimit";

    /// <summary>
    /// 串流影音檔的容量上限。
    /// </summary>
    [Index(7)]
    public const string StreamFileStorageLimit = "StreamFileStorageLimit";

    /// <summary>
    /// Logo 檔案的 Ticket。
    /// </summary>
    [Index(8)]
    public const string LogoTicket = "LogoTicket";

    /// <summary>
    /// 啟動金鑰內容。
    /// </summary>
    public const string ActivationKey = "ActivationKey";

    /// <summary>
    /// 所有的系統參數名稱。
    /// </summary>
    public static string[] AllSystemOptionName => new string[]
    {
        AsrUrl,
        AsrUser,
        AsrSecret,
        SiteTitle,
        PasswordExpireDays,
        PasswordNoneRepeatCount,
        RawFileStorageLimit,
        StreamFileStorageLimit,
        LogoTicket,
        ActivationKey
    };

    /// <summary>
    /// 隱藏的系統參數名稱。
    /// </summary>
    public static string[] InvisibleSystemOptionName => new string[]
    {
        ActivationKey
    };

    /// <summary>
    /// 加密的系統參數名稱。
    /// </summary>
    public static string[] EncrypedSystemOptionName => new string[]
    {
        AsrSecret
    };
}
