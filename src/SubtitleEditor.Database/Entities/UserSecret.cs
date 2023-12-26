using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SubtitleEditor.Core.Contexts;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SubtitleEditor.Database.Entities;

/// <summary>
/// 使用者密碼
/// </summary>
public class UserSecret
{
    [Key]
    [Column("_id", TypeName = "CHAR(36)", Order = 1)]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 加密後的密碼。
    /// </summary>
    [Required]
    [Column("_value", TypeName = "TEXT")]
    public string Value { get; set; } = null!;

    [Required]
    [Column("_reason", TypeName = "VARCHAR(32)")]
    public UserSecretCreateReason Reason { get; set; }

    /// <summary>
    /// 密碼被建立的時間。
    /// </summary>
    [Required]
    [Column("_create")]
    public DateTime Create { get; set; } = DateTime.Now;

    /// <summary>
    /// 密碼的有效時間
    /// </summary>
    [Required]
    [Column("_valid_to")]
    public DateTime ValidTo { get; set; } = DateTime.Now;

    [Required]
    [Column("_user_id", TypeName = "CHAR(36)")]
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;
}