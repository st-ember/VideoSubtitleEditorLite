namespace SubtitleEditor.Worker.Infrastructure.Services;

public interface IStreamConvertService
{
    Task ProcessNextAsync(CancellationToken cancellationToken);
}
