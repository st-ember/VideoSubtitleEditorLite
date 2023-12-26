using SubtitleEditor.Infrastructure.Models;
using SubtitleEditor.Web.Infrastructure.Models.Topic;

namespace SubtitleEditor.Web.Models.Topic;

public class TopicListResponse : ListResponseBase<TopicListData, TopicListData>
{
    public TopicListResponse(TopicListViewModel viewModel) : base(viewModel) { }

    public static TopicListResponse From(TopicListViewModel viewModel)
    {
        return new TopicListResponse(viewModel);
    }

    protected override TopicListData[] BuildList(IPageableViewModel<TopicListData> pageableRequest)
    {
        return pageableRequest.List;
    }
}