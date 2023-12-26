using System.ComponentModel;

namespace SubtitleEditor.Core.Contexts;

// <summary>
// 串流檔案的 ASR 狀態。
// </summary>
public enum AsrMediaStatus
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
    /// 辨識完成。
    /// </summary>
    [Description("辨識完成")]
    ASRCompleted,

    /// <summary>
    /// 辨識失敗。
    /// </summary>
    [Description("辨識失敗")]
    ASRFailed,

    /// <summary>
    /// 不須辨識。
    /// </summary>
    [Description("不須辨識")]
    ASRSkipped
}
