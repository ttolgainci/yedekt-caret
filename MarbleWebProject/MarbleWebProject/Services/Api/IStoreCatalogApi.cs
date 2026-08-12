using MarbleWebProject.Models;
using MarbleWebProject.Services;

namespace MarbleWebProject.Services.Api;

public interface IStoreCatalogApi
{
    Task<BaseResponse<List<CategoryListModel>>> GetCategoriesAsync(CategoryRequest request, CancellationToken cancellationToken = default);
    Task<BaseResponse<ProductsByCageoryResponse>> GetProductsByCategoryAsync(ProductsByCageoryRequest request, CancellationToken cancellationToken = default);
    Task<BaseResponse<List<CategoryRouteModel>>> GetCategoryRoutesAsync(CancellationToken cancellationToken = default);
    Task<BaseResponse<List<CategoryRouteModel>>> GetProductRoutesAsync(CancellationToken cancellationToken = default);
    Task<BaseResponse<ProductDetailResponse>> GetProductDetailAsync(ProductDetailRequest request, CancellationToken cancellationToken = default);
    Task<BaseResponse<string>> GetProductCanonicalUrlAsync(int productId, string languageCode, CancellationToken cancellationToken = default);
    Task<BaseResponse<List<ProductAlternateUrlModel>>> GetProductAlternateUrlsAsync(int productId, CancellationToken cancellationToken = default);
    Task<BaseResponse<ProductCartInfo>> GetProductForCartAsync(ProductCartRequest request, CancellationToken cancellationToken = default);
    Task<BaseResponse<List<ProductBreadcrumbsResponse>>> GetProductBreadcrumbAsync(ProductBreadcrumbRequest request, CancellationToken cancellationToken = default);
    Task<BaseResponse<List<CategoryBreadcrumbModel>>> GetCategoryBreadcrumbAsync(CategoryBreadcrumbRequest request, CancellationToken cancellationToken = default);
    Task<BaseResponse<List<CategoryBreadcrumbModel>>> GetProductBreadcrumbPathAsync(ProductBreadcrumbRequest request, CancellationToken cancellationToken = default);
    Task<BaseResponse<List<ProductList>>> GetProductDetailSimilarAsync(ProductsByCageoryRequest request, CancellationToken cancellationToken = default);
    Task<BaseResponse<List<HomeBrandModel>>> GetStorefrontBrandsAsync(string languageCode, CancellationToken cancellationToken = default);

    Task<BaseResponse<List<VehicleMakeListItem>>> GetVehicleMakesAsync(CancellationToken cancellationToken = default);
    Task<BaseResponse<List<VehicleModelListItem>>> GetVehicleModelsByMakeAsync(int makeId, CancellationToken cancellationToken = default);
    Task<BaseResponse<List<VehicleGenerationListItem>>> GetVehicleGenerationsByModelAsync(int modelId, CancellationToken cancellationToken = default);
    Task<BaseResponse<List<VehicleEngineListItem>>> GetVehicleEnginesByGenerationAsync(int generationId, CancellationToken cancellationToken = default);
    Task<BaseResponse<List<VehicleSearchCategoryListItem>>> GetCategoriesByVehicleEngineAsync(
        int engineId,
        string languageCode,
        IReadOnlyList<int>? brandIds = null,
        CancellationToken cancellationToken = default);
    Task<BaseResponse<List<VehicleSearchBrandListItem>>> GetBrandsByVehicleEngineAsync(
        int engineId,
        string languageCode,
        IReadOnlyList<int>? categoryIds = null,
        CancellationToken cancellationToken = default);
    Task<BaseResponse<VehicleSearchProductsResponse>> GetProductsByVehicleEngineAsync(
        int engineId,
        string languageCode,
        IReadOnlyList<int>? categoryIds = null,
        IReadOnlyList<int>? brandIds = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int pageNumber = 1,
        int pageSize = 12,
        CancellationToken cancellationToken = default);
    Task<BaseResponse<VehicleSearchPriceRangeModel>> GetPriceRangeByVehicleEngineAsync(
        int engineId,
        string languageCode,
        IReadOnlyList<int>? categoryIds = null,
        IReadOnlyList<int>? brandIds = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        CancellationToken cancellationToken = default);
    Task<BaseResponse<List<VehicleSearchCategoryListItem>>> GetSubcategoriesByCategoryAsync(
        int categoryId,
        string languageCode,
        int? brandId = null,
        CancellationToken cancellationToken = default);
    Task<BaseResponse<List<VehicleSearchBrandListItem>>> GetBrandsByCategoryAsync(
        int categoryId,
        string languageCode,
        int? brandId = null,
        CancellationToken cancellationToken = default);
    Task<BaseResponse<VehicleSearchProductsResponse>> GetProductsByCategorySearchAsync(
        int categoryId,
        string languageCode,
        IReadOnlyList<int>? brandIds = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int pageNumber = 1,
        int pageSize = 12,
        CancellationToken cancellationToken = default);
    Task<BaseResponse<VehicleSearchPriceRangeModel>> GetPriceRangeByCategoryAsync(
        int categoryId,
        string languageCode,
        int? brandId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        CancellationToken cancellationToken = default);
    Task<BaseResponse<HomeBrandModel>> GetBrandAsync(
        int brandId,
        string languageCode,
        CancellationToken cancellationToken = default);
    Task<BaseResponse<List<VehicleSearchBrandListItem>>> GetFilterBrandsAsync(
        string languageCode,
        int? categoryId = null,
        CancellationToken cancellationToken = default);
    Task<BaseResponse<List<VehicleSearchCategoryListItem>>> GetCategoriesByBrandAsync(
        int brandId,
        string languageCode,
        int? categoryId = null,
        CancellationToken cancellationToken = default);
    Task<BaseResponse<VehicleSearchProductsResponse>> GetProductsByBrandAsync(
        int brandId,
        string languageCode,
        IReadOnlyList<int>? categoryIds = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int pageNumber = 1,
        int pageSize = 12,
        CancellationToken cancellationToken = default);
    Task<BaseResponse<VehicleSearchPriceRangeModel>> GetPriceRangeByBrandAsync(
        int brandId,
        string languageCode,
        int? categoryId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        CancellationToken cancellationToken = default);

    Task<BaseResponse<CatalogSuggestResponse>> SuggestAsync(
        string query,
        string languageCode,
        int limit = 8,
        CancellationToken cancellationToken = default);
    Task<BaseResponse<List<VehicleSearchCategoryListItem>>> GetCategoriesByTextSearchAsync(
        string query,
        string languageCode,
        IReadOnlyList<int>? brandIds = null,
        CancellationToken cancellationToken = default);
    Task<BaseResponse<List<VehicleSearchBrandListItem>>> GetBrandsByTextSearchAsync(
        string query,
        string languageCode,
        IReadOnlyList<int>? categoryIds = null,
        CancellationToken cancellationToken = default);
    Task<BaseResponse<VehicleSearchProductsResponse>> GetProductsByTextSearchAsync(
        string query,
        string languageCode,
        IReadOnlyList<int>? categoryIds = null,
        IReadOnlyList<int>? brandIds = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int pageNumber = 1,
        int pageSize = 12,
        CancellationToken cancellationToken = default);
    Task<BaseResponse<VehicleSearchPriceRangeModel>> GetPriceRangeByTextSearchAsync(
        string query,
        string languageCode,
        IReadOnlyList<int>? categoryIds = null,
        IReadOnlyList<int>? brandIds = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        CancellationToken cancellationToken = default);
}
