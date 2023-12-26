namespace SubtitleEditor.Core.Models;

/// <summary>
/// 簡單結果
/// </summary>
public interface ISimpleResult
{
    /// <summary>
    /// 是否成功
    /// </summary>
    bool Success { get; }

    /// <summary>
    /// 訊息
    /// </summary>
    string? Message { get; }

    /// <summary>
    /// 輸入一個簡單結果物件以取代此物件內的資訊。
    /// </summary>
    /// <param name="simpleResult">Simple Result</param>
    void Apply(ISimpleResult simpleResult);
}

/// <summary>
/// 簡單結果
/// </summary>
public class SimpleResult : ISimpleResult
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public virtual bool Success { get; set; }

    /// <summary>
    /// 訊息
    /// </summary>
    public virtual string? Message { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public SimpleResult() { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="success"></param>
    public SimpleResult(bool success)
    {
        Success = success;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    public SimpleResult(string? message)
    {
        Message = message;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="success"></param>
    /// <param name="message"></param>
    public SimpleResult(bool success, string? message)
    {
        Success = success;
        Message = message;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static ISimpleResult IsSuccess(string? message = null)
    {
        return new SimpleResult(true, message);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static ISimpleResult IsFailed(string? message = null)
    {
        return new SimpleResult(false, message);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="exception"></param>
    /// <returns></returns>
    public static ISimpleResult IsException(Exception exception)
    {
        return new SimpleResult(false, exception.ToString());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <param name="data"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public static ISimpleResult<TData> IsSuccess<TData>(TData? data = default, string? message = null)
    {
        return new SimpleResult<TData>(true, message) { Data = data };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static ISimpleResult<TData> IsFailed<TData>(string? message = null)
    {
        return new SimpleResult<TData>(false, message);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="exception"></param>
    /// <returns></returns>
    public static ISimpleResult<TData> IsException<TData>(Exception exception)
    {
        return new SimpleResult<TData>(false, exception.ToString());
    }

    /// <summary>
    /// 輸入一個簡單結果物件以取代此物件內的資訊。
    /// </summary>
    /// <param name="simpleResult">Simple Result</param>
    public void Apply(ISimpleResult simpleResult)
    {
        Success = simpleResult.Success;
        Message = simpleResult.Message;
    }
}

/// <summary>
/// 帶有資料的簡單結果
/// </summary>
/// <typeparam name="TData">資料型別</typeparam>
public interface ISimpleResult<TData> : ISimpleResult
{
    /// <summary>
    /// 資料
    /// </summary>
    TData? Data { get; set; }

    /// <summary>
    /// 輸入一個簡單結果物件以取代此物件內的資訊。
    /// </summary>
    /// <param name="simpleResult">Simple Result</param>
    void Replace(ISimpleResult<TData> simpleResult);
}

/// <summary>
/// 帶有資料的簡單結果
/// </summary>
/// <typeparam name="TData">資料型別</typeparam>
public class SimpleResult<TData> : SimpleResult, ISimpleResult<TData>
{
    /// <summary>
    /// 資料
    /// </summary>
    public TData? Data { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public SimpleResult() { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="success"></param>
    public SimpleResult(bool success) : base(success) { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    public SimpleResult(string? message) : base(message) { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="success"></param>
    /// <param name="message"></param>
    public SimpleResult(bool success, string? message) : base(success, message) { }

    /// <summary>
    /// 輸入一個簡單結果物件以取代此物件內的資訊。
    /// </summary>
    /// <param name="simpleResult">Simple Result</param>
    public void Replace(ISimpleResult<TData> simpleResult)
    {
        Success = simpleResult.Success;
        Message = simpleResult.Message;
        Data = simpleResult.Data;
    }
}