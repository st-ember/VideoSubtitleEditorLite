namespace SubtitleEditor.Infrastructure.Services;

public interface IApiService
{
    void SetToken(string token);
    Task<string> GetAsync(string apiUrl, CancellationToken? cancellationToken = null);
    Task<TResponse> GetAsync<TResponse>(string apiUrl, CancellationToken? cancellationToken = null);
    Task<Stream> GetFileAsync(string apiUrl, CancellationToken? cancellationToken = null);
    Task<string> GetAsync(string apiUrl, Dictionary<string, string> parameters, CancellationToken? cancellationToken = null);
    Task<TResponse> GetAsync<TResponse>(string apiUrl, Dictionary<string, string> parameters, CancellationToken? cancellationToken = null);
    Task<Stream> GetFileAsync(string apiUrl, Dictionary<string, string> parameters, CancellationToken? cancellationToken = null);
    Task<string> PostFormAsync(string apiUrl, Dictionary<string, string> form, CancellationToken? cancellationToken = null);
    Task<TResponse> PostFormAsync<TResponse>(string apiUrl, Dictionary<string, string> form, CancellationToken? cancellationToken = null);
    Task<string> PostFormAsync(string apiUrl, MultipartFormDataContent formConten, CancellationToken? cancellationToken = null);
    Task<TResponse> PostFormAsync<TResponse>(string apiUrl, MultipartFormDataContent formContent, CancellationToken? cancellationToken = null);
    Task<string> PostAsync(string apiUrl, object data, CancellationToken? cancellationToken = null);
    Task<TResponse?> PostAsync<TResponse>(string apiUrl, object data, CancellationToken? cancellationToken = null);
    Task<string> DeleteAsync(string apiUrl, CancellationToken? cancellationToken = null);
    Task<TResponse?> DeleteAsync<TResponse>(string apiUrl, CancellationToken? cancellationToken = null);
}