using SubtitleEditor.Core.Abstract;

namespace SubtitleEditor.Infrastructure.ListAdeptors.OrderMap;

public class OrderMap<TEntity> : Dictionary<string, Func<bool, IQueryable<TEntity>, IOrderedQueryable<TEntity>>> where TEntity : IWithId
{
    public IQueryable<TEntity> Order(IQueryable<TEntity> query, string? orderTarget, bool descending)
    {
        return !string.IsNullOrWhiteSpace(orderTarget) && ContainsKey(orderTarget) ? this[orderTarget](descending, query) : query;
    }

    public IQueryable<object> OrderAndListId(IQueryable<TEntity> query, string? orderTarget, bool descending)
    {
        return Order(query, orderTarget, descending).Select(o => o.GetId());
    }
}