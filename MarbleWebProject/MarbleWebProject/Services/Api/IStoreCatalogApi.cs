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
}
