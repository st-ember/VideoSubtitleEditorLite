using Microsoft.Extensions.DependencyInjection;
using SubtitleEditor.Worker.Infrastructure.ServiceImplements;
using SubtitleEditor.Worker.Infrastructure.Services;

namespace SubtitleEditor.Worker.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static void AddWorkerInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<IAsrProcessService, AsrProcessService>();
        services.AddScoped<IStreamConvertService, StreamConvertService>();
        services.AddScoped<IClearTopicService, ClearTopicService>();
    }
}
