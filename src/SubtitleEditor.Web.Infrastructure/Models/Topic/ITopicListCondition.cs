namespace SubtitleEditor.Web.Infrastructure.Models.Topic;

public interface ITopicListCondition
{
    string Keyword { get; set; }
    string TopicStatus { get; set; }
    string AsrMediaStatus { get; set; }
    string ConvertMediaStatus { get; set; }

    /// <summary>
    /// 查詢日期起始
    /// </summary>
    string Start { get; set; }

    /// <summary>
    /// 查詢日期結束
    /// </summary>
    string End { get; set; }
}
