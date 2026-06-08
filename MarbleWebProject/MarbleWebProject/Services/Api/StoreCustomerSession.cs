using System.Text.Json;
using MarbleWebProject.Models;

namespace MarbleWebProject.Services.Api;

public sealed class StoreCustomerSession : IStoreCustomerSession
{
    private const string SessionKey = "StoreCustomerAuth";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public StoreCustomerSession(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<CustomerAuthResult?> GetAsync(CancellationToken cancellationToken = default)
    {
        var json = _httpContextAccessor.HttpContext?.Session.GetString(SessionKey);
        if (string.IsNullOrWhiteSpace(json))
            return Task.FromResult<CustomerAuthResult?>(null);

        return Task.FromResult(JsonSerializer.Deserialize<CustomerAuthResult>(json));
    }

    public Task SaveAsync(CustomerAuthResult auth, CancellationToken cancellationToken = default)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session != null)
            session.SetString(SessionKey, JsonSerializer.Serialize(auth));

        return Task.CompletedTask;
    }

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        _httpContextAccessor.HttpContext?.Session.Remove(SessionKey);
        return Task.CompletedTask;
    }

    public async Task<string?> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        var auth = await GetAsync(cancellationToken);
        return string.IsNullOrWhiteSpace(auth?.Token) ? null : auth.Token;
    }

    public bool IsLoggedIn()
    {
        var json = _httpContextAccessor.HttpContext?.Session.GetString(SessionKey);
        return !string.IsNullOrWhiteSpace(json);
    }
}
