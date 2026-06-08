using MarbleWebProject.Models;
using MarbleWebProject.Services;
using Microsoft.Extensions.Configuration;

namespace MarbleWebProject.Services.Api;

public sealed class StoreStorefrontApi : IStoreStorefrontApi
{
    private readonly IStoreApiClient _api;
    private readonly IStoreAuthService _auth;
    private readonly IConfiguration _configuration;

    public StoreStorefrontApi(IStoreApiClient api, IStoreAuthService auth, IConfiguration configuration)
    {
        _api = api;
        _auth = auth;
        _configuration = configuration;
    }

    public Task<BaseResponse<StorefrontPageLayoutModel>> GetHomeLayoutAsync(CancellationToken cancellationToken = default)
    {
        var tenant = AppConfig.ProjectName;
        var sector = AppConfig.ProjectService.SectorCode;
        var path = $"/api/storefront/layout/home?customName={Uri.EscapeDataString(tenant)}&sectorCode={Uri.EscapeDataString(sector)}";
        return _api.GetAsync<BaseResponse<StorefrontPageLayoutModel>>(path, null, cancellationToken);
    }

    public Task<BaseResponse<StorefrontPublishedLayoutModel>> GetPublishedLayoutAsync(CancellationToken cancellationToken = default)
    {
        var tenant = AppConfig.ProjectName;
        var sector = AppConfig.ProjectService.SectorCode;
        var path = $"/api/storefront/layout/published?customName={Uri.EscapeDataString(tenant)}&sectorCode={Uri.EscapeDataString(sector)}";
        return _api.GetAsync<BaseResponse<StorefrontPublishedLayoutModel>>(path, null, cancellationToken);
    }

    public async Task<BaseResponse<List<CampaignActiveModel>>> GetActiveCampaignsAsync(CancellationToken cancellationToken = default)
    {
        var session = await _auth.GetSessionAsync(cancellationToken);
        var path = $"/api/catalog/campaigns/active?languageCode={Uri.EscapeDataString(session.LanguageCode)}";
        return await _api.GetAsync<BaseResponse<List<CampaignActiveModel>>>(path, null, cancellationToken);
    }

    public async Task<BaseResponse<List<ProductList>>> GetHomeListedProductsAsync(int limit = 12, CancellationToken cancellationToken = default)
    {
        var session = await _auth.GetSessionAsync(cancellationToken);
        var path = $"/api/catalog/home/listed-products?languageCode={Uri.EscapeDataString(session.LanguageCode)}&limit={limit}";
        return await _api.GetAsync<BaseResponse<List<ProductList>>>(path, null, cancellationToken);
    }

    public async Task<BaseResponse<List<ProductList>>> GetTopSellingProductsAsync(int limit = 12, CancellationToken cancellationToken = default)
    {
        var session = await _auth.GetSessionAsync(cancellationToken);
        var path = $"/api/catalog/home/top-selling-products?languageCode={Uri.EscapeDataString(session.LanguageCode)}&limit={limit}";
        return await _api.GetAsync<BaseResponse<List<ProductList>>>(path, null, cancellationToken);
    }

    public Task<BaseResponse<PlaceOrderApiResponse>> PlaceGuestOrderAsync(
        PlaceGuestOrderApiRequest request,
        string checkoutKey,
        CancellationToken cancellationToken = default)
    {
        var headers = new Dictionary<string, string> { ["X-Checkout-Key"] = checkoutKey };
        return _api.PostAsync<BaseResponse<PlaceOrderApiResponse>>(
            "/api/orders/place-guest", request, bearerToken: null, extraHeaders: headers, cancellationToken: cancellationToken);
    }
}
