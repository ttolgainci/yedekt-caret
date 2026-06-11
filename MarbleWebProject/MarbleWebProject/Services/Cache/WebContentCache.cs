using System.Text.Json;
using MarbleWebProject.Models;
using Microsoft.Extensions.Caching.Distributed;

namespace MarbleWebProject.Services.Cache;

public sealed class WebContentCache : IWebContentCache
{
    private const string FooterPrefix = "web:content:footer:";
    private const string CheckoutLegalPrefix = "web:content:checkout-legal:";
    private static readonly JsonSerializerOptions JsonOptions = new();
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(30);

    private readonly IDistributedCache _cache;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WebContentCache> _logger;

    public WebContentCache(IDistributedCache cache, IConfiguration configuration, ILogger<WebContentCache> logger)
    {
        _cache = cache;
        _configuration = configuration;
        _logger = logger;
    }

    public bool IsEnabled() =>
        _configuration.GetValue("ContentCache:Enabled", true)
        && !string.IsNullOrWhiteSpace(
            _configuration.GetConnectionString("Redis")
            ?? _configuration["Redis:ConnectionString"]);

    public async Task<StorefrontFooterLinksModel?> TryGetFooterLinksAsync(
        string tenant, string languageCode, int version, CancellationToken cancellationToken = default)
    {
        if (!IsEnabled())
            return null;

        try
        {
            var bytes = await _cache.GetAsync(FooterPrefix + tenant + ":" + languageCode + ":" + version, cancellationToken);
            if (bytes == null || bytes.Length == 0)
                return null;
            return JsonSerializer.Deserialize<StorefrontFooterLinksModel>(bytes, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Web footer cache read failed.");
            return null;
        }
    }

    public async Task SetFooterLinksAsync(
        string tenant, string languageCode, int version, StorefrontFooterLinksModel data, CancellationToken cancellationToken = default)
    {
        if (!IsEnabled())
            return;

        try
        {
            await _cache.SetAsync(
                FooterPrefix + tenant + ":" + languageCode + ":" + version,
                JsonSerializer.SerializeToUtf8Bytes(data, JsonOptions),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = Ttl },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Web footer cache write failed.");
        }
    }

    public async Task<StorefrontCheckoutLegalModel?> TryGetCheckoutLegalAsync(
        string tenant, string languageCode, int version, CancellationToken cancellationToken = default)
    {
        if (!IsEnabled())
            return null;

        try
        {
            var bytes = await _cache.GetAsync(CheckoutLegalPrefix + tenant + ":" + languageCode + ":" + version, cancellationToken);
            if (bytes == null || bytes.Length == 0)
                return null;
            return JsonSerializer.Deserialize<StorefrontCheckoutLegalModel>(bytes, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Web checkout legal cache read failed.");
            return null;
        }
    }

    public async Task SetCheckoutLegalAsync(
        string tenant, string languageCode, int version, StorefrontCheckoutLegalModel data, CancellationToken cancellationToken = default)
    {
        if (!IsEnabled())
            return;

        try
        {
            await _cache.SetAsync(
                CheckoutLegalPrefix + tenant + ":" + languageCode + ":" + version,
                JsonSerializer.SerializeToUtf8Bytes(data, JsonOptions),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = Ttl },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Web checkout legal cache write failed.");
        }
    }
}
