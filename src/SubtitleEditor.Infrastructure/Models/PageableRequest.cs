using SubtitleEditor.Core.Models;

namespace SubtitleEditor.Infrastructure.Models;

/// <summary>
/// 可分頁的要求。
/// </summary>
public interface IPageableRequest : ISimpleResult
{
    /// <summary>
    /// 頁碼(從 1 開始)
    /// </summary>
    int Page { get; set; }

    /// <summary>
    /// 每頁項目數
    /// </summary>
    int PageSize { get; set; }

    /// <summary>
    /// 排序欄位名稱
    /// </summary>
    string? OrderColumn { get; set; }

    /// <summary>
    /// 是否以降冪排列
    /// </summary>
    bool Descending { get; set; }

    /// <summary>
    /// 分頁資料
    /// </summary>
    PageModel PageModel { get; set; }
}

/// <summary>
/// 可分頁的要求。
/// </summary>
public abstract class PageableRequest : SimpleResult, IPageableRequest
{
    /// <summary>
    /// 頁碼(從 1 開始)
    /// </summary>
    public int Page
    {
        get => _innerPage;
        set => _innerPage = value < 1 ? 1 : value;
    }

    /// <summary>
    /// 每頁項目數
    /// </summary>
    public virtual int PageSize { get; set; } = 20;

    private int _innerPage { get; set; } = 1;

    /// <summary>
    /// 分頁資料
    /// </summary>
    public PageModel PageModel { get; set; } = new PageModel();

    /// <summary>
    /// 排序欄位名稱
    /// </summary>
    public virtual string? OrderColumn { get; set; }

    /// <summary>
    /// 是否以降冪排列
    /// </summary>
    public virtual bool Descending { get; set; } = true;
}

/// <summary>
/// 可分頁的 ViewModel，包含描述表格 Head 的清單。
/// </summary>
public interface IPageableViewWithHeaderModel : IPageableRequest
{
    /// <summary>
    /// 表格 Head 清單
    /// </summary>
    IPageHeader[] Headers { get; }
}

/// <summary>
/// 可分頁的 ViewModel，包含指定資料型別的資料清單。
/// </summary>
/// <typeparam name="T">資料型別</typeparam>
public interface IPageableViewModel<T> : IPageableRequest where T : class
{
    /// <summary>
    /// 資料清單
    /// </summary>
    T[] List { get; set; }

    /// <summary>
    /// 輸入另一個可分頁的 ViewModel，將所有輸入的資訊複製一份到此物件中。
    /// </summary>
    /// <param name="response"></param>
    void Map(IPageableViewModel<T> response);
}

/// <summary>
/// 可分頁的 ViewModel 實作。
/// </summary>
/// <typeparam name="T">資料型別</typeparam>
public class PageableViewModel<T> : PageableRequest, IPageableViewModel<T>, IPageableViewWithHeaderModel, ISimpleResult where T : class
{
    /// <summary>
    /// 資料清單
    /// </summary>
    public T[] List { get; set; } = Array.Empty<T>();

    /// <summary>
    /// 表格 Head 清單
    /// </summary>
    public virtual IPageHeader[] Headers => Array.Empty<IPageHeader>();

    /// <summary>
    /// 輸入另一個可分頁的 ViewModel，將所有輸入的資訊複製一份到此物件中。
    /// </summary>
    public void Map(IPageableViewModel<T> response)
    {
        List = response.List;
        Page = response.Page;
        PageSize = response.PageSize;
        PageModel = response.PageModel;
        OrderColumn = response.OrderColumn;
        Descending = response.Descending;
    }
}

/// <summary>
/// 分頁資料
/// </summary>
public class PageModel
{
    /// <summary>
    /// 總頁數
    /// </summary>
    public int TotalPage { get; set; } = 0;

    /// <summary>
    /// 總資料數
    /// </summary>
    public int TotalCount { get; set; } = 0;
}