using System.Text.Json;
using MarbleWebProject.Models;
using Microsoft.Extensions.Caching.Distributed;

namespace MarbleWebProject.Services.Cache;

public sealed class WebInstagramCache : IWebInstagramCache
{
    private const string KeyPrefix = "web:instagram:feed:";
    private static readonly JsonSerializerOptions JsonOptions = new();
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(30);

    private readonly IDistributedCache _cache;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WebInstagramCache> _logger;

    public WebInstagramCache(IDistributedCache cache, IConfiguration configuration, ILogger<WebInstagramCache> logger)
    {
        _cache = cache;
        _configuration = configuration;
        _logger = logger;
    }

    public bool IsEnabled() =>
        _configuration.GetValue("InstagramCache:Enabled", true)
        && !string.IsNullOrWhiteSpace(
            _configuration.GetConnectionString("Redis")
            ?? _configuration["Redis:ConnectionString"]);

    private static string BuildKey(string tenant, string scopeKey, int limit, int version) =>
        KeyPrefix + tenant.Trim() + ":" + scopeKey + ":" + limit + ":" + version;

    public async Task<StorefrontInstagramFeedModel?> TryGetFeedAsync(
        string tenant, string scopeKey, int limit, int version, CancellationToken cancellationToken = default)
    {
        if (!IsEnabled())
            return null;

        try
        {
            var bytes = await _cache.GetAsync(BuildKey(tenant, scopeKey, limit, version), cancellationToken);
            if (bytes == null || bytes.Length == 0)
                return null;
            return JsonSerializer.Deserialize<StorefrontInstagramFeedModel>(bytes, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Web Instagram cache read failed.");
            return null;
        }
    }

    public async Task SetFeedAsync(
        string tenant, string scopeKey, int limit, int version, StorefrontInstagramFeedModel feed, CancellationToken cancellationToken = default)
    {
        if (!IsEnabled())
            return;

        try
        {
            await _cache.SetAsync(
                BuildKey(tenant, scopeKey, limit, version),
                JsonSerializer.SerializeToUtf8Bytes(feed, JsonOptions),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = Ttl },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Web Instagram cache write failed.");
        }
    }
}
