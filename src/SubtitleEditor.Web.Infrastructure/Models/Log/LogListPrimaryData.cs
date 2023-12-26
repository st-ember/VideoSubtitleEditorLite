using SubtitleEditor.Core.Abstract;

namespace SubtitleEditor.Web.Infrastructure.Models.Log;

public class LogListPrimaryData : LogListData, IWithId<Guid>
{
    /// <summary>
    /// 操作 ID
    /// </summary>
    public Guid ActionId { get; set; }

    /// <summary>
    /// 操作名稱
    /// </summary>
    public string? ActionText { get; set; }

    /// <summary>
    /// 操作使用者 ID
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// 操作使用者 IP
    /// </summary>
    public string? IPAddress { get; set; }

    /// <summary>
    /// 紀錄觸發操作使用的要求文字，通常是來自前端的 JSON 格式要求。
    /// </summary>
    public string? Request { get; set; }

    /// <summary>
    /// 紀錄操作完成後回傳的資訊，通常是指回傳給前端的 JSON 回應。
    /// </summary>
    public string? Response { get; set; }

    public LogListData[] Children { get; set; } = Array.Empty<LogListData>();

    public new Guid GetGenericId()
    {
        return Id;
    }

    public new object GetId()
    {
        return Id;
    }

    public new bool HasSameId(Guid id)
    {
        return id == Id;
    }

    public new bool HasSameId(object id)
    {
        return id is Guid guid && guid == Id;
    }

    public static new LogListPrimaryData From(Database.Entities.Log entity)
    {
        return new LogListPrimaryData()
        {
            Id = entity.Id,
            ActionId = entity.ActionId,
            Time = entity.Time,
            Success = entity.Success,
            UserId = entity.UserId,
            IPAddress = entity.IPAddress,
            ActionText = entity.ActionText,
            Request = entity.Request,
            Response = entity.Response,
            Target = entity.Target,
            Field = entity.Field,
            Before = entity.Before,
            After = entity.After,
            Message = entity.Message,
            Code = entity.Code,
            Exception = entity.Exception,
            InnerException = entity.InnerException
        };
    }
}