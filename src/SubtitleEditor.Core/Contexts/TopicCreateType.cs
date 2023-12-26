using System.ComponentModel;

namespace SubtitleEditor.Core.Contexts;

/// <summary>
/// 單集建立類型。
/// </summary>
public enum TopicCreateType
{
    /// <summary>
    /// 使用 ASR 流程來產生字幕。
    /// </summary>
    [Description("辨識流程")]
    ASR,

    /// <summary>
    /// 提供現有字幕，跳過 ASR 流程。
    /// </summary>
    [Description("現有字幕")]
    Subtitle,

    /// <summary>
    /// 提供逐字稿來人工製作字幕。
    /// </summary>
    [Description("逐字稿")]
    Transcript
}
