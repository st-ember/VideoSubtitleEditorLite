using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Database;
using SubtitleEditor.Infrastructure;
using SubtitleEditor.Infrastructure.Models.SystemOption;
using SubtitleEditor.Infrastructure.Services;
using SubtitleEditor.Web.AuthenticizeObjects;
using SubtitleEditor.Web.Hubs;
using SubtitleEditor.Web.Infrastructure;
using SubtitleEditor.Web.Workers;
using SubtitleEditor.Worker.Infrastructure;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

var builder = WebApplication.CreateBuilder(args);

var dbPath = Path.Combine(Environment.CurrentDirectory, builder.Configuration["StorageFolder"], builder.Configuration["DBFolder"], builder.Configuration["DBFile"]);
var useSSL = !bool.TryParse(builder.Configuration["Security:UseSsl"], out var b) || b;
var detailedErrors = bool.TryParse(builder.Configuration["Security:DetailedErrors"], out var d) && d;
var useSystemOptionConfiguration = bool.TryParse(builder.Configuration["SystemOption:UseConfiguration"], out var uc) && uc;

builder.Services.AddHostedService<ServiceWorker>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(option =>
{
    option.IdleTimeout = TimeSpan.FromDays(1);
    option.Cookie.Name = ".VSE.Session";
    option.Cookie.HttpOnly = true;
    option.Cookie.SameSite = SameSiteMode.Strict;
    option.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// 設定使用 Cookie 維持驗證
builder.Services.AddAuthentication("VSEInstance")
    .AddCookie("VSEInstance", o =>
    {
        o.LoginPath = "/Account/Login";
        o.AccessDeniedPath = "/AccessDenied";
        o.Cookie.HttpOnly = true;
        o.Cookie.Name = ".VSE.Cookies";
        o.Cookie.Path = "/";
        o.Cookie.SameSite = SameSiteMode.Strict;
        o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        o.ExpireTimeSpan = TimeSpan.FromDays(7);
        o.SlidingExpiration = true;
        o.CookieManager = new CookieManager();
    });

// 設定 Cors，這裡用預設就行了。預設就是 Same-site 規則限制所有其他來源。
builder.Services.AddCors();

builder.Services.AddControllersWithViews()
    .AddJsonOptions(o =>
    {
        // 設定 Action 回傳的 Json 格式為小寫開頭，且在解析 Request 時忽略大小寫。
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        o.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.CjkUnifiedIdeographs);
    })
    .AddRazorRuntimeCompilation(); // Razor 在除錯時可以變更後馬上更新，方便開發。

builder.Services
    .AddSignalR(option =>
    {
        option.EnableDetailedErrors = detailedErrors;
    });

builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = long.MaxValue;
});

builder.Services.Configure<FormOptions>(o =>
{
    o.ValueLengthLimit = int.MaxValue;
    o.MultipartBodyLengthLimit = long.MaxValue;
});

builder.Services.AddHttpClient("SkipCertificate").ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddResponseCompression();

builder.Services.AddDatabase(dbPath);
builder.Services.AddInfrastructureServices(options => options.UseSystemOptionConfiguration = useSystemOptionConfiguration);
builder.Services.AddWebInfrastructureServices();
builder.Services.AddWorkerInfrastructureServices();

var provider = new FileExtensionContentTypeProvider();
provider.Mappings.Remove(".ts");
provider.Mappings.Add(".key", "text/plain");
provider.Mappings.Add(".m3u8", "application/x-mpegURL");
provider.Mappings.Add(".ts", "video/MP2T");

var app = builder.Build();
app.UsePathBase("/subtitle-editor");
app.Use(async (context, next) =>
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    // Log the PathBase here
    logger.LogInformation($"Request PathBase: {context.Request.PathBase}");
    await next.Invoke();
});

//app.UseFileServer(enableDirectoryBrowsing: true);

using (var scope = app.Services.CreateScope())
{
    var ffmpegService = scope.ServiceProvider.GetRequiredService<IFFMpegService>();
    await ffmpegService.DetermineFFMpegSourceAsync();

    var fileService = scope.ServiceProvider.GetRequiredService<IFileService>();
    fileService.InitializeFolders();
    fileService.InitializeStorageCache();

    var userGroupService = scope.ServiceProvider.GetRequiredService<IUserGroupService>();
    await userGroupService.EnsureDefaultGroupAsync();

    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
    await userService.EnsureDefaultUserAsync();

    var systemOptionService = scope.ServiceProvider.GetRequiredService<ISystemOptionService>();
    await systemOptionService.InitializeSystemOptionsAsync(new SystemOptionModel[]
    {
        new SystemOptionModel(SystemOptionNames.AsrUrl, "https://61.219.178.73:8451", "ASR 服務的網址"),
        new SystemOptionModel(SystemOptionNames.AsrUser, "gbm_admin", "ASR 服務的使用者"),
        new SystemOptionModel(SystemOptionNames.AsrSecret, "Gbm@2021", "ASR 服務的使用者密碼"),
        new SystemOptionModel(SystemOptionNames.SiteTitle, "Video Subtitle Editor", "系統標題"),
        new SystemOptionModel(SystemOptionNames.PasswordExpireDays, "90", "密碼最長使用天數"),
        new SystemOptionModel(SystemOptionNames.PasswordNoneRepeatCount, "3", "密碼不得重複次數"),
        new SystemOptionModel(SystemOptionNames.RawFileStorageLimit, "0", "原始媒體儲存庫容量上限(byte)"),
        new SystemOptionModel(SystemOptionNames.StreamFileStorageLimit, "0", "串流媒體儲存庫容量上限(byte)"),
        new SystemOptionModel(SystemOptionNames.LogoTicket, "[]", "Logo 檔案", "file-ticket")
    });
}

if (app.Environment.IsProduction() && !detailedErrors)
{
    app.UseExceptionHandler("/Error");

    if (useSSL)
    {
        app.UseHsts();
    }
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseSession();



if (useSSL)
{
    app.UseHttpsRedirection();
}

// 設定每個 Request 都會回傳的 Header 安全性標籤。
app.Use(async (context, next) =>
{
    // 讓收到的 Request 可以被重複讀取，因為很多 Action 會透過 Attribute 去紀錄 Log，所以必須要開啟否則 Log 會記錄失敗。
    context.Request.EnableBuffering();

    // 此要求的回應應該要被快取的秒數，如果為 NULL 則表示不快取。
    int? cacheAge = null;
    if (context.Request.Path.HasValue && !app.Environment.IsDevelopment())
    {
        // 只有可以存取 Path 以及在非開發環境時才快取。
        // wwwroot 底下的 images, css, js, banners 和下載音檔的 API 會進行快取，快取留存 1 天。
        var isStaticResource = new string[] { "images", "css", "js", "lib" }.Any(o => context.Request.Path.Value.ToLower().Contains(o));
        // 字體檔案不會變動，可以快取更久，快取留存 1 年。
        var isFontResource = context.Request.Path.Value.Contains("fonts");
        if (isStaticResource || isFontResource)
        {
            cacheAge = isFontResource ? 31_536_000 : 86_400;
        }
    }

    // 依照上面計算快取的結果寫入 Header 中。
    context.Response.Headers.Add("Cache-Control", cacheAge.HasValue ? $"public,max-age={cacheAge.Value}" : "no-store,no-cache");

    // 當 Response 要被送回給 Client 前，要檢查這次操作是否有成功，如果操作失敗則不該快取。
    context.Response.OnStarting(() =>
    {
        // 透過 StatusCode 來檢查操作是否成功。
        if (context.Response.StatusCode >= 400 && context.Response.StatusCode < 600)
        {
            // 如果操作失敗則刪除 Cache-Control Header。
            context.Response.Headers.Remove("Cache-Control");
        }
        return Task.CompletedTask;
    });

    // 安全性標籤
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");

    // 如果不是在開發環境(本機)，要加上 CSR Policy。
    if (!app.Environment.IsDevelopment())
    {
        // 因為會用到來自 CDN 的 library，所以 CDN 也要加上去。
        context.Response.Headers.Add("Content-Security-Policy",
            "base-uri 'self';default-src 'self';object-src 'none';media-src 'self' blob:;frame-src 'none';img-src 'self' data:;" +
            "frame-ancestors 'none';form-action 'self';" +
            "font-src 'self' https://cdnjs.cloudflare.com data:;" +
            "style-src 'self' https://cdnjs.cloudflare.com https://cdn.jsdelivr.net https://unpkg.com 'sha256-47DEQpj8HBSa+/TImW+5JCeuQeRkm5NMpJWZG3hSuFU=' 'sha256-wPXhisdsFu1DtHYH1D9W5isSGqS5vIPn6QJWSNLqfCM=';" +
            "script-src 'self' https://cdnjs.cloudflare.com https://cdn.jsdelivr.net https://unpkg.com blob: 'sha256-0pvg8ol8mxIrtPPNjDULIMb1tv9qdKrjqUbzufiAI1U=' 'sha256-1cSEbjrR4Sng0qR6JIahu3I9XWRuTZNiifHGXfuieNQ=' 'sha256-lZibAL/LUoVC8wp8c0cstF/6u4aKWhsP3mw2F2mVuE4=';" +
            "connect-src 'self'"
            );
    }

    await next();
});


//app.UseStaticFiles(new StaticFileOptions
//{
//    ContentTypeProvider = provider
//});

app.UseStaticFiles();



app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseCookiePolicy();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<SessionHub>("/hub/session");

    //endpoints.MapControllerRoute(
    //    name: "error",
    //    pattern: "error",
    //    new { controller = "Home", action = "Error" });

    endpoints.MapControllerRoute(
        name: "login",
        pattern: "login",
        new { controller = "Account", action = "Login" });

    endpoints.MapControllerRoute(
        name: "logout",
        pattern: "logout",
        new { controller = "Account", action = "Logout" });

    endpoints.MapControllerRoute(
        name: "checkStream",
        pattern: "stream/check/{id}",
        defaults: new { Controller = "Stream", Action = "Check" }
        );
    endpoints.MapControllerRoute(
        name: "getStream",
        pattern: "stream/get/{id}",
        defaults: new { Controller = "Stream", Action = "Get" }
        );

    endpoints.MapControllerRoute(
        name: "topicList",
        pattern: "topics",
        defaults: new { Controller = "Topic", Action = "List" }
        );

    endpoints.MapControllerRoute(
        name: "editor",
        pattern: "editor",
        defaults: new { Controller = "Editor", Action = "Subtitle" }
        );

    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Entry}"
        );
});

app.Run();
