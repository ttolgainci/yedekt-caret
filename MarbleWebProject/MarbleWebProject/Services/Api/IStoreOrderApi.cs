using MarbleWebProject.Helpers;
using MarbleWebProject.Models;

namespace MarbleWebProject.Services.Api;

public interface IStoreOrderApi
{
    Task<BaseResponse<List<ShopOrderListItemModel>>> GetMyOrdersAsync(CancellationToken cancellationToken = default);
    Task<BaseResponse<ShopOrderDetailModel>> GetOrderDetailAsync(int orderId, CancellationToken cancellationToken = default);
    Task<BaseResponse<ShipmentTrackingResultModel>> TrackShipmentAsync(int orderId, bool refresh = false, CancellationToken cancellationToken = default);
    Task<BaseResponse<ShopOrderDetailModel>> GetGuestOrderDetailAsync(int orderId, string guestUserId, string checkoutKey, CancellationToken cancellationToken = default);
    Task<BaseResponse<List<OrderBasket>>> MergeCartAsync(MergeCartForm form, CancellationToken cancellationToken = default);
    Task<BaseResponse<List<WishlistApiItem>>> MergeWishlistAsync(MergeCartForm form, CancellationToken cancellationToken = default);
}
