using SubtitleEditor.Core.Abstract;

namespace SubtitleEditor.Web.Infrastructure.Models.Log;

public class LogListData : IWithId<Guid>
{
    /// <summary>
    /// ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 時間
    /// </summary>
    public DateTime Time { get; set; }

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 操作目標
    /// </summary>
    public string? Target { get; set; }

    /// <summary>
    /// 操作目標欄位
    /// </summary>
    public string? Field { get; set; }

    /// <summary>
    /// 操作前資料
    /// </summary>
    public string? Before { get; set; }

    /// <summary>
    /// 操作後資料
    /// </summary>
    public string? After { get; set; }

    /// <summary>
    /// 操作產生的訊息
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 紀錄事件的簡易代碼
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// 例外內容
    /// </summary>
    public string? Exception { get; set; }

    /// <summary>
    /// 內部例外內容
    /// </summary>
    public string? InnerException { get; set; }

    public Guid GetGenericId()
    {
        return Id;
    }

    public object GetId()
    {
        return Id;
    }

    public bool HasSameId(Guid id)
    {
        return id == Id;
    }

    public bool HasSameId(object id)
    {
        return id is Guid guid && guid == Id;
    }

    public static LogListData From(Database.Entities.Log entity)
    {
        return new LogListData()
        {
            Id = entity.Id,
            Time = entity.Time,
            Success = entity.Success,
            Target = entity.Target,
            Field = entity.Field,
            Before = entity.Before,
            After = entity.After,
            Message = entity.Message,
            Code = entity.Code,
            Exception = entity.Exception,
            InnerException = entity.InnerException,
        };
    }
}
