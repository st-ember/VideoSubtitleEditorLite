using Microsoft.CodeAnalysis;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Core.Helpers;
using SubtitleEditor.Web.Infrastructure.Models.Log;

namespace SubtitleEditor.Web.Models.Log;

public class LogListPrimaryDataModel
{
    public string? Id { get; set; }
    public string? ActionId { get; set; }
    public string? ActionText { get; set; }
    public string? UserId { get; set; }
    public string? UserAccount { get; set; }
    public string? IPAddress { get; set; }
    public string? Request { get; set; }
    public string? Response { get; set; }
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
    public string? ActionName { get; set; }
    public string? ActionMessage { get; set; }
    public LogListDataModel[] Children { get; set; } = Array.Empty<LogListDataModel>();

    public static LogListPrimaryDataModel From(LogListPrimaryData data)
    {
        var action = SystemAction.Unknown.TryParse(data.ActionText, out SystemAction am) ? am : SystemAction.Unknown;
        return new LogListPrimaryDataModel
        {
            Id = data.Id.ToString(),
            ActionId = data.ActionId.ToString(),
            ActionText = data.ActionText,
            UserId = data.UserId.HasValue ? data.UserId.Value.ToString() : "",
            IPAddress = data.IPAddress,
            Request = data.Request,
            Response = data.Response,
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
            ActionName = action.GetName(),
            ActionMessage = action != SystemAction.Unknown ?
                am.GetActionMessage(data.Target, data.Field, data.Before, data.After, data.Message) :
                data.ActionText,
            Children = data.Children
                .OrderByDescending(m => m.Time)
                .Select(m => LogListDataModel.From(m, data.ActionText))
                .ToArray()
        };
    }
}
