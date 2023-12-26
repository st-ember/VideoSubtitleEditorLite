using SubtitleEditor.Infrastructure.Models;
using SubtitleEditor.Web.Infrastructure.Models.UserGroup;

namespace SubtitleEditor.Web.Models.UserGroup;

public class UserGroupListResponse : ListResponseBase<UserGroupListData, UserGroupListData>
{
    public UserGroupListResponse(UserGroupListViewModel viewModel) : base(viewModel) { }

    public static UserGroupListResponse From(UserGroupListViewModel viewModel)
    {
        return new UserGroupListResponse(viewModel);
    }

    protected override UserGroupListData[] BuildList(IPageableViewModel<UserGroupListData> pageableRequest)
    {
        return pageableRequest.List;
    }
}
