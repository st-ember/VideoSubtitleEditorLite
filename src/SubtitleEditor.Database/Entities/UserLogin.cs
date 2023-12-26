using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SubtitleEditor.Database.Entities;

/// <summary>
/// 使用者登入紀錄
/// </summary>
public class UserLogin
{
    [Key]
    [Column("_id", TypeName = "CHAR(36)", Order = 1)]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 是否成功登入。
    /// </summary>
    [Required]
    [Column("_success", TypeName = "TINYINT(1)")]
    public bool Success { get; set; }

    /// <summary>
    /// 登入嘗試後產生的訊息。
    /// </summary>
    [Column("_message", TypeName = "TEXT")]
    public string? Message { get; set; }

    /// <summary>
    /// 登入嘗試所使用的 IP 位址。
    /// </summary>
    [Required]
    [Column("_ip", TypeName = "VARCHAR(128)")]
    [StringLength(128)]
    public string IPAddress { get; set; } = null!;

    /// <summary>
    /// 登入嘗試發生的時間。
    /// </summary>
    [Required]
    [Column("_time")]
    public DateTime Time { get; set; } = DateTime.Now;

    /// <summary>
    /// 登入階段的結束時間
    /// </summary>
    [Column("_due")]
    public DateTime? DueTime { get; set; }

    /// <summary>
    /// 此次登入嘗試所對應的操作 ID。
    /// </summary>
    [Required]
    [Column("_action_id", TypeName = "CHAR(36)")]
    public Guid ActionId { get; set; }

    [Required]
    [Column("_user_id", TypeName = "CHAR(36)")]
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;
}