namespace SubtitleEditor.Infrastructure.Models;

/// <summary>
/// 資料表格 Header 樣式
/// </summary>
public class PageHeaderStyle
{
    /// <summary>
    /// 指定寬度(px)
    /// </summary>
    public uint? Width { get; set; }

    /// <summary>
    /// 橫向跨欄數
    /// </summary>
    public uint? ColSpan { get; set; }

    /// <summary>
    /// 垂直跨行數
    /// </summary>
    public uint? RowSpan { get; set; }

    /// <summary>
    /// 對齊方式
    /// </summary>
    public TextAlign Align { get; set; }

    /// <summary>
    /// Class
    /// </summary>
    public string Class { get; set; } = "";

    /// <summary>
    /// 對齊方式
    /// </summary>
    public enum TextAlign
    {
        /// <summary>
        /// 向左對齊
        /// </summary>
        Left,

        /// <summary>
        /// 置中對齊
        /// </summary>
        Center,

        /// <summary>
        /// 向右對齊
        /// </summary>
        Right
    }
}