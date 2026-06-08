using MarbleWebProject.Models;

namespace MarbleWebProject.Services.Api;

public sealed class StoreShippingApi : IStoreShippingApi
{
    private readonly IStoreApiClient _api;

    public StoreShippingApi(IStoreApiClient api)
    {
        _api = api;
    }

    public async Task<ShippingCalculateResponse?> CalculateAsync(
        ShippingCalculateRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _api.PostAsync<ShippingCalculateResponse>(
                "/api/shipping/calculate",
                request,
                bearerToken: null,
                cancellationToken);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<ShippingOptionsResponse?> CalculateOptionsAsync(
        ShippingCalculateRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _api.PostAsync<ShippingOptionsResponse>(
                "/api/shipping/options",
                request,
                bearerToken: null,
                cancellationToken);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }
}
