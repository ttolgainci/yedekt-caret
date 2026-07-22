using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using MarbleWebProject.Models.Options;
using Microsoft.Extensions.Options;

namespace MarbleWebProject.Services.Api;

public sealed class StoreApiClient : IStoreApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = null,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    private readonly HttpClient _http;

    public StoreApiClient(HttpClient http, IOptions<ApiServiceOptions> options)
    {
        _http = http;
        var baseUrl = (options.Value.BaseUrl ?? "").Trim().TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new InvalidOperationException("ApiService:BaseUrl is not configured.");
        _http.BaseAddress = new Uri(baseUrl + "/");
        _http.Timeout = TimeSpan.FromMinutes(2);
    }

    public async Task<T> PostAsync<T>(string path, object? body, string? bearerToken = null, CancellationToken cancellationToken = default, IReadOnlyDictionary<string, string>? extraHeaders = null)
    {
        var relative = (path ?? "").Trim();
        if (!relative.StartsWith('/'))
            relative = "/" + relative;

        using var request = new HttpRequestMessage(HttpMethod.Post, relative);
        if (!string.IsNullOrWhiteSpace(bearerToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        if (extraHeaders != null)
        {
            foreach (var h in extraHeaders)
                request.Headers.TryAddWithoutValidation(h.Key, h.Value);
        }

        if (body != null)
            request.Content = JsonContent.Create(body, options: JsonOptions);

        using var response = await _http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"API {(int)response.StatusCode} {response.ReasonPhrase} for POST {relative}. Body: {errorBody}");
        }

        var data = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
        if (data == null)
            throw new InvalidOperationException($"Empty API response for POST {relative}.");
        return data;
    }

    public async Task<T> GetAsync<T>(string path, string? bearerToken = null, CancellationToken cancellationToken = default, IReadOnlyDictionary<string, string>? extraHeaders = null)
    {
        var relative = (path ?? "").Trim();
        if (!relative.StartsWith('/'))
            relative = "/" + relative;

        using var request = new HttpRequestMessage(HttpMethod.Get, relative);
        if (!string.IsNullOrWhiteSpace(bearerToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        if (extraHeaders != null)
        {
            foreach (var h in extraHeaders)
                request.Headers.TryAddWithoutValidation(h.Key, h.Value);
        }

        using var response = await _http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"API {(int)response.StatusCode} {response.ReasonPhrase} for GET {relative}. Body: {errorBody}");
        }

        var data = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
        if (data == null)
            throw new InvalidOperationException($"Empty API response for GET {relative}.");
        return data;
    }

    public async Task<string> GetStringAsync(string path, string? bearerToken = null, CancellationToken cancellationToken = default)
    {
        var relative = (path ?? "").Trim();
        if (!relative.StartsWith('/'))
            relative = "/" + relative;

        using var request = new HttpRequestMessage(HttpMethod.Get, relative);
        if (!string.IsNullOrWhiteSpace(bearerToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await _http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"API {(int)response.StatusCode} {response.ReasonPhrase} for GET {relative}. Body: {errorBody}");
        }

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public async Task<byte[]> GetBytesAsync(string path, string? bearerToken = null, CancellationToken cancellationToken = default)
    {
        var relative = (path ?? "").Trim();
        if (!relative.StartsWith('/'))
            relative = "/" + relative;

        using var request = new HttpRequestMessage(HttpMethod.Get, relative);
        if (!string.IsNullOrWhiteSpace(bearerToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await _http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"API {(int)response.StatusCode} {response.ReasonPhrase} for GET {relative}. Body: {errorBody}");
        }

        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }

    public async Task<T> PutAsync<T>(string path, object? body, string? bearerToken = null, CancellationToken cancellationToken = default, IReadOnlyDictionary<string, string>? extraHeaders = null)
    {
        var relative = (path ?? "").Trim();
        if (!relative.StartsWith('/'))
            relative = "/" + relative;

        using var request = new HttpRequestMessage(HttpMethod.Put, relative);
        if (!string.IsNullOrWhiteSpace(bearerToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        if (extraHeaders != null)
        {
            foreach (var h in extraHeaders)
                request.Headers.TryAddWithoutValidation(h.Key, h.Value);
        }

        if (body != null)
            request.Content = JsonContent.Create(body, options: JsonOptions);

        using var response = await _http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"API {(int)response.StatusCode} {response.ReasonPhrase} for PUT {relative}. Body: {errorBody}");
        }

        var data = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
        if (data == null)
            throw new InvalidOperationException($"Empty API response for PUT {relative}.");
        return data;
    }

    public async Task<T> DeleteAsync<T>(string path, string? bearerToken = null, CancellationToken cancellationToken = default)
    {
        var relative = (path ?? "").Trim();
        if (!relative.StartsWith('/'))
            relative = "/" + relative;

        using var request = new HttpRequestMessage(HttpMethod.Delete, relative);
        if (!string.IsNullOrWhiteSpace(bearerToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await _http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"API {(int)response.StatusCode} {response.ReasonPhrase} for DELETE {relative}. Body: {errorBody}");
        }

        var data = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
        if (data == null)
            throw new InvalidOperationException($"Empty API response for DELETE {relative}.");
        return data;
    }
}
