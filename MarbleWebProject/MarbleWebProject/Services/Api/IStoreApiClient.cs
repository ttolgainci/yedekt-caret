namespace MarbleWebProject.Services.Api;

public interface IStoreApiClient
{
    Task<T> PostAsync<T>(string path, object? body, string? bearerToken = null, CancellationToken cancellationToken = default, IReadOnlyDictionary<string, string>? extraHeaders = null);
    Task<T> GetAsync<T>(string path, string? bearerToken = null, CancellationToken cancellationToken = default);
}
