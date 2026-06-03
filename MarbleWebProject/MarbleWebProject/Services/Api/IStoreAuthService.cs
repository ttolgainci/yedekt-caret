using MarbleWebProject.Models;

namespace MarbleWebProject.Services.Api;

public interface IStoreAuthService
{
    Task<TokenResponse> GetSessionAsync(CancellationToken cancellationToken = default);
    Task<string> GetTokenAsync(CancellationToken cancellationToken = default);
}
