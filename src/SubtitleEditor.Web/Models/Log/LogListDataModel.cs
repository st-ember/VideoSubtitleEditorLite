using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Core.Helpers;
using SubtitleEditor.Web.Infrastructure.Models.Log;

namespace SubtitleEditor.Web.Models.Log;

public class LogListDataModel
{
    public string? Id { get; set; }
    public string? Time { get; set; }
    public bool Success { get; set; }
    public string? Target { get; set; }
    public string? Field { get; set; }
    public string? Before { get; set; }
    public string? After { get; set; }
    public string? Message { get; set; }
    public string? Code { get; set; }
    public string? Exception { get; set; }
    public string? InnerException { get; set; }
    public string? ActionMessage { get; set; }

    public static LogListDataModel From(LogListData data, string? actionText)
    {
        return new LogListDataModel
        {
            Id = data.Id.ToString(),
            Time = data.Time.ToString("yyyy/MM/dd HH:mm:ss"),
            Success = data.Success,
            Target = data.Target,
            Field = data.Field,
            Before = data.Before,
            After = data.After,
            Message = data.Message,
            Code = data.Code,
            Exception = data.Exception,
            InnerException = data.InnerException,
            ActionMessage = (SystemAction.Unknown.TryParse(actionText, out SystemAction am) ?
                am.GetActionMessage(data.Target, data.Field, data.Before, data.After, data.Message) :
                actionText) ?? ""
        };
    }
}
