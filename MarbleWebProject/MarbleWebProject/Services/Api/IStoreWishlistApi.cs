using MarbleWebProject.Helpers;
using MarbleWebProject.Models;
using MarbleWebProject.Services;

namespace MarbleWebProject.Services.Api;

public interface IStoreWishlistApi
{
    Task<BaseResponse<List<WishlistApiItem>>> GetAllAsync(WishlistAllRequest request, CancellationToken cancellationToken = default);
    Task<BaseResponse<WishlistToggleResultModel>> ToggleAsync(WishlistToggleRequest request, CancellationToken cancellationToken = default);
    Task<BaseResponse<List<WishlistApiItem>>> RemoveAsync(WishlistRemoveRequest request, CancellationToken cancellationToken = default);
}
