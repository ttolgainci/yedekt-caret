using MarbleWebProject.Helpers;
using MarbleWebProject.Models;
using MarbleWebProject.Services;

namespace MarbleWebProject.Services.Api;

public sealed class StoreWishlistApi : IStoreWishlistApi
{
    private readonly IStoreApiClient _api;
    private readonly IStoreAuthService _auth;

    public StoreWishlistApi(IStoreApiClient api, IStoreAuthService auth)
    {
        _api = api;
        _auth = auth;
    }

    public async Task<BaseResponse<List<WishlistApiItem>>> GetAllAsync(
        WishlistAllRequest request,
        CancellationToken cancellationToken = default)
    {
        var token = await _auth.GetTokenAsync(cancellationToken);
        return await _api.PostAsync<BaseResponse<List<WishlistApiItem>>>("/Wishlist/GetAll", request, token, cancellationToken);
    }

    public async Task<BaseResponse<WishlistToggleResultModel>> ToggleAsync(
        WishlistToggleRequest request,
        CancellationToken cancellationToken = default)
    {
        var token = await _auth.GetTokenAsync(cancellationToken);
        return await _api.PostAsync<BaseResponse<WishlistToggleResultModel>>("/Wishlist/Toggle", request, token, cancellationToken);
    }

    public async Task<BaseResponse<List<WishlistApiItem>>> RemoveAsync(
        WishlistRemoveRequest request,
        CancellationToken cancellationToken = default)
    {
        var token = await _auth.GetTokenAsync(cancellationToken);
        return await _api.PostAsync<BaseResponse<List<WishlistApiItem>>>("/Wishlist/Remove", request, token, cancellationToken);
    }
}
