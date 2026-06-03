using MarbleWebProject.Helper;
using MarbleWebProject.Models;
using MarbleWebProject.Services.Api;
using Microsoft.Extensions.Caching.Memory;

namespace MarbleWebProject.Services.Storefront;

public sealed class StorefrontRuntimeProvider : IStorefrontRuntimeProvider
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(20);
    private readonly IMemoryCache _cache;
    private readonly IStoreStorefrontApi _storefront;
    private readonly ILogger<StorefrontRuntimeProvider> _logger;

    public StorefrontRuntimeProvider(
        IMemoryCache cache,
        IStoreStorefrontApi storefront,
        ILogger<StorefrontRuntimeProvider> logger)
    {
        _cache = cache;
        _storefront = storefront;
        _logger = logger;
    }

    public async Task<StorefrontRuntimeConfig> GetAsync(CancellationToken cancellationToken = default)
    {
        var tenant = AppConfig.ProjectName;
        var sector = AppConfig.ProjectService.SectorCode;
        var cacheKey = $"storefront:published:{tenant}:{sector}";

        if (_cache.TryGetValue(cacheKey, out StorefrontRuntimeConfig? cached) && cached != null)
            return SyncAppConfig(cached);

        var runtime = await StorefrontBootstrap.LoadAsync(_storefront, _logger, cancellationToken);
        _cache.Set(cacheKey, runtime, CacheTtl);
        return SyncAppConfig(runtime);
    }

    private static StorefrontRuntimeConfig SyncAppConfig(StorefrontRuntimeConfig runtime)
    {
        AppConfig.Storefront = runtime;
        return runtime;
    }
}
