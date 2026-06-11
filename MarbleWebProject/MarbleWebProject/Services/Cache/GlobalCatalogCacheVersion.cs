using StackExchange.Redis;

namespace MarbleWebProject.Services.Cache;

public sealed class GlobalCatalogCacheVersion : IGlobalCatalogCacheVersion
{
    private const string KeyPrefix = "marble:global:catalog-version:";
    private readonly IConnectionMultiplexer? _multiplexer;
    private readonly ILogger<GlobalCatalogCacheVersion> _logger;

    public GlobalCatalogCacheVersion(
        IServiceProvider serviceProvider,
        ILogger<GlobalCatalogCacheVersion> logger)
    {
        _multiplexer = serviceProvider.GetService<IConnectionMultiplexer>();
        _logger = logger;
    }

    public int GetCurrent(string? tenant = null)
    {
        if (_multiplexer == null || !_multiplexer.IsConnected)
            return 0;

        try
        {
            var key = KeyPrefix + (string.IsNullOrWhiteSpace(tenant) ? "default" : tenant.Trim());
            var value = _multiplexer.GetDatabase().StringGet(key);
            return value.HasValue && value.TryParse(out long parsed) ? (int)parsed : 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Global catalog version read failed on Web.");
            return 0;
        }
    }
}
