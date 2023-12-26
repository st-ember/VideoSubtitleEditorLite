using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Database.Entities.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SubtitleEditor.Database.Entities;

/// <summary>
/// 單集。原則上每一個 ASR 任務都會對應一個單集，但單集也有可能在沒有任務的情況下單獨存在。每個單集也都會關聯一個 <see cref="Entities.Media"/> 物件。
/// </summary>
public class Topic : EntityWithIdBase
{
    /// <summary>
    /// 單集名稱，通常是不包含副檔名的檔案名稱。
    /// </summary>
    [Required]
    [Column("_name", TypeName = "NVARCHAR(256)")]
    [StringLength(256)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 描述。
    /// </summary>
    [Column("_desc", TypeName = "TEXT")]
    public string? Description { get; set; }

    /// <summary>
    /// ASR 任務的 Id。
    /// </summary>
    [Column("_asr_id")]
    public long? AsrTaskId { get; set; }

    /// <summary>
    /// ASR 任務的 JSON 格式內容。
    /// </summary>
    [Column("_asr_task", TypeName = "TEXT")]
    public string? AsrTask { get; set; }

    /// <summary>
    /// 優先度，數字越高越優先。
    /// </summary>
    [Column("_priority")]
    public int Priority { get; set; }

    /// <summary>
    /// 單集狀態。
    /// </summary>
    [Required]
    [Column("_status")]
    public TopicStatus Status { get; set; }

    [Required]
    [Column("_create")]
    public DateTime Create { get; set; } = DateTime.Now;

    [Required]
    [Column("_update")]
    public DateTime Update { get; set; } = DateTime.Now;

    [Required]
    [Column("_creator", TypeName = "CHAR(36)")]
    public Guid CreatorId { get; set; }

    public virtual Media Media { get; set; } = null!;
    public virtual Subtitle? Subtitle { get; set; }
}
