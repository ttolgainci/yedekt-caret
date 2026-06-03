using MarbleWebProject.Helper;
using MarbleWebProject.Models;
using MarbleWebProject.Models.Options;
using MarbleWebProject.Services;
using Microsoft.Extensions.Options;

namespace MarbleWebProject.Services.Api;

/// <summary>Uygulama açılışında SEO route listesi (HTTP session olmadan servis hesabı ile).</summary>
public sealed class StoreRouteBootstrap : IStoreRouteBootstrap
{
    private readonly IStoreApiClient _api;
    private readonly StoreAuthOptions _auth;

    public StoreRouteBootstrap(IStoreApiClient api, IOptions<StoreAuthOptions> auth)
    {
        _api = api;
        _auth = auth.Value;
    }

    public async Task<List<RouteListModel>> LoadRouteListAsync(CancellationToken cancellationToken = default)
    {
        var routeList = new List<RouteListModel>();

        var login = await _api.PostAsync<TokenResponse>("/AccountManager/login", new AccountLoginRequest
        {
            UserName = _auth.UserName,
            Password = _auth.Password,
            CustomName = _auth.CustomName
        }, bearerToken: null, cancellationToken);

        if (string.IsNullOrWhiteSpace(login?.Token) || login.Token.Contains("Kullanici", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                "API login failed at startup. Check StoreAuth (or CMSService) in appsettings.");

        if (AppConfig.CMSService != null)
        {
            AppConfig.CMSService.LanguageCode = login.LanguageCode;
            AppConfig.CMSService.LanguageCulture = login.LanguageCulture;
        }

        var token = login.Token;

        var categoryRoutes = await _api.PostAsync<BaseResponse<List<CategoryRouteModel>>>(
            "/Category/GetCategoryRoute", new { }, token, cancellationToken);
        if (categoryRoutes.Status && categoryRoutes.Data != null)
        {
            var index = 1;
            foreach (var item in categoryRoutes.Data)
            {
                routeList.Add(new RouteListModel
                {
                    PageType = "CATEGORY",
                    RouteName = item.Name + " " + index,
                    RouteUrl = item.Url,
                    LanguageCode = item.LanguageCode,
                    ID = item.ID
                });
                index++;
            }
        }

        var productRoutes = await _api.PostAsync<BaseResponse<List<CategoryRouteModel>>>(
            "/Product/GetProductRoute", new { }, token, cancellationToken);
        if (productRoutes.Status && productRoutes.Data != null)
        {
            var index = 1;
            foreach (var item in productRoutes.Data)
            {
                routeList.Add(new RouteListModel
                {
                    PageType = "PRODUCT",
                    RouteName = item.Name + " " + index,
                    RouteUrl = item.Url,
                    LanguageCode = item.LanguageCode,
                    ID = item.ID,
                    CatID = item.CatID
                });
                index++;
            }
        }

        var infoRoutes = await _api.PostAsync<BaseResponse<List<InformationRouteModel>>>(
            "/Information/GetInformationForRoute", new { }, token, cancellationToken);
        if (infoRoutes.Status && infoRoutes.Data != null)
        {
            var index = 1;
            foreach (var item in infoRoutes.Data)
            {
                routeList.Add(new RouteListModel
                {
                    PageType = item.Type,
                    RouteName = item.Name + " " + index,
                    RouteUrl = item.Url,
                    LanguageCode = item.LanguageCode
                });
                index++;
            }
        }

        var translateAll = await _api.PostAsync<BaseResponse<List<TranslateAllResponse>>>(
            "/Language/getTranslateAll", new { }, token, cancellationToken);
        if (translateAll.Status && translateAll.Data != null)
        {
            FilterParametersHelper.TranslateFullList = translateAll.Data.Select(c => new TranslateAllResponse
            {
                Key = c.Key,
                KeyLang = c.KeyLang,
                RetLang = c.RetLang,
                Translation = c.Translation
            }).ToList();
        }

        return routeList;
    }
}
