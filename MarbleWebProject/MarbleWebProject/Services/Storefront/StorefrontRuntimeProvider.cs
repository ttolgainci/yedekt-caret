using MarbleWebProject.Helper;
using MarbleWebProject.Models;
using MarbleWebProject.Services.Api;

namespace MarbleWebProject.Services.Storefront;

public sealed class StorefrontRuntimeProvider : IStorefrontRuntimeProvider
{
    private const string HttpContextItemKey = "marble:storefront:runtime";

    private readonly IStoreStorefrontApi _storefront;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<StorefrontRuntimeProvider> _logger;

    public StorefrontRuntimeProvider(
        IStoreStorefrontApi storefront,
        IHttpContextAccessor httpContextAccessor,
        ILogger<StorefrontRuntimeProvider> logger)
    {
        _storefront = storefront;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<StorefrontRuntimeConfig> GetAsync(CancellationToken cancellationToken = default)
    {
        var http = _httpContextAccessor.HttpContext;
        if (http?.Items.TryGetValue(HttpContextItemKey, out var cached) == true
            && cached is StorefrontRuntimeConfig cachedRuntime)
        {
            return SyncAppConfig(cachedRuntime);
        }

        var runtime = await StorefrontBootstrap.LoadAsync(_storefront, _logger, cancellationToken);

        if (http != null)
            http.Items[HttpContextItemKey] = runtime;

        return SyncAppConfig(runtime);
    }

    private static StorefrontRuntimeConfig SyncAppConfig(StorefrontRuntimeConfig runtime)
    {
        // Preserve StoreAuth across layout reloads (LanguageCode / credentials live here).
        var auth = AppConfig.Storefront.StoreAuth;
        AppConfig.Storefront = runtime;
        AppConfig.Storefront.StoreAuth = auth;
        return runtime;
    }
}
