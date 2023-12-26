using SubtitleEditor.Core.Abstract;
using SubtitleEditor.Database;
using SubtitleEditor.Infrastructure.ListAdeptors.Abstract;
using SubtitleEditor.Infrastructure.Models;
using SubtitleEditor.Infrastructure.Services;

namespace SubtitleEditor.Infrastructure.ServiceImplements;

public class GenericListService<TListProcessor> : IGenericListService<TListProcessor>
    where TListProcessor : IListProcessor, new()
{
    protected virtual EditorContext Database { get; }
    protected virtual IServiceProvider ServiceProvider { get; }
    protected virtual TListProcessor? Processor { get; set; }

    public GenericListService(IServiceProvider serviceProvider, EditorContext database)
    {
        ServiceProvider = serviceProvider;
        Database = database;
    }

    public virtual TListProcessor GetProcessor()
    {
        if (Processor != null)
        {
            return Processor;
        }

        Processor = new TListProcessor();
        Processor.Initialize(ServiceProvider, Database);
        return Processor;
    }

    public virtual Task<IQueryable> FilterAsync<TRequest>(TRequest request)
    {
        return GetProcessor().FilterAsync(request);
    }

    public virtual Task<IQueryable<TEntity>> FilterAsync<TRequest, TEntity>(TRequest request)
        where TEntity : class, IWithId
    {
        return GetProcessor().FilterAsync<TRequest, TEntity>(request);
    }

    public virtual async Task QueryAsync<TViewModel, TDataItem>(TViewModel viewModel)
        where TViewModel : IPageableViewModel<TDataItem>
        where TDataItem : class, IWithId
    {
        var response = await GetProcessor().QueryAsync<TViewModel, TDataItem>(viewModel);
        viewModel.Map(response);
    }

    public virtual async Task<TDataItem[]> QueryAllAsync<TViewModel, TDataItem>(TViewModel viewModel)
        where TViewModel : IPageableViewModel<TDataItem>
        where TDataItem : class, IWithId
    {
        viewModel.PageSize = 0;
        var response = await GetProcessor().QueryAsync<TViewModel, TDataItem>(viewModel);

        return response.List;
    }
}