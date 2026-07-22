namespace MarbleWebProject.Services.Api;

public interface IStoreApiClient
{
    Task<T> PostAsync<T>(string path, object? body, string? bearerToken = null, CancellationToken cancellationToken = default, IReadOnlyDictionary<string, string>? extraHeaders = null);
    Task<T> GetAsync<T>(string path, string? bearerToken = null, CancellationToken cancellationToken = default, IReadOnlyDictionary<string, string>? extraHeaders = null);
    Task<string> GetStringAsync(string path, string? bearerToken = null, CancellationToken cancellationToken = default);
    Task<byte[]> GetBytesAsync(string path, string? bearerToken = null, CancellationToken cancellationToken = default);
    Task<T> PutAsync<T>(string path, object? body, string? bearerToken = null, CancellationToken cancellationToken = default, IReadOnlyDictionary<string, string>? extraHeaders = null);
    Task<T> DeleteAsync<T>(string path, string? bearerToken = null, CancellationToken cancellationToken = default);
}
