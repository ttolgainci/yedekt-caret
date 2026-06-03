using MarbleWebProject.Models;
using MarbleWebProject.Services;

namespace MarbleWebProject.Services.Api;

public sealed class StoreBasketApi : IStoreBasketApi
{
    private readonly IStoreApiClient _api;
    private readonly IStoreAuthService _auth;

    public StoreBasketApi(IStoreApiClient api, IStoreAuthService auth)
    {
        _api = api;
        _auth = auth;
    }

    public async Task<BaseResponse<OrderBasket>> CreateOrUpdateBasketAsync(OrderBasket request, CancellationToken cancellationToken = default)
    {
        var token = await _auth.GetTokenAsync(cancellationToken);
        return await _api.PostAsync<BaseResponse<OrderBasket>>("/Basket/CreateOrUpdateBasket", request, token, cancellationToken);
    }

    public async Task<BaseResponse<List<OrderBasket>>> GetByIdBasketAsync(BasketRequest request, CancellationToken cancellationToken = default)
    {
        var token = await _auth.GetTokenAsync(cancellationToken);
        return await _api.PostAsync<BaseResponse<List<OrderBasket>>>("/Basket/GetByIDBaskets", request, token, cancellationToken);
    }

    public async Task<BaseResponse<List<OrderBasket>>> GetBasketAllAsync(BasketAllRequest request, CancellationToken cancellationToken = default)
    {
        var token = await _auth.GetTokenAsync(cancellationToken);
        return await _api.PostAsync<BaseResponse<List<OrderBasket>>>("/Basket/GetBasketAll", request, token, cancellationToken);
    }

    public async Task<BaseResponse<List<OrderBasket>>> DeleteProductFromCartAsync(DeleteBasketRequest request, CancellationToken cancellationToken = default)
    {
        var token = await _auth.GetTokenAsync(cancellationToken);
        return await _api.PostAsync<BaseResponse<List<OrderBasket>>>("/Basket/DeleteProductFromCart", request, token, cancellationToken);
    }
}
