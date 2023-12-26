using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Core.Helpers;
using SubtitleEditor.Core.Models;
using SubtitleEditor.Infrastructure.Models;
using SubtitleEditor.Web.Infrastructure.Models.Topic;

namespace SubtitleEditor.Web.Models.Topic;

public class TopicListViewModel : PageableViewModel<TopicListData>, ITopicListCondition
{
    public SelectOption[] TopicStatusOptions =>
        new SelectOption[] { new SelectOption("全部", "-1") { Text = "全部", Value = "-1" } }
            .Concat(Core.Contexts.TopicStatus.Normal.ListAllEnum()
                .Where(o => o != Core.Contexts.TopicStatus.Removed && o != Core.Contexts.TopicStatus.Paused)
                .Select(o => new SelectOption(o.GetDescription(), o.ToString())))
            .ToArray();

    public SelectOption[] AsrMediaStatusOptions =>
        new SelectOption[] { new SelectOption("全部", "-1") { Text = "全部", Value = "-1" } }
            .Concat(Core.Contexts.AsrMediaStatus.ASRCompleted.ListAllEnum().Select(o => new SelectOption(o.GetDescription(), o.ToString())))
            .ToArray();

    public SelectOption[] ConvertMediaStatusOptions =>
        new SelectOption[] { new SelectOption("全部", "-1") { Text = "全部", Value = "-1" } }
            .Concat(Core.Contexts.ConvertMediaStatus.FFMpegCompleted.ListAllEnum().Select(o => new SelectOption(o.GetDescription(), o.ToString())))
            .ToArray();

    public override string? OrderColumn { get; set; } = nameof(TopicListData.Create);
    public override bool Descending { get; set; } = true;

    public override IPageHeader[] Headers => new IPageHeader[]
    {
        PageHeader.From("名稱", nameof(TopicListData.Name), true,
            new PageHeaderStyle() { Align = PageHeaderStyle.TextAlign.Left }),
        PageHeader.From("原始檔案大小", nameof(TopicListData.OriginalSize), true,
            new PageHeaderStyle() { Align = PageHeaderStyle.TextAlign.Left }),
        PageHeader.From("編輯器占用容量", nameof(TopicListData.Size), true,
            new PageHeaderStyle() { Align = PageHeaderStyle.TextAlign.Left }),
        PageHeader.From("長度", nameof(TopicListData.Length), true,
            new PageHeaderStyle() { Align = PageHeaderStyle.TextAlign.Left }),
        PageHeader.From("花費時間", nameof(TopicListData.ProcessTime), false,
            new PageHeaderStyle() { Align = PageHeaderStyle.TextAlign.Left }),
        PageHeader.From("單集狀態", nameof(TopicListData.TopicStatus), true,
            new PageHeaderStyle() { Align = PageHeaderStyle.TextAlign.Left }),
        PageHeader.From("ASR 狀態", nameof(TopicListData.AsrMediaStatus), true,
            new PageHeaderStyle() { Align = PageHeaderStyle.TextAlign.Left }),
        PageHeader.From("轉檔狀態", nameof(TopicListData.ConvertMediaStatus), true,
            new PageHeaderStyle() { Align = PageHeaderStyle.TextAlign.Left }),
        PageHeader.From("建立方式", nameof(TopicListData.CreatedOption), true,
            new PageHeaderStyle() { Align = PageHeaderStyle.TextAlign.Left }),
        PageHeader.From("建立時間", nameof(TopicListData.Create), true,
            new PageHeaderStyle() { Align = PageHeaderStyle.TextAlign.Left })
    };

    public string Keyword { get; set; } = string.Empty;
    public string TopicStatus { get; set; } = string.Empty;
    public string AsrMediaStatus { get; set; } = string.Empty;
    public string ConvertMediaStatus { get; set; } = string.Empty;
    public string Start { get; set; } = string.Empty;
    public string End { get; set; } = string.Empty;
    public bool AsrAccess {  get; set; } = false;
}
