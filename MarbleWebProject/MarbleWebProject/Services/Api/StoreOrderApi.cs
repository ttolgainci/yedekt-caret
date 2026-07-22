using MarbleWebProject.Models;

namespace MarbleWebProject.Services.Api;

public sealed class StoreOrderApi : IStoreOrderApi
{
    private readonly IStoreApiClient _api;
    private readonly IStoreCustomerSession _session;

    public StoreOrderApi(IStoreApiClient api, IStoreCustomerSession session)
    {
        _api = api;
        _session = session;
    }

    public async Task<BaseResponse<List<ShopOrderListItemModel>>> GetMyOrdersAsync(CancellationToken cancellationToken = default)
    {
        var token = await _session.GetTokenAsync(cancellationToken);
        return await _api.GetAsync<BaseResponse<List<ShopOrderListItemModel>>>("/api/orders/my", token, cancellationToken);
    }

    public async Task<BaseResponse<ShopOrderDetailModel>> GetOrderDetailAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var token = await _session.GetTokenAsync(cancellationToken);
        return await _api.GetAsync<BaseResponse<ShopOrderDetailModel>>($"/api/orders/{orderId}", token, cancellationToken);
    }

    public async Task<BaseResponse<ShipmentTrackingResultModel>> TrackShipmentAsync(
        int orderId,
        bool refresh = false,
        CancellationToken cancellationToken = default)
    {
        var token = await _session.GetTokenAsync(cancellationToken);
        var refreshQuery = refresh ? "?refresh=true" : "";
        return await _api.GetAsync<BaseResponse<ShipmentTrackingResultModel>>(
            $"/api/orders/{orderId}/shipment/track{refreshQuery}",
            token,
            cancellationToken);
    }

    public Task<BaseResponse<ShopOrderDetailModel>> GetGuestOrderDetailAsync(
        int orderId,
        string guestUserId,
        string checkoutKey,
        CancellationToken cancellationToken = default)
    {
        var path = $"/api/orders/guest/{orderId}?guestUserId={Uri.EscapeDataString(guestUserId)}";
        var headers = new Dictionary<string, string> { ["X-Checkout-Key"] = checkoutKey };
        return _api.GetAsync<BaseResponse<ShopOrderDetailModel>>(path, bearerToken: null, cancellationToken, headers);
    }

    public async Task<BaseResponse<List<OrderBasket>>> MergeCartAsync(MergeCartForm form, CancellationToken cancellationToken = default)
    {
        var token = await _session.GetTokenAsync(cancellationToken);
        return await _api.PostAsync<BaseResponse<List<OrderBasket>>>(
            "/api/customers/merge-cart",
            new
            {
                guestUserId = form.GuestUserId,
                languageCode = form.LanguageCode ?? "tr"
            },
            token,
            cancellationToken);
    }
}
