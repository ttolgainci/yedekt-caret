using System.Text.Json;
using MarbleWebProject.Models;
using Microsoft.Extensions.Caching.Distributed;

namespace MarbleWebProject.Services.Cache;

public sealed class WebCatalogCache : IWebCatalogCache
{
    private const string KeyPrefix = "web:categories:header:";
    private static readonly JsonSerializerOptions JsonOptions = new();
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(15);

    private readonly IDistributedCache _cache;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WebCatalogCache> _logger;

    public WebCatalogCache(
        IDistributedCache cache,
        IConfiguration configuration,
        ILogger<WebCatalogCache> logger)
    {
        _cache = cache;
        _configuration = configuration;
        _logger = logger;
    }

    public bool IsEnabled() =>
        _configuration.GetValue("CatalogCache:Enabled", true)
        && !string.IsNullOrWhiteSpace(
            _configuration.GetConnectionString("Redis")
            ?? _configuration["Redis:ConnectionString"]);

    private static string BuildKey(string tenant, string languageCode, int catalogVersion) =>
        KeyPrefix + tenant.Trim() + ":" + languageCode.Trim() + ":" + catalogVersion;

    public async Task<List<CategoryListModel>?> TryGetHeaderCategoriesAsync(
        string tenant,
        string languageCode,
        int catalogVersion,
        CancellationToken cancellationToken = default)
    {
        if (!IsEnabled())
            return null;

        try
        {
            var bytes = await _cache.GetAsync(BuildKey(tenant, languageCode, catalogVersion), cancellationToken);
            if (bytes == null || bytes.Length == 0)
                return null;

            return JsonSerializer.Deserialize<List<CategoryListModel>>(bytes, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Web catalog cache read failed.");
            return null;
        }
    }

    public async Task SetHeaderCategoriesAsync(
        string tenant,
        string languageCode,
        int catalogVersion,
        List<CategoryListModel> categories,
        CancellationToken cancellationToken = default)
    {
        if (!IsEnabled())
            return;

        try
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(categories, JsonOptions);
            await _cache.SetAsync(
                BuildKey(tenant, languageCode, catalogVersion),
                bytes,
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = Ttl },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Web catalog cache write failed.");
        }
    }
}
