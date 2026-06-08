using MarbleWebProject.Models;

namespace MarbleWebProject.Services.Api;

public interface IStoreShippingApi
{
    Task<ShippingCalculateResponse?> CalculateAsync(
        ShippingCalculateRequest request,
        CancellationToken cancellationToken = default);

    Task<ShippingOptionsResponse?> CalculateOptionsAsync(
        ShippingCalculateRequest request,
        CancellationToken cancellationToken = default);
}
