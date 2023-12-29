using System.Text.Json.Serialization;

namespace SubtitleEditor.Core.Models;

/// <summary>
/// 啟用資料，此資料會實際被寫在啟用金鑰內。
/// </summary>
public class ActivationData
{
    /// <summary>
    /// 版本。
    /// </summary>
    [JsonPropertyName("v")]
    public uint Version { get; set; } = 1;

    /// <summary>
    /// ID
    /// </summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 發行者名稱。
    /// </summary>
    [JsonPropertyName("pub")]
    public string Publisher { get; set; } = string.Empty;

    /// <summary>
    /// 發行對象名稱，通常是購買方的名字。
    /// </summary>
    [JsonPropertyName("tag")]
    public string Target { get; set; } = string.Empty;

    /// <summary>
    /// 次版本代碼。
    /// 以AsrAccess使用
    /// </summary>
    [JsonPropertyName("edn")]
    public string Editions { get; set; } = string.Empty;

    [JsonPropertyName("aac")]
    public bool AsrAccess { get; set; }

    /// <summary>
    /// 發行日期，yyyy-MM-dd 格式的文字。
    /// </summary>
    [JsonPropertyName("dat")]
    public string Date { get; set; } = string.Empty;

    /// <summary>
    /// 有效日期，yyyy-MM-dd 格式的文字。
    /// </summary>
    [JsonPropertyName("due")]
    public string? Due { get; set; }

    /// <summary>
    /// Client Access License (CAL) 的數量，表示可同時使用的使用者人數，如果為 0 則代表不限制。
    /// </summary>
    [JsonPropertyName("cal")]
    public uint CalCount { get; set; }

    /// <summary>
    /// 其他資訊。
    /// </summary>
    [JsonPropertyName("mta")]
    public string? Meta { get; set; }

    [JsonIgnore]
    public DateTime? DueDate
    {
        get => !string.IsNullOrWhiteSpace(Due) && DateTime.TryParse(Due, out var dd) ? dd : null;
        set => Due = value.HasValue ? value.Value.ToString("yyyy-MM-dd") : null;
    }

    [JsonIgnore]
    public DateTime PublishDate => !string.IsNullOrWhiteSpace(Date) && DateTime.TryParse(Date, out var d) ? d : default;
}
