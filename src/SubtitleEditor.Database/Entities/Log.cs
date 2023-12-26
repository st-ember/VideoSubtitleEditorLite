using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SubtitleEditor.Core.Abstract;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Database.Entities.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SubtitleEditor.Database.Entities;

public class Log : EntityWithIdBase, ILog
{
    /// <summary>
    /// 操作 ID，理論上同一次操作產生的所有 Log 應該要擁有相同的操作 ID。
    /// </summary>
    [Required]
    [Column("_action_id", TypeName = "CHAR(36)")]
    public Guid ActionId { get; set; }

    /// <summary>
    /// 紀錄時間
    /// </summary>
    [Required]
    [Column("_time")]
    public DateTime Time { get; set; }

    /// <summary>
    /// 操作代碼
    /// </summary>
    [Required]
    [Column("_action", TypeName = "VARCHAR(256)")]
    [StringLength(256)]
    public string ActionText { get; set; }

    /// <summary>
    /// 該次操作是否成功
    /// </summary>
    [Required]
    [Column("_success", TypeName = "TINYINT(1)")]
    public bool Success { get; set; }

    /// <summary>
    /// 觸發操作的使用者 ID
    /// </summary>
    [Column("_user_id", TypeName = "CHAR(36)")]
    public Guid? UserId { get; set; }

    /// <summary>
    /// 紀錄觸發操作使用的要求文字，通常是來自前端的 JSON 格式要求。
    /// </summary>
    [Column("_request", TypeName = "TEXT")]
    public string? Request { get; set; }

    /// <summary>
    /// 紀錄操作完成後回傳的資訊，通常是指回傳給前端的 JSON 回應。
    /// </summary>
    [Column("_response", TypeName = "TEXT")]
    public string? Response { get; set; }

    /// <summary>
    /// 觸發操作的 IP
    /// </summary>
    [Column("_ip", TypeName = "VARCHAR(128)")]
    [StringLength(128)]
    public string? IPAddress { get; set; }

    /// <summary>
    /// 進行操作的目標物件可辨識的名稱。如果沒辦法取得名稱，至少該輸入物件 ID。
    /// </summary>
    [Column("_target", TypeName = "NVARCHAR(128)")]
    [StringLength(128)]
    public string? Target { get; set; }

    /// <summary>
    /// 被變更的欄位名稱。
    /// </summary>
    [Column("_field", TypeName = "TEXT")]
    public string? Field { get; set; }

    /// <summary>
    /// 變更前的資料。
    /// </summary>
    [Column("_before", TypeName = "TEXT")]
    public string? Before { get; set; }

    /// <summary>
    /// 變更後的資料。
    /// </summary>
    [Column("_after", TypeName = "TEXT")]
    public string? After { get; set; }

    /// <summary>
    /// 操作過程產生的訊息。
    /// </summary>
    [Column("_message", TypeName = "TEXT")]
    public string? Message { get; set; }

    /// <summary>
    /// 操作結果代碼。通常用在結果出錯的時候，使用比較簡明的方式表示錯誤原因。
    /// </summary>
    [Column("_code", TypeName = "VARCHAR(32)")]
    [StringLength(32)]
    public string? Code { get; set; }

    /// <summary>
    /// 操作過程接到的例外文字內容。
    /// </summary>
    [Column("_exception", TypeName = "TEXT")]
    public string? Exception { get; set; }

    /// <summary>
    /// 操作過程接到的例外中內部例外的文字內容。
    /// </summary>
    [Column("_inner_exception", TypeName = "TEXT")]
    public string? InnerException { get; set; }

    /// <summary>
    /// 這筆紀錄是否是該次操作最外層、最主要的那筆。一次操作常常會產生多筆紀錄，但只有最後一筆是主要紀錄。
    /// </summary>
    [Required]
    [Column("_primary", TypeName = "TINYINT(1)")]
    public bool Primary { get; set; }

    public Log()
    {
        Id = Guid.NewGuid();
        Time = DateTime.Now;
        ActionText = "";
    }

    public Log SetAction(SystemAction action)
    {
        ActionText = action.ToString();
        return this;
    }
}