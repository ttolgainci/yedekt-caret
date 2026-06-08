using System.Text.Json;
using MarbleWebProject.Models;
using Microsoft.Extensions.Caching.Distributed;

namespace MarbleWebProject.Services.CheckoutDraft;

public sealed class CheckoutDraftStore : ICheckoutDraftStore
{
    private const string KeyPrefix = "checkout:draft:";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IDistributedCache _cache;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CheckoutDraftStore> _logger;

    public CheckoutDraftStore(
        IDistributedCache cache,
        IConfiguration configuration,
        ILogger<CheckoutDraftStore> logger)
    {
        _cache = cache;
        _configuration = configuration;
        _logger = logger;
    }

    private static string BuildKey(string basketUserId) => KeyPrefix + basketUserId.Trim();

    private TimeSpan Ttl() =>
        TimeSpan.FromDays(_configuration.GetValue("CheckoutDraft:TtlDays", 14));

    public async Task<CheckoutDraftModel?> GetAsync(string basketUserId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(basketUserId)) return null;

        try
        {
            var bytes = await _cache.GetAsync(BuildKey(basketUserId), cancellationToken);
            if (bytes == null || bytes.Length == 0) return null;
            return JsonSerializer.Deserialize<CheckoutDraftModel>(bytes, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Checkout draft read failed for {BasketUserId}.", basketUserId);
            return null;
        }
    }

    public async Task SaveAsync(string basketUserId, CheckoutDraftModel draft, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(basketUserId) || draft == null) return;

        try
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(draft, JsonOptions);
            await _cache.SetAsync(
                BuildKey(basketUserId),
                bytes,
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = Ttl() },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Checkout draft write failed for {BasketUserId}.", basketUserId);
        }
    }

    public async Task DeleteAsync(string basketUserId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(basketUserId)) return;

        try
        {
            await _cache.RemoveAsync(BuildKey(basketUserId), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Checkout draft delete failed for {BasketUserId}.", basketUserId);
        }
    }
}
