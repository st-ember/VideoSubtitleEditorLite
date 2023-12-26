using Microsoft.Extensions.DependencyInjection;
using SubtitleEditor.Infrastructure.ServiceImplements;
using SubtitleEditor.Infrastructure.Services;

namespace SubtitleEditor.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static void AddInfrastructureServices(this IServiceCollection services, Action<ServiceCollectionOptions>? optionFunc = null)
    {
        services.AddSingleton<IEncryptService, EncryptService>();
        services.AddSingleton<IFFMpegService, FFMpegService>();
        services.AddSingleton<ICacheService, CacheService>();
        services.AddSingleton<IFileService, FileService>();
        services.AddSingleton<IStreamFileService, StreamFileService>();
        services.AddSingleton<IApiService, ApiService>();
        services.AddSingleton<ISubtitleService, SubtitleService>();
        services.AddSingleton<IConfigurationService, ConfigurationService>();
        services.AddScoped<IAsrService, AsrService>();
        services.AddScoped<IActivationService, ActivationService>();
        services.AddScoped<ILogService, LogService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserGroupService, UserGroupService>();
        services.AddScoped<IUserMetaService, UserMetaService>();
        services.AddScoped(typeof(IGenericListService<>), typeof(GenericListService<>));

        var options = new ServiceCollectionOptions();
        if (optionFunc != null)
        {
            optionFunc(options);
        }

        if (options.UseSystemOptionConfiguration)
        {
            services.AddScoped<ISystemOptionService, SystemOptionService>();
        }
        else
        {
            services.AddScoped<ISystemOptionService, DBSystemOptionService>();
        }

        if (options.UseNctuFixBook)
        {
            services.AddScoped<IFixBookService, NctuFixBookService>();
        }
        else
        {
            services.AddScoped<IFixBookService, FixBookService>();
        }
    }
}
