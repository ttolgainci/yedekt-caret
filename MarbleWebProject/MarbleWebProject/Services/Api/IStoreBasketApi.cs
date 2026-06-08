using MarbleWebProject.Models;
using MarbleWebProject.Services;

namespace MarbleWebProject.Services.Api;

public interface IStoreBasketApi
{
    Task<BaseResponse<OrderBasket>> CreateOrUpdateBasketAsync(OrderBasket request, CancellationToken cancellationToken = default);
    Task<BaseResponse<List<OrderBasket>>> GetByIdBasketAsync(BasketRequest request, CancellationToken cancellationToken = default);
    Task<BaseResponse<List<OrderBasket>>> GetBasketAllAsync(BasketAllRequest request, CancellationToken cancellationToken = default);
    Task<BaseResponse<List<OrderBasket>>> DeleteProductFromCartAsync(DeleteBasketRequest request, CancellationToken cancellationToken = default);
    Task<BaseResponse<List<OrderBasket>>> ClearCartAsync(BasketAllRequest request, CancellationToken cancellationToken = default);
}
