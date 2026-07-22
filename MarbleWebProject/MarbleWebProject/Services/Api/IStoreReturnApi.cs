using MarbleWebProject.Models;

namespace MarbleWebProject.Services.Api;

public interface IStoreReturnApi
{
    Task<BaseResponse<List<ReturnRequestListItemModel>>> GetMyReturnsAsync(CancellationToken cancellationToken = default);
    Task<BaseResponse<ReturnRequestDetailModel>> GetReturnDetailAsync(int returnRequestId, CancellationToken cancellationToken = default);
    Task<BaseResponse<ReturnRequestDetailModel>> CreateReturnAsync(int shopOrderId, StoreReturnCreateForm body, CancellationToken cancellationToken = default);
}
