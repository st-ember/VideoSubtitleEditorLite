using System.Text.Json;

namespace SubtitleEditor.Infrastructure.Models.UserMeta;

public class UserMetaData
{
    public Guid UserId { get; set; }
    public string Key { get; set; } = null!;
    public string? Data { get; set; }

    public T? GetData<T>()
    {
        return GetData<T>(default);
    }

    public T? GetData<T>(T? defaultValue)
    {
        return !string.IsNullOrWhiteSpace(Data) ?
            JsonSerializer.Deserialize<T>(Data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }) :
            defaultValue;
    }

    public void SetData<T>(T data)
    {
        Data = data != null ?
            JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }) : null;
    }
}