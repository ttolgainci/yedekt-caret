using MarbleWebProject.Models;

namespace MarbleWebProject.Services.Cache;

public interface IWebInstagramCache
{
    bool IsEnabled();
    Task<StorefrontInstagramFeedModel?> TryGetFeedAsync(string tenant, string scopeKey, int limit, int version, CancellationToken cancellationToken = default);
    Task SetFeedAsync(string tenant, string scopeKey, int limit, int version, StorefrontInstagramFeedModel feed, CancellationToken cancellationToken = default);
}
