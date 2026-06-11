using MarbleWebProject.Models;
using MarbleWebProject.Services;

namespace MarbleWebProject.Services.Api;

public interface IStoreStorefrontApi
{
    Task<BaseResponse<StorefrontPageLayoutModel>> GetHomeLayoutAsync(CancellationToken cancellationToken = default);
    Task<BaseResponse<StorefrontPublishedLayoutModel>> GetPublishedLayoutAsync(CancellationToken cancellationToken = default);
    Task<BaseResponse<StoreGeneralSettingsModel>> GetGeneralSettingsAsync(CancellationToken cancellationToken = default);
    Task<BaseResponse<List<CampaignActiveModel>>> GetActiveCampaignsAsync(CancellationToken cancellationToken = default);
    Task<BaseResponse<List<ProductList>>> GetHomeListedProductsAsync(int limit = 12, CancellationToken cancellationToken = default);
    Task<BaseResponse<List<ProductList>>> GetTopSellingProductsAsync(int limit = 12, CancellationToken cancellationToken = default);
    Task<BaseResponse<List<ProductList>>> GetFeaturedProductsAsync(int limit = 12, CancellationToken cancellationToken = default);
    Task<BaseResponse<List<ProductList>>> GetDealOfWeekProductsAsync(int limit = 8, CancellationToken cancellationToken = default);
    Task<BaseResponse<List<ProductList>>> GetDiscountedProductsAsync(int limit = 12, CancellationToken cancellationToken = default);
    Task<BaseResponse<List<ProductList>>> GetNewArrivalProductsAsync(int limit = 12, CancellationToken cancellationToken = default);
    Task<BaseResponse<PlaceOrderApiResponse>> PlaceGuestOrderAsync(PlaceGuestOrderApiRequest request, string checkoutKey, CancellationToken cancellationToken = default);
}
