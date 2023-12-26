namespace SubtitleEditor.Worker.Infrastructure.Services;

public interface IAsrProcessService
{
    Task ProcessNextAsync(CancellationToken cancellationToken);
}
