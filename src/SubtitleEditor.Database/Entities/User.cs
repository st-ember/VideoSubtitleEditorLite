using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SubtitleEditor.Core.Abstract;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Database.Entities.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SubtitleEditor.Database.Entities;

public class User : EntityWithIdBase, IUser
{
    /// <summary>
    /// 登入用的帳號
    /// </summary>
    [Required]
    [Column("_account", TypeName = "NVARCHAR(256)")]
    [StringLength(256)]
    public string Account { get; set; } = null!;

    /// <summary>
    /// 加密後的密碼
    /// </summary>
    [Required]
    [Column("_password", TypeName = "TEXT")]
    public string Password { get; set; } = null!;

    /// <summary>
    /// 使用者名稱
    /// </summary>
    [Column("_name", TypeName = "NVARCHAR(256)")]
    [StringLength(256)]
    public string? Name { get; set; }

    /// <summary>
    /// 使用者職稱
    /// </summary>
    [Column("_title", TypeName = "NVARCHAR(256)")]
    [StringLength(256)]
    public string? Title { get; set; }

    /// <summary>
    /// 使用者電話
    /// </summary>
    [Column("_tel", TypeName = "NVARCHAR(256)")]
    [StringLength(256)]
    public string? Telephone { get; set; }

    /// <summary>
    /// 使用者信箱
    /// </summary>
    [Column("_email", TypeName = "NVARCHAR(256)")]
    [StringLength(256)]
    public string? Email { get; set; }

    /// <summary>
    /// 使用者帳號描述
    /// </summary>
    [Column("_desc", TypeName = "TEXT")]
    public string? Description { get; set; }

    [Required]
    [Column("_status", TypeName = "VARCHAR(16)")]
    public UserStatus Status { get; set; }

    [Required]
    [Column("_create")]
    public DateTime Create { get; set; } = DateTime.Now;

    [Required]
    [Column("_update")]
    public DateTime Update { get; set; } = DateTime.Now;

    public virtual ICollection<UserSecret> Secrets { get; set; } = new HashSet<UserSecret>();
    public virtual ICollection<UserLogin> Logins { get; set; } = new HashSet<UserLogin>();
    public virtual ICollection<UserMeta> Metas { get; set; } = new HashSet<UserMeta>();
    public virtual ICollection<UserGroupLink> UserGroupLinks { get; set; } = new HashSet<UserGroupLink>();
}