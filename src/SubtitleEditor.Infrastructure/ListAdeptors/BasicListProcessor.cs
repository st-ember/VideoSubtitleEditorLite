using Microsoft.EntityFrameworkCore;
using SubtitleEditor.Core.Abstract;
using SubtitleEditor.Database;
using SubtitleEditor.Infrastructure.ListAdeptors.Abstract;
using SubtitleEditor.Infrastructure.ListAdeptors.OrderMap;
using SubtitleEditor.Infrastructure.Models;

namespace SubtitleEditor.Infrastructure.ListAdeptors;

/// <summary>
/// 清單處理器物件，用來從指定的資料庫取得清單，並依照頁數等資訊回傳清單結果。
/// </summary>
/// <typeparam name="TEntity">要 Query 的物件型別，需要實作 IWithId 介面。</typeparam>
/// <typeparam name="TCondition">Query 的條件物件型別。</typeparam>
public abstract class BasicListProcessor<TEntity, TCondition> : IListProcessor
    where TEntity : IWithId
{
    public int Page { get; protected set; }
    public int TotalCount { get; protected set; }
    public int TotalPage { get; protected set; }
    public int DataSkip { get; protected set; }
    public int DataTake { get; protected set; }

    /// <summary>
    /// Service Provider Object
    /// </summary>
    protected virtual IServiceProvider ServiceProvider { get; set; } = null!;

    /// <summary>
    /// 資料來源的資料庫。
    /// </summary>
    protected virtual EditorContext Database { get; set; } = default!;

    /// <summary>
    /// 清單排序對應物件。
    /// </summary>
    protected virtual OrderMap<TEntity> OrderFuncMap => new();

    /// <summary>
    /// 清單要求物件。
    /// </summary>
    protected IPageableRequest? Request { get; set; }

    /// <summary>
    /// 泛型的清單要求物件。
    /// </summary>
    protected TCondition Condition { get; set; } = default!;

    /// <summary>
    /// 經篩選後的物件 Identity 陣列，通常包含物件的 ID。
    /// </summary>
    protected object[] Ids { get; set; } = Array.Empty<object>();

    public virtual void Initialize(IServiceProvider serviceProvider, EditorContext database)
    {
        ServiceProvider = serviceProvider;
        Database = database;
    }

    public async Task<IQueryable> FilterAsync<TRequest>(TRequest request)
    {
        Request = request is IPageableRequest pageableRequest ? pageableRequest : null;

        if (request is TCondition condition)
        {
            Condition = condition;
        }
        else
        {
            throw new ArgumentException("輸入的要求不符合清單處理器定義的有效條件", nameof(request));
        }

        return await QueryEntityAsync();
    }

    public async Task<IQueryable<TFilterEntity>> FilterAsync<TRequest, TFilterEntity>(TRequest request)
        where TFilterEntity : class, IWithId
    {
        var query = await FilterAsync(request);
        return query.OfType<TFilterEntity>();
    }

    public async Task<IPageableViewModel<TDataItem>> QueryAsync<TRequest, TDataItem>(TRequest request)
        where TRequest : IPageableRequest
        where TDataItem : class, IWithId
    {
        Request = request;
        Condition = (TCondition)Request;

        // 1. 組成 Query
        // 2. 排序後，依照頁碼取回 Identity 清單。
        await OrderAndListIdAsync(await QueryEntityAsync());

        // 3. 依照 Identity 清單取回物件。
        // 4. 依照 Identity 排序
        var list = SortItemById(await RetrieveDataAsync());

        return new PageableViewModel<TDataItem>()
        {
            List = await PostProduce(list.Cast<TDataItem>()),
            Page = Page,
            PageSize = request.PageSize,
            PageModel = new PageModel { TotalCount = TotalCount, TotalPage = TotalPage },
            OrderColumn = request.OrderColumn,
            Descending = request.Descending
        };
    }

    /// <summary>
    /// 從資料庫內依照要求條件進行 Query。
    /// </summary>
    /// <returns>尚未執行的 Query</returns>
    protected abstract Task<IQueryable<TEntity>> QueryEntityAsync();

    /// <summary>
    /// 依照 Identity 清單依序從資料庫取回物件。
    /// </summary>
    /// <returns>尚未排序的物件清單</returns>
    protected abstract Task<IWithId[]> RetrieveDataAsync();

    /// <summary>
    /// 完成 query 並依照頁碼與頁數資訊取得該頁的物件 Identity 清單，完成後會將 Identity 清單放入 Ids 中。
    /// </summary>
    /// <param name="query">尚未執行的 Query</param>
    protected virtual async Task OrderAndListIdAsync(IQueryable<TEntity> query)
    {
        if (Request == null)
        {
            throw new Exception("Request is null");
        }

        // 先進行排序
        var orderedIds = OrderFuncMap.OrderAndListId(query, Request.OrderColumn, Request.Descending);

        // 取得物件總數
        await SetTotalCountAsync(orderedIds);

        if (Request.PageSize > 0)
        {
            // 如果有提供 PageSize 代表要依照指定頁碼取得資料
            // 計算總頁數
            TotalPage = TotalCount > 0 && Request.PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / Request.PageSize) : 0;

            // 確認有效的頁碼
            Page = Request.Page > TotalPage ? TotalPage > 0 ? TotalPage : 1 : Request.Page;

            // 取得指定頁碼的資料
            DataSkip = (Page - 1) * Request.PageSize;
            DataTake = Request.PageSize;
            await SetIdsAsync(orderedIds.Skip(DataSkip).Take(DataTake));
        }
        else
        {
            // 如果沒有提供 PageSize，代表要撈回所有資料。
            TotalPage = 1;
            Page = 1;
            await SetIdsAsync(orderedIds);
        }
    }

    /// <summary>
    /// 計算 Query 中的物件總數，並放入 TotalCount 屬性中。
    /// </summary>
    /// <param name="orderedIds"></param>
    protected virtual async Task SetTotalCountAsync(IQueryable<object> orderedIds)
    {
        TotalCount = await orderedIds.CountAsync();
    }

    /// <summary>
    /// 從 Query 中取得 ID，並放入 Ids 屬性中。
    /// </summary>
    /// <param name="orderedIds"></param>
    protected virtual async Task SetIdsAsync(IQueryable<object> orderedIds)
    {
        Ids = await orderedIds.ToArrayAsync();
    }

    /// <summary>
    /// 依照 Identity 來排序物件清單。
    /// </summary>
    /// <param name="items">物件清單</param>
    /// <returns>已經排序的物件清單</returns>
    protected virtual IWithId[] SortItemById(IEnumerable<IWithId> items)
    {
        return Ids.Select(id => items.Where(e => e.HasSameId(id)).Single()).ToArray();
    }

    /// <summary>
    /// 後期處理，將已經排序的物件清單轉換成陣列，或做更多資料修飾。
    /// </summary>
    /// <typeparam name="TDataItem">回傳的清單物件型別</typeparam>
    /// <param name="list">已經排序的物件清單</param>
    /// <returns>已完成後期處理的物件陣列</returns>
    protected virtual Task<TDataItem[]> PostProduce<TDataItem>(IEnumerable<TDataItem> list) where TDataItem : class, IWithId
    {
        return Task.FromResult(list.ToArray());
    }
}