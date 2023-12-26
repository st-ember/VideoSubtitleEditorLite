using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace SubtitleEditor.Database.Entities;

public class Subtitle
{
    [Key]
    [Column("_topic_id", TypeName = "CHAR(36)", Order = 1)]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid TopicId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 備份的 JSON 格式原始字幕資料。這個資料在這筆資料被建立後就不該再被修改。
    /// </summary>
    [Required]
    [Column("_original", TypeName = "TEXT")]
    public string OriginalData { get; set; } = string.Empty;

    /// <summary>
    /// JSON 格式的字幕資料。
    /// </summary>
    [Required]
    [Column("_data", TypeName = "TEXT")]
    public string Data { get; set; } = string.Empty;

    /// <summary>
    /// 要用來製作字幕的純文字資料原始備份。
    /// </summary>
    [Column("_original_text", TypeName = "TEXT")]
    public string? OriginalTranscript { get; set; }

    /// <summary>
    /// 要用來製作字幕的純文字資料。
    /// </summary>
    [Column("_text", TypeName = "TEXT")]
    public string? Transcript { get; set; }

    /// <summary>
    /// 字數上限。此為全形文字的軟上限，不會硬性限制每行字幕的自數。
    /// </summary>
    [Column("_word_limit")]
    public int? WordLimit { get; set; }

    [Required]
    [Column("_create")]
    public DateTime Create { get; set; } = DateTime.Now;

    [Required]
    [Column("_update")]
    public DateTime Update { get; set; } = DateTime.Now;

    public virtual Topic Topic { get; set; } = null!;

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public Core.Models.Subtitle GetOrigianlSubtitle()
    {
        return !string.IsNullOrWhiteSpace(OriginalData) ?
            JsonSerializer.Deserialize<Core.Models.Subtitle>(OriginalData, _jsonSerializerOptions)! : new Core.Models.Subtitle();
    }

    public Core.Models.Subtitle GetSubtitle()
    {
        return !string.IsNullOrWhiteSpace(Data) ?
            JsonSerializer.Deserialize<Core.Models.Subtitle>(Data, _jsonSerializerOptions)! : new Core.Models.Subtitle();
    }

    public void SetOrigianlSubtitle(Core.Models.Subtitle subtitle)
    {
        OriginalData = JsonSerializer.Serialize(subtitle, _jsonSerializerOptions);
    }

    public void SetSubtitle(Core.Models.Subtitle subtitle)
    {
        Data = JsonSerializer.Serialize(subtitle, _jsonSerializerOptions);
    }
}
//需要加入createdOption