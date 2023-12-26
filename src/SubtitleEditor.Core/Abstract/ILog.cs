namespace SubtitleEditor.Core.Abstract;

/// <summary>
/// 系統紀錄
/// </summary>
public interface ILog
{
    /// <summary>
    /// ID
    /// </summary>
    Guid Id { get; set; }

    /// <summary>
    /// 操作 ID
    /// </summary>
    Guid ActionId { get; set; }

    /// <summary>
    /// 時間
    /// </summary>
    DateTime Time { get; set; }

    /// <summary>
    /// 操作名稱
    /// </summary>
    string ActionText { get; set; }

    /// <summary>
    /// 是否成功
    /// </summary>
    bool Success { get; set; }

    /// <summary>
    /// 操作使用者 ID
    /// </summary>
    Guid? UserId { get; set; }

    /// <summary>
    /// 操作使用者 IP
    /// </summary>
    string? IPAddress { get; set; }

    /// <summary>
    /// 操作目標
    /// </summary>
    string? Target { get; set; }

    /// <summary>
    /// 操作目標欄位
    /// </summary>
    string? Field { get; set; }

    /// <summary>
    /// 操作前資料
    /// </summary>
    string? Before { get; set; }

    /// <summary>
    /// 操作後資料
    /// </summary>
    string? After { get; set; }

    /// <summary>
    /// 操作產生的訊息
    /// </summary>
    string? Message { get; set; }

    /// <summary>
    /// 紀錄事件的簡易代碼
    /// </summary>
    string? Code { get; set; }

    /// <summary>
    /// 例外內容
    /// </summary>
    string? Exception { get; set; }

    /// <summary>
    /// 內部例外內容
    /// </summary>
    string? InnerException { get; set; }

    /// <summary>
    /// 此項目是否為主要紀錄
    /// </summary>
    bool Primary { get; set; }
}