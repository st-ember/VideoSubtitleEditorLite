using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SubtitleEditor.Database.Entities.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SubtitleEditor.Database.Entities;

public class FixBook : EntityWithIdBase
{
    /// <summary>
    /// 適用的 Model 名稱，如維持 null 則表示所有 model 皆適用。
    /// </summary>
    [Column("_model", TypeName = "NVARCHAR(256)")]
    [StringLength(256)]
    public string? Model { get; set; }

    /// <summary>
    /// 原始字串。
    /// </summary>
    [Required]
    [Column("_original", TypeName = "NVARCHAR(256)")]
    [StringLength(256)]
    public string Original { get; set; } = string.Empty;

    /// <summary>
    /// 取代後的新字串。
    /// </summary>
    [Required]
    [Column("_correction", TypeName = "NVARCHAR(256)")]
    [StringLength(256)]
    public string Correction { get; set; } = string.Empty;

    [Required]
    [Column("_create")]
    public DateTime Create { get; set; } = DateTime.Now;
}
