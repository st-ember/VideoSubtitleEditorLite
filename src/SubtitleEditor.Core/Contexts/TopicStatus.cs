using System.ComponentModel;

namespace SubtitleEditor.Core.Contexts;

/// <summary>
/// 單集狀態。
/// </summary>
public enum TopicStatus
{
    /// <summary>
    /// 正常。
    /// </summary>
    [Description("正常")]
    Normal,

    /// <summary>
    /// 暫停，此單集所有類型的背景工作將停止。
    /// </summary>
    [Description("暫停")]
    Paused,

    /// <summary>
    /// 封存，除暫停外，此單集不在清單中主動顯示，但檔案皆暫時保留。
    /// </summary>
    [Description("封存")]
    Archived,

    /// <summary>
    /// 已移除，所有相關檔案皆刪除。
    /// </summary>
    [Description("移除")]
    Removed
}
