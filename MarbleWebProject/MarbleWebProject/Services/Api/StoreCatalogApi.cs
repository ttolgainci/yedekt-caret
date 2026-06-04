using MarbleWebProject.Models;
using MarbleWebProject.Services;

namespace MarbleWebProject.Services.Api;

public sealed class StoreCatalogApi : IStoreCatalogApi
{
    private readonly IStoreApiClient _api;
    private readonly IStoreAuthService _auth;

    public StoreCatalogApi(IStoreApiClient api, IStoreAuthService auth)
    {
        _api = api;
        _auth = auth;
    }

    public async Task<BaseResponse<List<CategoryListModel>>> GetCategoriesAsync(CategoryRequest request, CancellationToken cancellationToken = default)
    {
        var token = await _auth.GetTokenAsync(cancellationToken);
        return await _api.PostAsync<BaseResponse<List<CategoryListModel>>>("/Category/GetCagetory", request, token, cancellationToken);
    }

    public async Task<BaseResponse<ProductsByCageoryResponse>> GetProductsByCategoryAsync(ProductsByCageoryRequest request, CancellationToken cancellationToken = default)
    {
        var token = await _auth.GetTokenAsync(cancellationToken);
        return await _api.PostAsync<BaseResponse<ProductsByCageoryResponse>>("/Product/GetProductByCategory", request, token, cancellationToken);
    }

    public async Task<BaseResponse<List<CategoryRouteModel>>> GetCategoryRoutesAsync(CancellationToken cancellationToken = default)
    {
        var token = await _auth.GetTokenAsync(cancellationToken);
        return await _api.PostAsync<BaseResponse<List<CategoryRouteModel>>>("/Category/GetCategoryRoute", new { }, token, cancellationToken);
    }

    public async Task<BaseResponse<List<CategoryRouteModel>>> GetProductRoutesAsync(CancellationToken cancellationToken = default)
    {
        var token = await _auth.GetTokenAsync(cancellationToken);
        return await _api.PostAsync<BaseResponse<List<CategoryRouteModel>>>("/Product/GetProductRoute", new { }, token, cancellationToken);
    }

    public async Task<BaseResponse<ProductDetailResponse>> GetProductDetailAsync(ProductDetailRequest request, CancellationToken cancellationToken = default)
    {
        var token = await _auth.GetTokenAsync(cancellationToken);
        return await _api.PostAsync<BaseResponse<ProductDetailResponse>>("/Product/GetProductDetail", request, token, cancellationToken);
    }

    public async Task<BaseResponse<ProductCartInfo>> GetProductForCartAsync(ProductCartRequest request, CancellationToken cancellationToken = default)
    {
        var token = await _auth.GetTokenAsync(cancellationToken);
        return await _api.PostAsync<BaseResponse<ProductCartInfo>>("/Product/GetProductForCart", request, token, cancellationToken);
    }

    public async Task<BaseResponse<List<ProductBreadcrumbsResponse>>> GetProductBreadcrumbAsync(ProductBreadcrumbRequest request, CancellationToken cancellationToken = default)
    {
        var token = await _auth.GetTokenAsync(cancellationToken);
        return await _api.PostAsync<BaseResponse<List<ProductBreadcrumbsResponse>>>("/Product/GetProductBreadcrumb", request, token, cancellationToken);
    }

    public async Task<BaseResponse<List<CategoryBreadcrumbModel>>> GetCategoryBreadcrumbAsync(CategoryBreadcrumbRequest request, CancellationToken cancellationToken = default)
    {
        var token = await _auth.GetTokenAsync(cancellationToken);
        return await _api.PostAsync<BaseResponse<List<CategoryBreadcrumbModel>>>("/Category/GetCategoryBreadcrumbAsync", request, token, cancellationToken);
    }

    public async Task<BaseResponse<List<CategoryBreadcrumbModel>>> GetProductBreadcrumbPathAsync(ProductBreadcrumbRequest request, CancellationToken cancellationToken = default)
    {
        var token = await _auth.GetTokenAsync(cancellationToken);
        return await _api.PostAsync<BaseResponse<List<CategoryBreadcrumbModel>>>("/Product/GetProductBreadcrumbAsync", request, token, cancellationToken);
    }

    public async Task<BaseResponse<List<ProductList>>> GetProductDetailSimilarAsync(ProductsByCageoryRequest request, CancellationToken cancellationToken = default)
    {
        var token = await _auth.GetTokenAsync(cancellationToken);
        return await _api.PostAsync<BaseResponse<List<ProductList>>>("/Product/GetProductDetailSimilar", request, token, cancellationToken);
    }

    public Task<BaseResponse<List<HomeBrandModel>>> GetStorefrontBrandsAsync(string languageCode, CancellationToken cancellationToken = default)
    {
        var lang = string.IsNullOrWhiteSpace(languageCode) ? "tr" : languageCode.Trim();
        var path = $"/api/catalog/brands?languageCode={Uri.EscapeDataString(lang)}";
        return _api.GetAsync<BaseResponse<List<HomeBrandModel>>>(path, bearerToken: null, cancellationToken);
    }

    public Task<BaseResponse<List<VehicleMakeListItem>>> GetVehicleMakesAsync(CancellationToken cancellationToken = default) =>
        _api.GetAsync<BaseResponse<List<VehicleMakeListItem>>>("/api/catalog/vehicle-makes", bearerToken: null, cancellationToken);

    public Task<BaseResponse<List<VehicleModelListItem>>> GetVehicleModelsByMakeAsync(int makeId, CancellationToken cancellationToken = default) =>
        _api.GetAsync<BaseResponse<List<VehicleModelListItem>>>($"/api/catalog/vehicle-makes/{makeId}/models", bearerToken: null, cancellationToken);

    public Task<BaseResponse<List<VehicleGenerationListItem>>> GetVehicleGenerationsByModelAsync(int modelId, CancellationToken cancellationToken = default) =>
        _api.GetAsync<BaseResponse<List<VehicleGenerationListItem>>>($"/api/catalog/vehicle-models/{modelId}/generations", bearerToken: null, cancellationToken);

    public Task<BaseResponse<List<VehicleEngineListItem>>> GetVehicleEnginesByGenerationAsync(int generationId, CancellationToken cancellationToken = default) =>
        _api.GetAsync<BaseResponse<List<VehicleEngineListItem>>>($"/api/catalog/vehicle-generations/{generationId}/engines", bearerToken: null, cancellationToken);

    public Task<BaseResponse<List<VehicleSearchCategoryListItem>>> GetCategoriesByVehicleEngineAsync(
        int engineId,
        string languageCode,
        int? brandId = null,
        CancellationToken cancellationToken = default)
    {
        var lang = string.IsNullOrWhiteSpace(languageCode) ? "tr" : languageCode.Trim();
        var path = $"/api/catalog/vehicle-engines/{engineId}/categories?languageCode={Uri.EscapeDataString(lang)}";
        if (brandId is > 0)
            path += $"&brandId={brandId.Value}";
        return _api.GetAsync<BaseResponse<List<VehicleSearchCategoryListItem>>>(path, bearerToken: null, cancellationToken);
    }

    public Task<BaseResponse<List<VehicleSearchBrandListItem>>> GetBrandsByVehicleEngineAsync(
        int engineId,
        string languageCode,
        int? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        var lang = string.IsNullOrWhiteSpace(languageCode) ? "tr" : languageCode.Trim();
        var path = $"/api/catalog/vehicle-engines/{engineId}/brands?languageCode={Uri.EscapeDataString(lang)}";
        if (categoryId is > 0)
            path += $"&categoryId={categoryId.Value}";
        return _api.GetAsync<BaseResponse<List<VehicleSearchBrandListItem>>>(path, bearerToken: null, cancellationToken);
    }

    public Task<BaseResponse<VehicleSearchProductsResponse>> GetProductsByVehicleEngineAsync(
        int engineId,
        string languageCode,
        int? categoryId = null,
        int? brandId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int pageNumber = 1,
        int pageSize = 12,
        CancellationToken cancellationToken = default)
    {
        var lang = string.IsNullOrWhiteSpace(languageCode) ? "tr" : languageCode.Trim();
        var path = $"/api/catalog/vehicle-engines/{engineId}/products?languageCode={Uri.EscapeDataString(lang)}&pageNumber={pageNumber}&pageSize={pageSize}";
        if (categoryId is > 0)
            path += $"&categoryId={categoryId.Value}";
        if (brandId is > 0)
            path += $"&brandId={brandId.Value}";
        if (minPrice.HasValue)
            path += $"&minPrice={minPrice.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
        if (maxPrice.HasValue)
            path += $"&maxPrice={maxPrice.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
        return _api.GetAsync<BaseResponse<VehicleSearchProductsResponse>>(path, bearerToken: null, cancellationToken);
    }

    public Task<BaseResponse<VehicleSearchPriceRangeModel>> GetPriceRangeByVehicleEngineAsync(
        int engineId,
        string languageCode,
        int? categoryId = null,
        int? brandId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        CancellationToken cancellationToken = default)
    {
        var lang = string.IsNullOrWhiteSpace(languageCode) ? "tr" : languageCode.Trim();
        var path = $"/api/catalog/vehicle-engines/{engineId}/price-range?languageCode={Uri.EscapeDataString(lang)}";
        if (categoryId is > 0)
            path += $"&categoryId={categoryId.Value}";
        if (brandId is > 0)
            path += $"&brandId={brandId.Value}";
        if (minPrice.HasValue)
            path += $"&minPrice={minPrice.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
        if (maxPrice.HasValue)
            path += $"&maxPrice={maxPrice.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
        return _api.GetAsync<BaseResponse<VehicleSearchPriceRangeModel>>(path, bearerToken: null, cancellationToken);
    }
}
