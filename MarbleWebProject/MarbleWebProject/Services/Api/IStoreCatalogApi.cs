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
        int? brandId = null,
        CancellationToken cancellationToken = default);
    Task<BaseResponse<List<VehicleSearchBrandListItem>>> GetBrandsByVehicleEngineAsync(
        int engineId,
        string languageCode,
        int? categoryId = null,
        CancellationToken cancellationToken = default);
    Task<BaseResponse<VehicleSearchProductsResponse>> GetProductsByVehicleEngineAsync(
        int engineId,
        string languageCode,
        int? categoryId = null,
        int? brandId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int pageNumber = 1,
        int pageSize = 12,
        CancellationToken cancellationToken = default);
    Task<BaseResponse<VehicleSearchPriceRangeModel>> GetPriceRangeByVehicleEngineAsync(
        int engineId,
        string languageCode,
        int? categoryId = null,
        int? brandId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        CancellationToken cancellationToken = default);
}
