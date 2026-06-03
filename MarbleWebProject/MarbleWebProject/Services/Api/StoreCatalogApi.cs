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
}
