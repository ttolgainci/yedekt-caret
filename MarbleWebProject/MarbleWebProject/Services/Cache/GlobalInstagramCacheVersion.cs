using StackExchange.Redis;

namespace MarbleWebProject.Services.Cache;

public sealed class GlobalInstagramCacheVersion : IGlobalInstagramCacheVersion
{
    private const string KeyPrefix = "marble:global:instagram-version:";
    private int _localFallback;
    private readonly IConnectionMultiplexer? _multiplexer;
    private readonly ILogger<GlobalInstagramCacheVersion> _logger;

    public GlobalInstagramCacheVersion(IServiceProvider serviceProvider, ILogger<GlobalInstagramCacheVersion> logger)
    {
        _multiplexer = serviceProvider.GetService<IConnectionMultiplexer>();
        _logger = logger;
    }

    public int GetCurrent(string? tenant = null)
    {
        if (_multiplexer == null || !_multiplexer.IsConnected)
            return _localFallback;

        try
        {
            var value = _multiplexer.GetDatabase().StringGet(KeyPrefix + NormalizeTenant(tenant));
            return value.HasValue && value.TryParse(out long parsed) ? (int)parsed : 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Global Instagram version read failed.");
            return _localFallback;
        }
    }

    private static string NormalizeTenant(string? tenant) =>
        string.IsNullOrWhiteSpace(tenant) ? "default" : tenant.Trim();
}
