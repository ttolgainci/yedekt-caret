using System.Text.Json;
using MarbleWebProject.Helper;
using MarbleWebProject.Models;
using MarbleWebProject.Models.Options;
using Microsoft.Extensions.Options;

namespace MarbleWebProject.Services.Api;

public sealed class StoreAuthService : IStoreAuthService
{
    private const string SessionKey = "CmsApiToken";

    private readonly IStoreApiClient _api;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly StoreAuthOptions _auth;

    public StoreAuthService(
        IStoreApiClient api,
        IHttpContextAccessor httpContextAccessor,
        IOptions<StoreAuthOptions> auth)
    {
        _api = api;
        _httpContextAccessor = httpContextAccessor;
        _auth = auth.Value;
    }

    public async Task<TokenResponse> GetSessionAsync(CancellationToken cancellationToken = default)
    {
        var cached = ReadFromSession();
        if (IsValidToken(cached))
        {
            ApplyLanguageToAppConfig(cached!);
            return cached!;
        }

        var login = await LoginAsync(cancellationToken);
        WriteToSession(login);
        ApplyLanguageToAppConfig(login);
        return login;
    }

    public async Task<string> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        var session = await GetSessionAsync(cancellationToken);
        return session.Token ?? "";
    }

    internal async Task<TokenResponse> LoginWithCredentialsAsync(
        string userName,
        string password,
        string customName,
        CancellationToken cancellationToken = default)
    {
        var body = new AccountLoginRequest
        {
            UserName = userName,
            Password = password,
            CustomName = customName,
            Audience = "store"
        };
        var response = await _api.PostAsync<TokenResponse>("/AccountManager/login", body, bearerToken: null, cancellationToken);
        if (!IsValidToken(response))
            throw new InvalidOperationException(
                "API login failed. Check StoreAuth UserName/Password/CustomName in appsettings.");
        return response;
    }

    private async Task<TokenResponse> LoginAsync(CancellationToken cancellationToken)
    {
        return await LoginWithCredentialsAsync(
            _auth.UserName ?? "",
            _auth.Password ?? "",
            _auth.CustomName ?? "",
            cancellationToken);
    }

    private static bool IsValidToken(TokenResponse? token) =>
        !string.IsNullOrWhiteSpace(token?.Token)
        && !token.Token.Contains("Kullanici", StringComparison.OrdinalIgnoreCase)
        && LooksLikeJwt(token.Token);

    private static bool LooksLikeJwt(string token)
    {
        var parts = token.Split('.');
        return parts.Length == 3 && parts.All(p => p.Length > 0);
    }

    private TokenResponse? ReadFromSession()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Session == null)
            return null;

        var json = httpContext.Session.GetString(SessionKey);
        if (string.IsNullOrEmpty(json))
            return null;

        return JsonSerializer.Deserialize<TokenResponse>(json);
    }

    private void WriteToSession(TokenResponse token)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Session == null)
            return;

        httpContext.Session.SetString(SessionKey, JsonSerializer.Serialize(token));
    }

    private static void ApplyLanguageToAppConfig(TokenResponse token)
    {
        AppConfig.Storefront.StoreAuth.LanguageCode = token.LanguageCode;
        AppConfig.Storefront.StoreAuth.LanguageCulture = token.LanguageCulture;
    }
}
