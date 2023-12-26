using System.ComponentModel;

namespace SubtitleEditor.Core.Contexts;

/// <summary>
/// 串流檔案的狀態。
/// </summary>
public enum StreamMediaStatus
{
    /// <summary>
    /// 辨識排程等待中。
    /// </summary>
    [Description("辨識排程等待中")]
    ASRWaiting,

    /// <summary>
    /// 辨識進行中。
    /// </summary>
    [Description("辨識進行中")]
    ASRProcessing,

    /// <summary>
    /// 辨識被取消。
    /// </summary>
    [Description("辨識被取消")]
    ASRCanceled,

    /// <summary>
    /// 辨識失敗。
    /// </summary>
    [Description("辨識失敗")]
    ASRFailed,

    /// <summary>
    /// 辨識完成。
    /// </summary>
    [Description("辨識完成")]
    ASRCompleted,

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
    Completed,

    /// <summary>
    /// 轉檔失敗。
    /// </summary>
    [Description("轉檔失敗")]
    Failed
}
