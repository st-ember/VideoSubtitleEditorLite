using SubtitleEditor.Core.Abstract;
using SubtitleEditor.Core.Models;

namespace SubtitleEditor.Infrastructure.Models;

public interface ILogResult : ISimpleResult
{
    string? Code { get; set; }
    string? Exception { get; set; }
}

public class LogResult : SimpleResult, ILogResult
{
    public override bool Success { get; set; } = true;

    public string? Code { get; set; }

    public string? Exception { get; set; }

    public static LogResult From(ILog log)
    {
        return new LogResult()
        {
            Success = log.Success,
            Message = log.Message,
            Code = log.Code,
            Exception = log.Exception
        };
    }
}

public interface ILogResult<T> : ILogResult, ISimpleResult<T> { }

public class LogResult<T> : LogResult, ILogResult<T>
{
    public T? Data { get; set; }

    public void Replace(ISimpleResult<T> simpleResult)
    {
        Success = simpleResult.Success;
        Message = simpleResult.Message;
        Data = simpleResult.Data;
    }

    public static LogResult<T> From(ILog log, T? data)
    {
        return new LogResult<T>()
        {
            Success = log.Success,
            Message = log.Message,
            Code = log.Code,
            Exception = log.Exception,
            Data = data
        };
    }
}