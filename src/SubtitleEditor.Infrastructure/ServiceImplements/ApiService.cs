using SubtitleEditor.Infrastructure.Services;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Web;

namespace SubtitleEditor.Infrastructure.ServiceImplements;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ApiService(
        IHttpClientFactory httpClientFactory
        )
    {
        _httpClient = httpClientFactory.CreateClient("SkipCertificate");
        _httpClient.Timeout = TimeSpan.FromSeconds(600);
    }

    public void SetToken(string token)
    {
        if (!string.IsNullOrWhiteSpace(token))
        {
            var array = token.Split(' ');
            _httpClient.DefaultRequestHeaders.Authorization = array.Length == 2 ?
                new AuthenticationHeaderValue(array.First(), array.Last()) :
                new AuthenticationHeaderValue("bearer", token);
        }
    }

    public Task<TResponse> GetAsync<TResponse>(string apiUrl, CancellationToken? cancellationToken = null)
    {
        return GetAsync<TResponse>(apiUrl, null, cancellationToken);
    }

    public async Task<TResponse> GetAsync<TResponse>(string apiUrl, Dictionary<string, string>? parameters = null, CancellationToken? cancellationToken = null)
    {
        var responseText = await GetAsync(apiUrl, parameters, cancellationToken);
        return JsonSerializer.Deserialize<TResponse>(responseText, _jsonSerializerOptions)!;
    }

    public Task<string> GetAsync(string apiUrl, CancellationToken? cancellationToken = null)
    {
        return GetAsync(apiUrl, null, cancellationToken);
    }

    public async Task<string> GetAsync(string apiUrl, Dictionary<string, string>? parameters = null, CancellationToken? cancellationToken = null)
    {
        var query = parameters != null ? string.Join("&", parameters.Select(pair => $"{pair.Key}={HttpUtility.UrlEncode(pair.Value)}")) : "";
        var adoptedUrl = $"{apiUrl}{(!string.IsNullOrEmpty(query) ? $"?{query}" : "")}";
        var response = cancellationToken.HasValue ? await _httpClient.GetAsync(adoptedUrl, cancellationToken.Value) : await _httpClient.GetAsync(adoptedUrl);
        return await response.Content.ReadAsStringAsync();
    }

    public Task<Stream> GetFileAsync(string apiUrl, CancellationToken? cancellationToken = null)
    {
        return GetFileAsync(apiUrl, null, cancellationToken);
    }

    public async Task<Stream> GetFileAsync(string apiUrl, Dictionary<string, string>? parameters = null, CancellationToken? cancellationToken = null)
    {
        var query = parameters != null ? string.Join("&", parameters.Select(pair => $"{pair.Key}={HttpUtility.UrlEncode(pair.Value)}")) : "";
        var adoptedUrl = $"{apiUrl}{(!string.IsNullOrEmpty(query) ? $"?{query}" : "")}";
        var response = cancellationToken.HasValue ? await _httpClient.GetAsync(adoptedUrl, cancellationToken.Value) : await _httpClient.GetAsync(adoptedUrl);
        return await response.Content.ReadAsStreamAsync();
        //if (response.Headers.Contains("transfer-encoding") && response.Headers.GetValues("transfer-encoding").Any(v => v == "chunked"))
        //{
        //    using var stream = await response.Content.ReadAsStreamAsync();
        //    var buffer = new List<byte>();
        //    var contentBuffer = new List<byte>();

        //    int? chunkSize = null;
        //    while (true)
        //    {
        //        var b = stream.ReadByte();
        //        if (b == -1)
        //        {
        //            if (buffer.Count > 0)
        //            {
        //                buffer.ForEach(o => contentBuffer.Add(o));
        //            }
        //            break;
        //        }

        //        if (chunkSize.HasValue)
        //        {
        //            if (buffer.Count < chunkSize.Value)
        //            {
        //                buffer.Add(Convert.ToByte(b));
        //            }
        //            else
        //            {
        //                buffer.ForEach(o => contentBuffer.Add(o));
        //                buffer.Clear();
        //                chunkSize = null;
        //            }
        //        }
        //        else
        //        {
        //            buffer.Add(Convert.ToByte(b));

        //            var text = Encoding.UTF8.GetString(buffer.ToArray(), 0, buffer.Count);
        //            if (text == "\n")
        //            {
        //                buffer.Clear();
        //            }
        //            else if (text.Contains('\n'))
        //            {
        //                var validSize = int.TryParse(text, System.Globalization.NumberStyles.HexNumber, null, out int num);
        //                if (validSize)
        //                {
        //                    chunkSize = num;
        //                    buffer.Clear();
        //                }
        //            }
        //        }
        //    }

        //    return new MemoryStream(contentBuffer.ToArray());
        //}
        //else
        //{
        //    return await response.Content.ReadAsStreamAsync();
        //}
    }

    public async Task<TResponse> PostFormAsync<TResponse>(string apiUrl, Dictionary<string, string> form, CancellationToken? cancellationToken = null)
    {
        var responseText = await PostFormAsync(apiUrl, form, cancellationToken);
        return JsonSerializer.Deserialize<TResponse>(responseText, _jsonSerializerOptions)!;
    }

    public async Task<string> PostFormAsync(string apiUrl, Dictionary<string, string> form, CancellationToken? cancellationToken = null)
    {
        var formContent = new FormUrlEncodedContent(form);
        var response = cancellationToken.HasValue ? await _httpClient.PostAsync(apiUrl, formContent, cancellationToken.Value) : await _httpClient.PostAsync(apiUrl, formContent);
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<TResponse> PostFormAsync<TResponse>(string apiUrl, MultipartFormDataContent formContent, CancellationToken? cancellationToken = null)
    {
        var responseText = await PostFormAsync(apiUrl, formContent, cancellationToken);
        return JsonSerializer.Deserialize<TResponse>(responseText, _jsonSerializerOptions)!;
    }

    public async Task<string> PostFormAsync(string apiUrl, MultipartFormDataContent formContent, CancellationToken? cancellationToken = null)
    {
        var response = cancellationToken.HasValue ? await _httpClient.PostAsync(apiUrl, formContent, cancellationToken.Value) : await _httpClient.PostAsync(apiUrl, formContent);
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> PostAsync(string apiUrl, object data, CancellationToken? cancellationToken = null)
    {
        var httpContent = new StringContent(
            data != null ? JsonSerializer.Serialize(data, _jsonSerializerOptions) : "{}",
            Encoding.UTF8,
            "application/json"
            );

        var response = cancellationToken.HasValue ? await _httpClient.PostAsync(apiUrl, httpContent, cancellationToken.Value) : await _httpClient.PostAsync(apiUrl, httpContent);
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<TResponse?> PostAsync<TResponse>(string apiUrl, object data, CancellationToken? cancellationToken = null)
    {
        var responseString = await PostAsync(apiUrl, data, cancellationToken);
        return !string.IsNullOrWhiteSpace(responseString) ?
            JsonSerializer.Deserialize<TResponse>(responseString, _jsonSerializerOptions) : default;
    }

    public async Task<string> DeleteAsync(string apiUrl, CancellationToken? cancellationToken = null)
    {
        var response = cancellationToken.HasValue ? await _httpClient.DeleteAsync(apiUrl, cancellationToken.Value) : await _httpClient.DeleteAsync(apiUrl);
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<TResponse?> DeleteAsync<TResponse>(string apiUrl, CancellationToken? cancellationToken = null)
    {
        var responseString = await DeleteAsync(apiUrl, cancellationToken);
        return !string.IsNullOrWhiteSpace(responseString) ?
            JsonSerializer.Deserialize<TResponse>(responseString, _jsonSerializerOptions) : default;
    }
}
