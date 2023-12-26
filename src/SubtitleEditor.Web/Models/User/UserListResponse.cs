using SubtitleEditor.Infrastructure.Models;
using SubtitleEditor.Web.Infrastructure.Models.User;

namespace SubtitleEditor.Web.Models.User;

public class UserListResponse : ListResponseBase<UserListData, UserListData>
{
    public UserListResponse(UserListViewModel viewModel) : base(viewModel) { }

    public static UserListResponse From(UserListViewModel viewModel)
    {
        return new UserListResponse(viewModel);
    }

    protected override UserListData[] BuildList(IPageableViewModel<UserListData> pageableRequest)
    {
        return pageableRequest.List;
    }
}