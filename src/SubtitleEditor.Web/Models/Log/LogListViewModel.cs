using Microsoft.CodeAnalysis;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Core.Models;
using SubtitleEditor.Core.Helpers;
using SubtitleEditor.Infrastructure.Models;
using SubtitleEditor.Web.Infrastructure.Models.Log;

namespace SubtitleEditor.Web.Models.Log;

public class LogListViewModel : PageableViewModel<LogListPrimaryData>, ILogListCondition
{
    public SelectOption[] ActionOptions =>
        new SelectOption[] { new SelectOption("全部", "-1") { IsAllOption = true } }
            .Concat(SystemAction.Unknown.ListAllEnum().Where(o => o != SystemAction.Unknown).Select(o => new SelectOption(o.GetName(), o.ToString())))
            .ToArray();

    public SelectOption[] SuccessOptions =>
        new SelectOption[]
        {
            new SelectOption("全部", "-1"),
            new SelectOption("成功", "1"),
            new SelectOption("失敗", "0")
        };

    public override string? OrderColumn { get; set; } = nameof(LogListPrimaryData.Time);
    public override bool Descending { get; set; } = true;
    public override int PageSize { get; set; } = 100;

    public override IPageHeader[] Headers => new IPageHeader[]
    {
        PageHeader.From("操作結果", nameof(LogListPrimaryData.Success), false,
            new PageHeaderStyle() { Align = PageHeaderStyle.TextAlign.Left, Width = 78 }),
        PageHeader.From("時間", nameof(LogListPrimaryData.Time), true,
            new PageHeaderStyle() { Align = PageHeaderStyle.TextAlign.Left, Width = 150 }),
        PageHeader.From("使用者", nameof(LogListPrimaryDataModel.UserAccount), false,
            new PageHeaderStyle() { Align = PageHeaderStyle.TextAlign.Left }),
        PageHeader.From("IP", nameof(LogListPrimaryData.IPAddress), false,
            new PageHeaderStyle() { Align = PageHeaderStyle.TextAlign.Left }),
        PageHeader.From("操作", nameof(LogListPrimaryDataModel.ActionName), false,
            new PageHeaderStyle() { Align = PageHeaderStyle.TextAlign.Left }),
        PageHeader.From("操作目標", nameof(LogListPrimaryDataModel.Target), false,
            new PageHeaderStyle() { Align = PageHeaderStyle.TextAlign.Left }),
        PageHeader.From("訊息", nameof(LogListPrimaryDataModel.Message), false,
            new PageHeaderStyle() { Align = PageHeaderStyle.TextAlign.Left })
    };

    public string? Start { get; set; } = DateTime.Today.AddDays(-7).ToString("yyyy/MM/dd");
    public string? End { get; set; } = DateTime.Today.ToString("yyyy/MM/dd");
    public string? Actions { get; set; }
    public string? Target { get; set; }
    public string? IPAddress { get; set; }
    public string? User { get; set; }
    public bool? IsActionSuccess { get; set; }
    public bool? AsrAccess { get; set; } = false;
    public bool? HasKey { get; set; }
}
