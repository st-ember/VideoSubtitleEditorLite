namespace SubtitleEditor.Infrastructure.Models;

/// <summary>
/// 可分頁要求完成後，透過 API 回傳的回應物件。
/// </summary>
public abstract class ListResponseBase
{
    /// <summary>
    /// 總資料數
    /// </summary>
    public virtual int TotalCount { get; set; }

    /// <summary>
    /// 總頁數
    /// </summary>
    public virtual int TotalPage { get; set; }

    /// <summary>
    /// 頁碼(從 1 開始)
    /// </summary>
    public virtual int Page { get; set; }

    /// <summary>
    /// 使用一個已完成的 <see cref="IPageableRequest"/> 物件來初始化此回應物件。
    /// </summary>
    /// <param name="pageableRequest">已完成的分頁要求物件</param>
    public ListResponseBase(IPageableRequest pageableRequest)
    {
        Setup(pageableRequest);
    }

    /// <summary>
    /// 初始化回應物件的虛擬方法。
    /// </summary>
    /// <param name="pageableRequest">已完成的分頁要求物件</param>
    protected virtual void Setup(IPageableRequest pageableRequest)
    {
        Page = pageableRequest.Page;
        TotalPage = pageableRequest.PageModel.TotalPage;
        TotalCount = pageableRequest.PageModel.TotalCount;
    }
}

/// <summary>
/// 可分頁要求完成後，透過 API 回傳的回應物件，包含泛型資料清單。
/// </summary>
/// <typeparam name="TListDataModel">來源清單項目的型別</typeparam>
/// <typeparam name="TDataModel">結果清單的型別</typeparam>
public abstract class ListResponseBase<TListDataModel, TDataModel> : ListResponseBase
    where TListDataModel : class
    where TDataModel : class
{
    /// <summary>
    /// 泛型資料清單
    /// </summary>
    public virtual TDataModel[] List { get; set; } = Array.Empty<TDataModel>();

    /// <summary>
    /// 使用一個已完成的 <see cref="IPageableViewModel{TListDataModel}"/> 物件來初始化此回應物件。
    /// </summary>
    /// <param name="pageableRequest">已完成的分頁要求物件</param>
    public ListResponseBase(IPageableViewModel<TListDataModel> pageableRequest) : base(pageableRequest)
    {
        List = BuildList(pageableRequest);
    }

    /// <summary>
    /// 將 <see cref="IPageableViewModel{TListDataModel}"/> 內的 List 轉換成實際要回應的結果物件。
    /// </summary>
    protected abstract TDataModel[] BuildList(IPageableViewModel<TListDataModel> pageableRequest);
}