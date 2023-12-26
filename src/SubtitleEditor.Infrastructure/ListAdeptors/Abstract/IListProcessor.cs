using SubtitleEditor.Core.Abstract;
using SubtitleEditor.Database;
using SubtitleEditor.Infrastructure.Models;

namespace SubtitleEditor.Infrastructure.ListAdeptors.Abstract;

/// <summary>
/// 清單處理器物件基本介面，用來從指定的資料庫取得清單，並依照頁數等資訊回傳清單結果。
/// </summary>
public interface IListProcessor
{
    /// <summary>
    /// 目前清單的頁碼，從 1 開始。
    /// </summary>
    int Page { get; }

    /// <summary>
    /// 目前清單的資料總數。
    /// </summary>
    int TotalCount { get; }

    /// <summary>
    /// 目前清單的總頁數。
    /// </summary>
    int TotalPage { get; }

    /// <summary>
    /// 目前頁面第一筆資料前被跳過的資料筆數。
    /// </summary>
    int DataSkip { get; }

    /// <summary>
    /// 目前頁面需要從資料庫取回的資料筆數，等同於 PageSize 屬性。
    /// </summary>
    int DataTake { get; }

    /// <summary>
    /// 輸入資料庫與 Service Procider 來初始化此處理器。
    /// </summary>
    /// <param name="serviceProvider">Service Procider</param>
    /// <param name="database">資料來源的資料庫物件</param>
    void Initialize(IServiceProvider serviceProvider, EditorContext database);

    /// <summary>
    /// 使用輸入的條件建立篩選，並回傳尚未執行的 Query。
    /// </summary>
    /// <typeparam name="TRequest">要求物件型別</typeparam>
    /// <param name="request">要求物件</param>
    /// <returns>尚未執行的 Query</returns>
    Task<IQueryable> FilterAsync<TRequest>(TRequest request);

    /// <summary>
    /// 使用輸入的條件建立篩選，並回傳尚未執行且使用資料庫 Entity 轉換的 Query。
    /// </summary>
    /// <typeparam name="TRequest">要求物件型別</typeparam>
    /// <typeparam name="TFilterEntity">Query 內使用的資料庫 Entity 型別</typeparam>
    /// <param name="request">要求物件</param>
    /// <returns>包含資料庫 Entity 型別且尚未執行的 Query</returns>
    Task<IQueryable<TFilterEntity>> FilterAsync<TRequest, TFilterEntity>(TRequest request)
        where TFilterEntity : class, IWithId;

    /// <summary>
    /// 對資料庫執行 Query，並提供篩選條件來取得清單。
    /// </summary>
    /// <typeparam name="TRequest">要求物件型別</typeparam>
    /// <typeparam name="TDataItem">回傳的清單物件型別</typeparam>
    /// <param name="request">要求物件</param>
    /// <returns>可分頁的清單物件</returns>
    Task<IPageableViewModel<TDataItem>> QueryAsync<TRequest, TDataItem>(TRequest request)
        where TRequest : IPageableRequest
        where TDataItem : class, IWithId;
}