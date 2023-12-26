namespace SubtitleEditor.Infrastructure.Models;

/// <summary>
/// 資料表格 Header
/// </summary>
public interface IPageHeader
{
    /// <summary>
    /// 顯示的標籤文字
    /// </summary>
    string Label { get; }

    /// <summary>
    /// 名稱
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 是否可排序
    /// </summary>
    bool Sortable { get; }

    /// <summary>
    /// 樣式設定
    /// </summary>
    PageHeaderStyle Style { get; }
}

/// <summary>
/// 資料表格 Header
/// </summary>
public class PageHeader : IPageHeader
{
    /// <summary>
    /// 顯示的標籤文字
    /// </summary>
    public string Label { get; set; } = "";

    /// <summary>
    /// 名稱
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// 是否可排序
    /// </summary>
    public bool Sortable { get; set; } = false;

    /// <summary>
    /// 樣式設定
    /// </summary>
    public PageHeaderStyle Style { get; set; } = new PageHeaderStyle();

    /// <summary>
    /// 使用標籤文字與名稱建立一個 Header 物件。
    /// </summary>
    public static IPageHeader From(string label, string name)
    {
        return new PageHeader
        {
            Label = label,
            Name = name
        };
    }

    /// <summary>
    /// 使用標籤文字、名稱與排序設定建立一個 Header 物件。
    /// </summary>
    public static IPageHeader From(string label, string name, bool sortable)
    {
        return new PageHeader
        {
            Label = label,
            Name = name,
            Sortable = sortable
        };
    }

    /// <summary>
    /// 使用標籤文字、名稱、排序設定與樣式建立一個 Header 物件。
    /// </summary>
    public static IPageHeader From(string label, string name, bool sortable, PageHeaderStyle style)
    {
        return new PageHeader
        {
            Label = label,
            Name = name,
            Sortable = sortable,
            Style = style
        };
    }
}