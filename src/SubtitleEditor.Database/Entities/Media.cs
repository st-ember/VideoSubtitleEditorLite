using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Core.Helpers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SubtitleEditor.Database.Entities;

/// <summary>
/// 影音檔案資訊。在影片完成上傳後便會建立此物件，並等待 ASR 與後續 FFMpeg 的轉檔。
/// </summary>
public class Media
{
    [Key]
    [Column("_topic_id", TypeName = "CHAR(36)", Order = 1)]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid TopicId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 儲存在 Storage 內的檔案 Ticket 名稱。
    /// </summary>
    [Column("_ticket", TypeName = "NVARCHAR(256)")]
    public string? Ticket { get; set; }

    /// <summary>
    /// 原始檔案包含副檔名的完整檔案名稱。
    /// </summary>
    [Required]
    [Column("_name", TypeName = "NVARCHAR(256)")]
    [StringLength(256)]
    public string Filename { get; set; } = string.Empty;

    /// <summary>
    /// 原始檔案的副檔名。
    /// </summary>
    [Required]
    [Column("_ext", TypeName = "NVARCHAR(256)")]
    [StringLength(256)]
    public string Extension { get; set; } = string.Empty;

    /// <summary>
    /// 轉檔前的檔案大小 (bytes)
    /// </summary>
    [Required]
    [Column("_original_size")]
    public long OriginalSize { get; set; }

    /// <summary>
    /// 轉檔後的檔案大小 (bytes)
    /// </summary>
    [Required]
    [Column("_size")]
    public long Size { get; set; }

    /// <summary>
    /// 影音長度 (秒)。
    /// </summary>
    [Required]
    [Column("_length")]
    public double Length { get; set; }

    /// <summary>
    /// 影格速率
    /// </summary>
    [Column("_frame_rate")]
    public double? FrameRate { get; set; }

    /// <summary>
    /// 狀態。
    /// </summary>
    [Obsolete("僅提供 1.0.34 以前版本使用。")]
    [Required]
    [Column("_status")]
    public StreamMediaStatus Status { get; set; }

    /// <summary>
    /// ASR 辨識狀態。
    /// </summary>
    [Required]
    [Column("_asr_status")]
    public AsrMediaStatus AsrStatus { get; set; }

    /// <summary>
    /// 轉檔狀態。
    /// </summary>
    [Required]
    [Column("_convert_status")]
    public ConvertMediaStatus ConvertStatus { get; set; }

    /// <summary>
    /// 轉檔進度
    /// </summary>
    [Required]
    [Column("_progress")]
    public int Progress { get; set; } = 0;

    /// <summary>
    /// 轉檔錯誤資訊
    /// </summary>
    [Column("_error")]
    public string? Error { get; set; }

    /// <summary>
    /// 開始 ASR 流程的時間。
    /// </summary>
    [Column("_asr_process_start")]
    public DateTime? ASRProcessStart { get; set; }

    /// <summary>
    /// 開始 ASR 流程的時間。
    /// </summary>
    [Column("_asr_process_end")]
    public DateTime? ASRProcessEnd { get; set; }

    /// <summary>
    /// 開始轉檔的時間。
    /// </summary>
    [Column("_process_start")]
    public DateTime? ProcessStart { get; set; }

    /// <summary>
    /// 轉檔結束的時間，無論成功或失敗。
    /// </summary>
    [Column("_process_end")]
    public DateTime? ProcessEnd { get; set; }

    [Required]
    [Column("_create")]
    public DateTime Create { get; set; } = DateTime.Now;

    [Required]
    [Column("_update")]
    public DateTime Update { get; set; } = DateTime.Now;

    public virtual Topic Topic { get; set; } = null!;
}
