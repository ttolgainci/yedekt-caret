using MarbleWebProject.Models;

namespace MarbleWebProject.Services.Api;

public sealed class StoreReturnApi : IStoreReturnApi
{
    private readonly IStoreApiClient _api;
    private readonly IStoreCustomerSession _session;

    public StoreReturnApi(IStoreApiClient api, IStoreCustomerSession session)
    {
        _api = api;
        _session = session;
    }

    public async Task<BaseResponse<List<ReturnRequestListItemModel>>> GetMyReturnsAsync(CancellationToken cancellationToken = default)
    {
        var token = await _session.GetTokenAsync(cancellationToken);
        return await _api.GetAsync<BaseResponse<List<ReturnRequestListItemModel>>>(
            "/api/orders/returns", token, cancellationToken);
    }

    public async Task<BaseResponse<ReturnRequestDetailModel>> GetReturnDetailAsync(
        int returnRequestId,
        CancellationToken cancellationToken = default)
    {
        var token = await _session.GetTokenAsync(cancellationToken);
        return await _api.GetAsync<BaseResponse<ReturnRequestDetailModel>>(
            $"/api/orders/returns/{returnRequestId}", token, cancellationToken);
    }

    public async Task<BaseResponse<ReturnRequestDetailModel>> CreateReturnAsync(
        int shopOrderId,
        StoreReturnCreateForm body,
        CancellationToken cancellationToken = default)
    {
        var token = await _session.GetTokenAsync(cancellationToken);
        return await _api.PostAsync<BaseResponse<ReturnRequestDetailModel>>(
            $"/api/orders/{shopOrderId}/returns",
            body,
            token,
            cancellationToken);
    }
}
