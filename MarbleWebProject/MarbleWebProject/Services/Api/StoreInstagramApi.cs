using MarbleWebProject.Models;
using MarbleWebProject.Services;

namespace MarbleWebProject.Services.Api;

public sealed class StoreInstagramApi : IStoreInstagramApi
{
    private readonly IStoreApiClient _api;

    public StoreInstagramApi(IStoreApiClient api) => _api = api;

    public Task<BaseResponse<StorefrontInstagramFeedModel>> GetFeedAsync(int? accountId = null, int limit = 12, CancellationToken cancellationToken = default)
    {
        var path = accountId.HasValue && accountId.Value > 0
            ? $"/api/storefront/instagram/feed?accountId={accountId.Value}&limit={limit}"
            : $"/api/storefront/instagram/feed?limit={limit}";
        return _api.GetAsync<BaseResponse<StorefrontInstagramFeedModel>>(path, null, cancellationToken);
    }
}
