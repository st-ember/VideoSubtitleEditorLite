using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Core.Helpers;
using SubtitleEditor.Core.Models;
using SubtitleEditor.Infrastructure.Models;
using SubtitleEditor.Web.Infrastructure.Models.User;

namespace SubtitleEditor.Web.Models.User;

public class UserListViewModel : PageableViewModel<UserListData>, IUserListCondition
{
    public SelectOption[] StatusOptions =>
        new SelectOption[] {
            new SelectOption("全部", "-1") { Text = "全部", Value = "-1" },
            new SelectOption(UserStatus.Enabled.GetName(), ((int)UserStatus.Enabled).ToString()),
            new SelectOption(UserStatus.Disabled.GetName(), ((int)UserStatus.Disabled).ToString())
        };

    public override string? OrderColumn { get; set; } = nameof(UserListData.Account);
    public override bool Descending { get; set; } = false;

    public override IPageHeader[] Headers => new IPageHeader[]
    {
        PageHeader.From("帳號", nameof(UserListData.Account), true,
            new PageHeaderStyle() { Align = PageHeaderStyle.TextAlign.Left, Width = 100 }),
        PageHeader.From("姓名", nameof(UserListData.Name), true,
            new PageHeaderStyle() { Align = PageHeaderStyle.TextAlign.Left }),
        PageHeader.From("職稱", nameof(UserListData.Title), true,
            new PageHeaderStyle() { Align = PageHeaderStyle.TextAlign.Left }),
        PageHeader.From("電話", nameof(UserListData.Telephone), true,
            new PageHeaderStyle() { Align = PageHeaderStyle.TextAlign.Left }),
        PageHeader.From("Email", nameof(UserListData.Email), true,
            new PageHeaderStyle() { Align = PageHeaderStyle.TextAlign.Left }),
        PageHeader.From("群組", nameof(UserListData.UserGroups), false,
            new PageHeaderStyle() { Align = PageHeaderStyle.TextAlign.Left }),
        PageHeader.From("狀態", nameof(UserListData.Status), false,
            new PageHeaderStyle() { Align = PageHeaderStyle.TextAlign.Left }),
        PageHeader.From("建立時間", nameof(UserListData.Create), true,
            new PageHeaderStyle() { Align = PageHeaderStyle.TextAlign.Left }),
    };

    public string Keyword { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool AsrAccess { get; set; } = false;
}
