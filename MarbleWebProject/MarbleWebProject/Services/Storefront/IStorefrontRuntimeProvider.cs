using MarbleWebProject.Models;

namespace MarbleWebProject.Services.Storefront;

public interface IStorefrontRuntimeProvider
{
    Task<StorefrontRuntimeConfig> GetAsync(CancellationToken cancellationToken = default);
}
