using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SubtitleEditor.Core.Abstract;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SubtitleEditor.Database.Entities;

/// <summary>
/// 系統參數
/// </summary>
public class SystemOption : ISystemOption
{
    /// <summary>
    /// 名稱
    /// </summary>
    [Key]
    [Required]
    [Column("_name", TypeName = "NVARCHAR(256)", Order = 1)]
    [StringLength(256)]
    public string Name { get; set; } = null!;

    [Key]
    [Required]
    [Column("_create", Order = 2)]
    public DateTime Create { get; set; } = DateTime.Now;

    /// <summary>
    /// 值。
    /// </summary>
    [Column("_content", TypeName = "TEXT")]
    public string? Content { get; set; }

    /// <summary>
    /// 說明文字。
    /// </summary>
    [Column("_desc", TypeName = "TEXT")]
    public string? Description { get; set; }

    /// <summary>
    /// 是否在系統參數管理畫面中顯示。
    /// </summary>
    [Required]
    [Column("_visible", TypeName = "TINYINT(1)")]
    public bool Visible { get; set; }

    /// <summary>
    /// 此資料是否需要加密。
    /// </summary>
    [Required]
    [Column("_encrypted", TypeName = "TINYINT(1)")]
    public bool Encrypted { get; set; }

    /// <summary>
    /// 資料類型。
    /// </summary>
    [Required]
    [Column("_type", TypeName = "NVARCHAR(256)")]
    public string? Type { get; set; }
}
