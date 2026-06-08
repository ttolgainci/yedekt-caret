using MarbleWebProject.Models;

namespace MarbleWebProject.Services.Api;

public interface IStoreCustomerSession
{
    Task<CustomerAuthResult?> GetAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(CustomerAuthResult auth, CancellationToken cancellationToken = default);
    Task ClearAsync(CancellationToken cancellationToken = default);
    Task<string?> GetTokenAsync(CancellationToken cancellationToken = default);
    bool IsLoggedIn();
}
