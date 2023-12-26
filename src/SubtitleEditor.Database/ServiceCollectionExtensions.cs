using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SubtitleEditor.Database;

public static class ServiceCollectionExtensions
{
    public static void AddDatabase(this IServiceCollection services, string path)
    {
        services.AddSqlite<EditorContext>($"Data Source={path}");
    }

    public static EditorContext GetDatabase(this IServiceProvider serviceProvider)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var path = Path.Combine(Environment.CurrentDirectory, configuration["StorageFolder"], configuration["DBFolder"], configuration["DBFile"]);
        var dbContextOptions = new DbContextOptionsBuilder<EditorContext>().UseSqlite($"Data Source={path}").Options;
        return new EditorContext(dbContextOptions);
    }
}
