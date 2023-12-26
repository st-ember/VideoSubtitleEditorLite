using SubtitleEditor.Infrastructure.Models;
using SubtitleEditor.Web.Infrastructure.Models.UserGroup;

namespace SubtitleEditor.Web.Models.UserGroup;

public class UserGroupListViewModel : PageableViewModel<UserGroupListData>, IUserGroupListCondition
{
    public override string? OrderColumn { get; set; } = nameof(UserGroupListData.Name);
    public override bool Descending { get; set; } = false;

    public override IPageHeader[] Headers => new IPageHeader[]
    {
        PageHeader.From("名稱", nameof(UserGroupListData.Name), true,
            new PageHeaderStyle() { Align = PageHeaderStyle.TextAlign.Left }),
        PageHeader.From("說明", nameof(UserGroupListData.Description), false,
            new PageHeaderStyle() { Align = PageHeaderStyle.TextAlign.Left }),
        PageHeader.From("群組類型", nameof(UserGroupListData.GroupType), false,
            new PageHeaderStyle() { Align = PageHeaderStyle.TextAlign.Left }),
        PageHeader.From("使用者數量", nameof(UserGroupListData.UserCount), false,
            new PageHeaderStyle() { Align = PageHeaderStyle.TextAlign.Left }),
        PageHeader.From("建立時間", nameof(UserGroupListData.Create), true,
            new PageHeaderStyle() { Align = PageHeaderStyle.TextAlign.Left }),
    };

    public string Keyword { get; set; } = string.Empty;
    public bool AsrAccess { get; set; } = false;

}
