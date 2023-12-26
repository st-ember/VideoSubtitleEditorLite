using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SubtitleEditor.Core.Abstract;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Database.Entities.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SubtitleEditor.Database.Entities;

/// <summary>
/// 使用者群組
/// </summary>
public class UserGroup : EntityWithIdBase, IUserGroup
{
    /// <summary>
    /// 群組名稱
    /// </summary>
    [Required]
    [Column("_name", TypeName = "NVARCHAR(256)")]
    [StringLength(256)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 說明
    /// </summary>
    [Required]
    [Column("_desc", TypeName = "TEXT")]
    public string Description { get; set; } = null!;

    /// <summary>
    /// 群組類型，內含一個權限類型資料，代表此群組自動獲得的權限類型。如果群組類型是 <see cref="PermissionType.SystemAdmin"/>，代表此群組擁有所有權限。
    /// </summary>
    [Column("_type", TypeName = "VARCHAR(32)")]
    public PermissionType? GroupType { get; set; }

    /// <summary>
    /// 操作清單字串，使用半形逗號分隔此群組允許的操作名稱。權限清單參考自 <see cref="SystemAction"/> 列舉。
    /// </summary>
    [Required]
    [Column("_permission", TypeName = "TEXT")]
    public string Permission { get; set; } = string.Empty;

    [Required]
    [Column("_create")]
    public DateTime Create { get; set; } = DateTime.Now;

    [Required]
    [Column("_update")]
    public DateTime Update { get; set; } = DateTime.Now;

    public virtual ICollection<UserGroupLink> UserGroupLinks { get; set; } = new HashSet<UserGroupLink>();
}