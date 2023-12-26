using SubtitleEditor.Core.Contexts;

namespace SubtitleEditor.Infrastructure.Models.Asr;

public class NctuTask
{
    public long Id { get; set; }
    public string Owner { get; set; } = string.Empty;
    public int IsOwner { get; set; } // 0, 1
    public int SourceType { get; set; }
    public string SourceWebLink { get; set; } = string.Empty;

    /// <summary>
    /// 上傳的檔案名稱
    /// </summary>
    public string UploadedFileName { get; set; } = string.Empty;

    /// <summary>
    /// 任務標題
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 任務描述
    /// </summary>
    public string Description { get; set; } = string.Empty;
    public int AudioChannel { get; set; }
    public NctuTaskStatus Status { get; set; }

    /// <summary>
    /// 進度 (0~100)
    /// </summary>
    public int Progress { get; set; }

    /// <summary>
    /// 是否已取消
    /// </summary>
    public bool Canceled { get; set; }
    public int? Deleted { get; set; } // 0, 1

    /// <summary>
    /// 媒體長度秒數
    /// </summary>
    public int AudioLength { get; set; }
    public int? ErrorCode { get; set; }

    /// <summary>
    /// 結果說明文字，只出現在失敗的時候用來說明失敗原因。
    /// </summary>
    public string ResultComment { get; set; } = string.Empty;
    public int? ResultAudioFileExist { get; set; }
    public int? ResultSubtitleFileExist { get; set; }
    public int? ResultScriptFileExist { get; set; }

    /// <summary>
    /// 建立時間 (NTP)
    /// </summary>
    public string CreateTime { get; set; } = string.Empty; // 2023-05-26T08:52:04Z

    /// <summary>
    /// 執行秒數
    /// </summary>
    public int? ProcessTime { get; set; }
    public int? TaskPriority { get; set; } // 1: normal, 2: high
    public string ModelName { get; set; } = string.Empty;
    public int? AppliedModelCode { get; set; }
    public string ModelDisplayName { get; set; } = string.Empty;
}