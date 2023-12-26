using SubtitleEditor.Core.Abstract;
using SubtitleEditor.Infrastructure.ListAdeptors.Abstract;
using SubtitleEditor.Infrastructure.Models;

namespace SubtitleEditor.Infrastructure.Services;

/// <summary>
/// 泛型的清單服務，透過提供不同的清單處理器，可以從資料庫內使用指定條件來取出物件清單。
/// </summary>
/// <typeparam name="TListProcessor">清單處理器的物件型別</typeparam>
public interface IGenericListService<TListProcessor>
    where TListProcessor : IListProcessor, new()
{
    /// <summary>
    /// 取得清單處理器，如果曾經使用過將會回傳同一個處理器。
    /// </summary>
    /// <returns>清單處理器</returns>
    TListProcessor GetProcessor();

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
    /// <typeparam name="TEntity">Query 內使用的資料庫 Entity 型別</typeparam>
    /// <param name="request">要求物件</param>
    /// <returns>包含資料庫 Entity 型別且尚未執行的 Query</returns>
    Task<IQueryable<TEntity>> FilterAsync<TRequest, TEntity>(TRequest request)
        where TEntity : class, IWithId;

    /// <summary>
    /// 使用 ViewModel 內的條件從資料庫取出物件清單，並放回 ViewModel 內。
    /// </summary>
    /// <typeparam name="TViewModel">ViewModel 物件型別</typeparam>
    /// <typeparam name="TDataItem">回傳的物件型別</typeparam>
    /// <param name="viewModel">可分頁的 ViewModel 物件</param>
    Task QueryAsync<TViewModel, TDataItem>(TViewModel viewModel)
        where TViewModel : IPageableViewModel<TDataItem>
        where TDataItem : class, IWithId;

    /// <summary>
    /// 使用 ViewModel 內的條件從資料庫取出所有物件，並回傳這些物件。
    /// </summary>
    /// <typeparam name="TViewModel">ViewModel 物件型別</typeparam>
    /// <typeparam name="TDataItem">回傳的物件型別</typeparam>
    /// <param name="viewModel">可分頁的 ViewModel 物件</param>
    /// <returns>從資料庫內取出的物件陣列。</returns>
    Task<TDataItem[]> QueryAllAsync<TViewModel, TDataItem>(TViewModel viewModel)
        where TViewModel : IPageableViewModel<TDataItem>
        where TDataItem : class, IWithId;
}