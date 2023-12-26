using SubtitleEditor.Database.Entities;
using SubtitleEditor.Infrastructure.Models.Asr;
using System.Text.Json;

namespace SubtitleEditor.Infrastructure.Helpers;

public static class TopicHelper
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static void SetAsrTaskData(this Topic topic, NctuTask? nctuTask)
    {
        topic.AsrTask = nctuTask != null ? JsonSerializer.Serialize(nctuTask, _jsonSerializerOptions) : null;
    }

    public static void ClearAsrTaskData(this Topic topic)
    {
        topic.AsrTask = null;
    }

    public static NctuTask? GetAsrTaskData(this Topic topic)
    {
        return !string.IsNullOrWhiteSpace(topic.AsrTask) ? JsonSerializer.Deserialize<NctuTask>(topic.AsrTask, _jsonSerializerOptions) : null;
    }

    public static void SetModelName(this Topic topic, string? modelName)
    {
        var nctuTask = GetAsrTaskData(topic) ?? new();
        nctuTask.ModelName = modelName ?? string.Empty;
        SetAsrTaskData(topic, nctuTask);
    }

    public static string GetModelName(this Topic topic)
    {
        var nctuTask = GetAsrTaskData(topic) ?? new();
        return nctuTask.ModelName;
    }
}
