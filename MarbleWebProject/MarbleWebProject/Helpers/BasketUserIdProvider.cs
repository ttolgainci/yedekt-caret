using MarbleWebProject.Services.Api;

namespace MarbleWebProject.Helpers;

public interface IBasketUserIdProvider
{
    /// <summary>Cookie'deki misafir kimliği (geçersizse boş).</summary>
    string GetGuestBasketUserId();

    /// <summary>Üye ise Customer.Id, değilse GuestToken.</summary>
    Task<string> ResolveBasketUserIdAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Logout sonrası yeni boş misafir bağlamı: eski GuestToken'ı bırakıp yenisini yazar.
    /// Üye sepet/wishlist SQL'de kalır; yeni misafir oturumunda görünmez.
    /// </summary>
    string RotateGuestBasketUserId();
}

public sealed class BasketUserIdProvider : IBasketUserIdProvider
{
    public const string GuestCookieName = "UserIDForBasket";
    private static readonly TimeSpan GuestCookieLifetime = TimeSpan.FromDays(14);

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IStoreCustomerSession _customerSession;

    public BasketUserIdProvider(IHttpContextAccessor httpContextAccessor, IStoreCustomerSession customerSession)
    {
        _httpContextAccessor = httpContextAccessor;
        _customerSession = customerSession;
    }

    public string GetGuestBasketUserId()
    {
        var raw = _httpContextAccessor.HttpContext?.Request.Cookies[GuestCookieName];
        return IsValidGuestToken(raw) ? raw!.Trim() : string.Empty;
    }

    public async Task<string> ResolveBasketUserIdAsync(CancellationToken cancellationToken = default)
    {
        var auth = await _customerSession.GetAsync(cancellationToken);
        if (auth?.Customer?.Id is > 0)
            return auth.Customer.Id.ToString();

        return GetGuestBasketUserId();
    }

    public string RotateGuestBasketUserId()
    {
        var ctx = _httpContextAccessor.HttpContext;
        if (ctx == null)
            return string.Empty;

        var newToken = Guid.NewGuid().ToString("D");
        ctx.Response.Cookies.Append(GuestCookieName, newToken, BuildCookieOptions(ctx));
        return newToken;
    }

    public static CookieOptions BuildCookieOptions(HttpContext ctx) => new()
    {
        HttpOnly = true,
        Secure = ctx.Request.IsHttps,
        SameSite = SameSiteMode.Strict,
        IsEssential = true,
        Expires = DateTimeOffset.UtcNow.Add(GuestCookieLifetime)
    };

    /// <summary>Sadece öngörülemez GUID kabul edilir; manipüle edilmiş değerler yok sayılır.</summary>
    public static bool IsValidGuestToken(string? token) =>
        !string.IsNullOrWhiteSpace(token) && Guid.TryParse(token.Trim(), out _);
}
