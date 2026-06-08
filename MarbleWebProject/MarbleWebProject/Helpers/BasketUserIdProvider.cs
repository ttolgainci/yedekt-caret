using MarbleWebProject.Services.Api;

namespace MarbleWebProject.Helpers;

public interface IBasketUserIdProvider
{
    string GetGuestBasketUserId();
    Task<string> ResolveBasketUserIdAsync(CancellationToken cancellationToken = default);
}

public sealed class BasketUserIdProvider : IBasketUserIdProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IStoreCustomerSession _customerSession;

    public BasketUserIdProvider(IHttpContextAccessor httpContextAccessor, IStoreCustomerSession customerSession)
    {
        _httpContextAccessor = httpContextAccessor;
        _customerSession = customerSession;
    }

    public string GetGuestBasketUserId() =>
        _httpContextAccessor.HttpContext?.Request.Cookies["UserIDForBasket"] ?? string.Empty;

    public async Task<string> ResolveBasketUserIdAsync(CancellationToken cancellationToken = default)
    {
        var auth = await _customerSession.GetAsync(cancellationToken);
        if (auth?.Customer?.Id is > 0)
            return auth.Customer.Id.ToString();

        return GetGuestBasketUserId();
    }
}
