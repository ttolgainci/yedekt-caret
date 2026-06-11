using StackExchange.Redis;

namespace MarbleWebProject.Services.Cache;

public sealed class GlobalContentCacheVersion : IGlobalContentCacheVersion
{
    private const string KeyPrefix = "marble:global:content-version:";
    private int _localFallback;
    private readonly IConnectionMultiplexer? _multiplexer;
    private readonly ILogger<GlobalContentCacheVersion> _logger;

    public GlobalContentCacheVersion(IServiceProvider serviceProvider, ILogger<GlobalContentCacheVersion> logger)
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
            _logger.LogWarning(ex, "Global content version read failed.");
            return _localFallback;
        }
    }

    private static string NormalizeTenant(string? tenant) =>
        string.IsNullOrWhiteSpace(tenant) ? "default" : tenant.Trim();
}
