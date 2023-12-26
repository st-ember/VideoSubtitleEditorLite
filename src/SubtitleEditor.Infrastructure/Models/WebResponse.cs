namespace SubtitleEditor.Infrastructure.Models;

public class WebResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? Code { get; set; }

    public WebResponse() { }

    public WebResponse(bool success)
    {
        Success = success;
    }

    public WebResponse(bool success, string? message)
    {
        Success = success;
        Message = message;
    }
}

public class WebResponse<TData> : WebResponse
{
    public TData? Data { get; set; }

    public WebResponse() : base() { }

    public WebResponse(TData? data)
    {
        Data = data;
    }

    public WebResponse(bool success, TData? data)
    {
        Success = success;
        Data = data;
    }

    public WebResponse(TData? data, string? message)
    {
        Data = data;
        Message = message;
    }

    public WebResponse(bool success, TData? data, string? message)
    {
        Success = success;
        Data = data;
        Message = message;
    }
}