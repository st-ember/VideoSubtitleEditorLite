using Microsoft.CodeAnalysis;
using SubtitleEditor.Infrastructure.Models;
using SubtitleEditor.Web.Infrastructure.Models.Log;

namespace SubtitleEditor.Web.Models.Log;

public class LogListResponse : ListResponseBase<LogListPrimaryData, LogListPrimaryDataModel>
{
    public LogListResponse(LogListViewModel viewModel) : base(viewModel) { }

    public static LogListResponse From(LogListViewModel viewModel)
    {
        return new LogListResponse(viewModel);
    }

    protected override LogListPrimaryDataModel[] BuildList(IPageableViewModel<LogListPrimaryData> pageableRequest)
    {
        return pageableRequest.List.Select(m => LogListPrimaryDataModel.From(m)).ToArray();
    }
}