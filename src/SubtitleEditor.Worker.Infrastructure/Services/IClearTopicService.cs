namespace SubtitleEditor.Worker.Infrastructure.Services;

public interface IClearTopicService
{
    Task ProcessNextAsync(CancellationToken cancellationToken);
}
