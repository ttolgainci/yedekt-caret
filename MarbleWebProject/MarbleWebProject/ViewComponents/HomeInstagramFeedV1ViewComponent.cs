using MarbleWebProject.Models;
using MarbleWebProject.Services.Api;
using MarbleWebProject.Services.Cache;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace MarbleWebProject.ViewComponents;

[ViewComponent]
public class HomeInstagramFeedV1ViewComponent : ViewComponent
{
    private static readonly TimeSpan MemoryTtl = TimeSpan.FromMinutes(5);
    private const int DefaultLimit = 12;

    private readonly IStoreInstagramApi _instagram;
    private readonly IWebInstagramCache _webCache;
    private readonly IGlobalInstagramCacheVersion _version;
    private readonly IMemoryCache _memory;

    public HomeInstagramFeedV1ViewComponent(
        IStoreInstagramApi instagram,
        IWebInstagramCache webCache,
        IGlobalInstagramCacheVersion version,
        IMemoryCache memory)
    {
        _instagram = instagram;
        _webCache = webCache;
        _version = version;
        _memory = memory;
    }

    public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken = default)
    {
        var tenant = AppConfig.ProjectName;
        var scopeKey = "all";
        var cacheVersion = _version.GetCurrent(tenant);
        var memoryKey = $"InstagramFeed:{tenant}:{scopeKey}:{DefaultLimit}:{cacheVersion}";

        if (_memory.TryGetValue(memoryKey, out StorefrontInstagramFeedModel? cached) && cached != null)
            return View(cached);

        var feed = await _webCache.TryGetFeedAsync(tenant, scopeKey, DefaultLimit, cacheVersion, cancellationToken);
        if (feed == null)
        {
            try
            {
                var response = await _instagram.GetFeedAsync(null, DefaultLimit, cancellationToken);
                feed = response.Status && response.Data != null
                    ? response.Data
                    : new StorefrontInstagramFeedModel();
                await _webCache.SetFeedAsync(tenant, scopeKey, DefaultLimit, cacheVersion, feed, cancellationToken);
            }
            catch
            {
                feed = new StorefrontInstagramFeedModel();
            }
        }

        _memory.Set(memoryKey, feed, MemoryTtl);
        return View(feed);
    }
}
