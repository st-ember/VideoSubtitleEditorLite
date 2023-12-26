namespace SubtitleEditor.Core.Models;

/// <summary>
/// 下拉式選單項目
/// </summary>
public class SelectOption
{
    /// <summary>
    /// 文字
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// 值
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// 說明文字
    /// </summary>
    public string? SubText { get; set; }

    /// <summary>
    /// 此項目是否為群組（必須包含 Children）
    /// </summary>
    public bool IsGroup { get; set; }

    /// <summary>
    /// 此項目是否為"全部"項目
    /// </summary>
    public bool IsAllOption { get; set; }

    /// <summary>
    /// 此項目如果是群組，其底下的子項目陣列。
    /// </summary>
    public SelectOption[] Children { get; set; } = Array.Empty<SelectOption>();

    public SelectOption(string text, string value)
    {
        Text = text;
        Value = value;
    }
}