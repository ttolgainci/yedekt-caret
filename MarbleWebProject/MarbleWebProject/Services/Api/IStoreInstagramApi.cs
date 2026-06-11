using MarbleWebProject.Models;
using MarbleWebProject.Services;

namespace MarbleWebProject.Services.Api;

public interface IStoreInstagramApi
{
    Task<BaseResponse<StorefrontInstagramFeedModel>> GetFeedAsync(int? accountId = null, int limit = 12, CancellationToken cancellationToken = default);
}
