using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SubtitleEditor.Database.Entities;

/// <summary>
/// 使用者與群組的多對多關聯
/// </summary>
public class UserGroupLink
{
    [Key]
    [Required]
    [Column("_user_id", TypeName = "CHAR(36)")]
    public Guid UserId { get; set; }

    [Key]
    [Required]
    [Column("_user_group_id", TypeName = "CHAR(36)")]
    public Guid UserGroupId { get; set; }

    public User User { get; set; } = null!;
    public UserGroup UserGroup { get; set; } = null!;
}