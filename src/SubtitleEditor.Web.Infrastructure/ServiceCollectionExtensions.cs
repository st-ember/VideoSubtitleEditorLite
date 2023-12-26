using Microsoft.Extensions.DependencyInjection;
using SubtitleEditor.Web.Infrastructure.ServiceImplements;
using SubtitleEditor.Web.Infrastructure.Services;

namespace SubtitleEditor.Web.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static void AddWebInfrastructureServices(this IServiceCollection services)
    {
        services.AddSingleton<ISessionService, SessionService>();

        services.AddScoped<ICaptchaService, CaptchaService>();
        services.AddScoped<ILoginService, LoginService>();
        services.AddScoped<ITopicService, TopicService>();
        services.AddScoped<IBenchmarkService, BenchmarkService>();
    }
}
