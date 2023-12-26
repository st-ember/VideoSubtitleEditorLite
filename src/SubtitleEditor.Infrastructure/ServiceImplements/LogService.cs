using Microsoft.AspNetCore.Mvc.Filters;
using SubtitleEditor.Database;
using SubtitleEditor.Database.Entities;
using SubtitleEditor.Infrastructure.Models;
using SubtitleEditor.Infrastructure.Services;
using System.Text.Json;

namespace SubtitleEditor.Infrastructure.ServiceImplements;

public class LogService : ILogService
{
    public virtual Log? Log { get; protected set; }

    public string ActionText
    {
        get => Log?.ActionText ?? "";
        set
        {
            if (Log != null)
            {
                Log.ActionText = value;
            }
        }
    }

    public string? Message
    {
        get => Log?.Message;
        set
        {
            if (Log != null)
            {
                Log.Message = value;
            }
        }
    }

    public string? Target
    {
        get => Log?.Target;
        set
        {
            if (Log != null)
            {
                Log.Target = value;
            }
        }
    }

    public string? Field
    {
        get => Log?.Field;
        set
        {
            if (Log != null)
            {
                Log.Field = value;
            }
        }
    }

    public string? Before
    {
        get => Log?.Before;
        set
        {
            if (Log != null)
            {
                Log.Before = value;
            }
        }
    }

    public string? After
    {
        get => Log?.After;
        set
        {
            if (Log != null)
            {
                Log.After = value;
            }
        }
    }

    public string? Code
    {
        get => Log?.Code ?? "";
        set
        {
            if (Log != null)
            {
                Log.Code = value;
            }
        }
    }

    public virtual Guid? UserId
    {
        get => Log?.UserId;
        set
        {
            if (Log != null)
            {
                Log.UserId = value;
            }
        }
    }

    public virtual object? Request
    {
        set
        {
            if (Log != null)
            {
                Log.Request = SerializerJson(value);
            }
        }
    }

    public virtual object? Response
    {
        set
        {
            if (Log != null)
            {
                Log.Response = SerializerJson(value);
            }
        }
    }

    public Guid? ActionId => Log?.ActionId;

    public bool OnlyLogOnError { get; set; } = false;

    /// <summary>
    /// 額外的紀錄物件清單，這些物件會在 End() 被觸發的時候一起寫入資料庫。
    /// </summary>
    protected List<Log> Logs { get; set; } = new List<Log>();

    protected virtual EditorContext Database { get; set; }
    protected virtual IAccountService AccountService { get; set; }

    protected virtual string? LoginUserIPAddress => AccountService?.GetUserIPAddress();

    protected virtual JsonSerializerOptions JsonSerializerOptions { get; set; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public LogService(
        EditorContext database,
        IAccountService accountService
        )
    {
        Database = database;
        AccountService = accountService;
    }

    protected virtual Log GenerateNewAnonymousPrimaryLog()
    {
        var log = new Log()
        {
            UserId = null,
            IPAddress = LoginUserIPAddress,
            Success = true,
            Primary = true
        };

        log.ActionId = log.Id;
        return log;
    }

    protected virtual Log GenerateNewAuthorizedPrimaryLog()
    {
        var log = new Log()
        {
            UserId = AccountService?.GetLoginUserId(),
            IPAddress = LoginUserIPAddress,
            Success = true,
            Primary = true
        };

        log.ActionId = log.Id;
        return log;
    }

    protected virtual Log GenerateNewPrimaryLog()
    {
        return AccountService != default && AccountService.IsLogined() ? GenerateNewAuthorizedPrimaryLog() : GenerateNewAnonymousPrimaryLog();
    }

    public virtual Log GenerateNewLog()
    {
        return AppendLog(new Log
        {
            Success = true,
            ActionId = ActionId ?? Guid.NewGuid(),
            ActionText = Log?.ActionText ?? "",
            UserId = UserId,
            IPAddress = LoginUserIPAddress
        });
    }

    public virtual async Task<ILogResult> StartAsync(string logAction, Action func)
    {
        Log = GenerateNewPrimaryLog();
        Logs = new List<Log>();
        ActionText = logAction;
        try
        {
            func();
        }
        catch (Exception ex)
        {
            Exception(ex);
        }

        return await EndAsync();
    }

    public virtual async Task<ILogResult> StartAsync(string logAction, Func<Task> func)
    {
        Log = GenerateNewPrimaryLog();
        Logs = new List<Log>();
        ActionText = logAction;
        try
        {
            await func();
        }
        catch (Exception ex)
        {
            Exception(ex);
        }

        return await EndAsync();
    }

    public virtual async Task<ILogResult<T>> StartAsync<T>(string logAction, Func<T> func)
    {
        Log = GenerateNewPrimaryLog();
        Logs = new List<Log>();
        ActionText = logAction;
        T? data = default;
        try
        {
            data = func();
        }
        catch (Exception ex)
        {
            Exception(ex);
        }

        return await EndAsync(data);
    }

    public virtual async Task<ILogResult<T>> StartAsync<T>(string logAction, Func<Task<T>> func)
    {
        Log = GenerateNewPrimaryLog();
        Logs = new List<Log>();
        ActionText = logAction;
        T? data = default;
        try
        {
            data = await func();
        }
        catch (Exception ex)
        {
            Exception(ex);
        }

        return await EndAsync(data);
    }

    public virtual async Task<ILogResult<ActionExecutedContext>> StartAsync(string logAction, Func<Task<ActionExecutedContext>> func)
    {
        Log = GenerateNewPrimaryLog();
        Logs = new List<Log>();
        ActionText = logAction;
        ActionExecutedContext? data = default;
        try
        {
            data = await func();
        }
        catch (Exception ex)
        {
            Exception(ex);
        }

        return await EndAsync(data);
    }

    public virtual Task<ILogResult> StartAsync<TEnum>(TEnum logAction, Action func) where TEnum : Enum
    {
        return StartAsync(logAction.ToString(), func);
    }

    public virtual Task<ILogResult> StartAsync<TEnum>(TEnum logAction, Func<Task> func) where TEnum : Enum
    {
        return StartAsync(logAction.ToString(), func);
    }

    public virtual Task<ILogResult<T>> StartAsync<T, TEnum>(TEnum logAction, Func<T> func) where TEnum : Enum
    {
        return StartAsync(logAction.ToString(), func);
    }

    public virtual Task<ILogResult<T>> StartAsync<T, TEnum>(TEnum logAction, Func<Task<T>> func) where TEnum : Enum
    {
        return StartAsync(logAction.ToString(), func);
    }

    public virtual Task<ILogResult<ActionExecutedContext>> StartAsync<TEnum>(TEnum logAction, Func<Task<ActionExecutedContext>> func) where TEnum : Enum
    {
        return StartAsync(logAction.ToString(), func);
    }

    protected virtual void Exception(Exception exception)
    {
        Console.WriteLine(exception.ToString());
        CheckDatabase();

        Log ??= GenerateNewPrimaryLog();

        Database!.DetachAllEntities();
        Log.Success = false;
        Log.Message = exception.Message;
        Log.Exception = exception.ToString();
        Log.InnerException = exception.InnerException?.ToString();
    }

    protected virtual async Task<ILogResult> EndAsync()
    {
        CheckDatabase();

        Log ??= GenerateNewPrimaryLog();
        Log.Time = DateTime.Now;

        foreach (var log in Logs)
        {
            Database!.Logs.Add(log);
        }

        if (!OnlyLogOnError || OnlyLogOnError && !Log.Success)
        {
            Database!.Logs.Add(Log);
        }

        await Database!.SaveChangesAsync();

        return LogResult.From(Log);
    }

    protected virtual async Task<ILogResult<T>> EndAsync<T>(T? data)
    {
        CheckDatabase();

        Log ??= GenerateNewPrimaryLog();
        Log.Time = DateTime.Now;
        Response = SerializerJson(data);

        foreach (var log in Logs)
        {
            Database!.Logs.Add(log);
        }

        if (!OnlyLogOnError || OnlyLogOnError && !Log.Success)
        {
            Database!.Logs.Add(Log);
        }

        await Database!.SaveChangesAsync();

        return LogResult<T>.From(Log, data);
    }

    /// <summary>
    /// 給 ActionFilter 專用的 EndAsync()，收到 data 後需要判斷是否有例外發生，並攔截例外來避免 MVC 回傳錯誤畫面。
    /// </summary>
    /// <param name="data"></param>
    protected virtual async Task<ILogResult<ActionExecutedContext>> EndAsync(ActionExecutedContext? data)
    {
        CheckDatabase();

        Log ??= GenerateNewPrimaryLog();

        if (data != null)
        {
            if (data.Exception != null)
            {
                Exception(data.Exception);
                data.Exception = null; // 攔截例外
            }

            if (data.Result is Microsoft.AspNetCore.Mvc.JsonResult jsonResult)
            {
                Response = jsonResult.Value;
            }
        }

        Log.Time = DateTime.Now;

        foreach (var log in Logs)
        {
            Database!.Logs.Add(log);
        }

        if (!OnlyLogOnError || OnlyLogOnError && !Log.Success)
        {
            Database!.Logs.Add(Log);
        }

        await Database!.SaveChangesAsync();

        return LogResult<ActionExecutedContext>.From(Log, data);
    }

    protected virtual string? SerializerJson<T>(T? data)
    {
        try
        {
            return data != null ?
                data is string json ? json : JsonSerializer.Serialize(data, JsonSerializerOptions) :
                null;
        }
        catch
        {
            return "Serialize failed";
        }
    }

    public virtual Log AppendLog(Log log)
    {
        Logs.Add(log);
        return log;
    }

    public virtual Log SystemInfo(string message, string? target = "", string? field = "", string? before = "", string? after = "")
    {
        return AppendLog(new Log
        {
            Success = true,
            ActionId = ActionId ?? Guid.NewGuid(),
            ActionText = Log?.ActionText ?? "",
            Message = message,
            Target = target,
            Field = field,
            Before = before,
            After = after,
            UserId = UserId,
            IPAddress = LoginUserIPAddress
        });
    }

    public virtual Log SystemError(string message)
    {
        return AppendLog(new Log
        {
            Success = false,
            ActionId = ActionId ?? Guid.NewGuid(),
            ActionText = Log?.ActionText ?? "",
            Message = message,
            UserId = UserId,
            IPAddress = LoginUserIPAddress
        });
    }

    public virtual Log SystemError(Exception exception)
    {
        return AppendLog(new Log
        {
            Success = false,
            ActionId = ActionId ?? Guid.NewGuid(),
            ActionText = Log?.ActionText ?? "",
            Message = exception.Message,
            Exception = exception.ToString(),
            InnerException = exception.InnerException?.ToString(),
            UserId = UserId,
            IPAddress = LoginUserIPAddress
        });
    }

    public virtual async Task SaveLogAsync()
    {
        CheckDatabase();

        if (Logs.Any())
        {
            foreach (var log in Logs)
            {
                Database!.Logs.Add(log);
            }

            await Database!.SaveChangesAsync();
            Logs.Clear();
        }
    }

    protected virtual void CheckDatabase()
    {
        if (Database == null)
        {
            throw new Exception("Database is null");
        }
    }
}