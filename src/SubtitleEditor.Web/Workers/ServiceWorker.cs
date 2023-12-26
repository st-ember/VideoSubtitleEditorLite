using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Infrastructure.Services;
using SubtitleEditor.Worker.Infrastructure.Services;

namespace SubtitleEditor.Web.Workers;

public class ServiceWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private Task? _asrTask;
    private Task? _streamConvertTask;
    private Task? _clearTopicTask;

    public ServiceWorker(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _doAsyncWork(stoppingToken);
            await Task.Delay(10_000, stoppingToken);
        }
    }

    private void _doAsyncWork(CancellationToken cancellationToken)
    {
        _asrTask ??= Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var asrProcessService = scope.ServiceProvider.GetRequiredService<IAsrProcessService>();
            var logService = scope.ServiceProvider.GetRequiredService<ILogService>();
            logService.OnlyLogOnError = true;

            await logService.StartAsync(SystemAction.ProcessAsr, () => asrProcessService.ProcessNextAsync(cancellationToken));

            await Task.Delay(10_000);
            _asrTask = null;
        }, cancellationToken);

        _streamConvertTask ??= Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var streamConvertService = scope.ServiceProvider.GetRequiredService<IStreamConvertService>();
            var logService = scope.ServiceProvider.GetRequiredService<ILogService>();
            logService.OnlyLogOnError = true;

            await logService.StartAsync(SystemAction.ProcessStreamConvert, () => streamConvertService.ProcessNextAsync(cancellationToken));

            await Task.Delay(10_000);
            _streamConvertTask = null;
        }, cancellationToken);

        _clearTopicTask ??= Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var clearTopicService = scope.ServiceProvider.GetRequiredService<IClearTopicService>();
            var logService = scope.ServiceProvider.GetRequiredService<ILogService>();
            logService.OnlyLogOnError = true;

            await logService.StartAsync(SystemAction.ProcessClearTopic, () => clearTopicService.ProcessNextAsync(cancellationToken));

            await Task.Delay(3600_000);
            _clearTopicTask = null;
        }, cancellationToken);
    }
}
