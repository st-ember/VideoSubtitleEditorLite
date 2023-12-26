using System.ComponentModel;

namespace SubtitleEditor.Core.Contexts;

/// <summary>
/// 串流檔案的轉檔狀態。
/// </summary>
public enum ConvertMediaStatus
{
    /// <summary>
    /// 排程等待轉檔中。
    /// </summary>
    [Description("排程等待轉檔中")]
    FFMpegWaiting,

    /// <summary>
    /// 正在轉檔中。
    /// </summary>
    [Description("正在轉檔中")]
    FFMpegProcessing,

    /// <summary>
    /// 已完成。
    /// </summary>
    [Description("已完成")]
    FFMpegCompleted,

    /// <summary>
    /// 轉檔失敗。
    /// </summary>
    [Description("轉檔失敗")]
    FFMpegFailed
}