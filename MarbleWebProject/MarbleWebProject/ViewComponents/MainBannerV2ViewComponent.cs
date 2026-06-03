using MarbleWebProject.Helper;
using MarbleWebProject.Models;
using MarbleWebProject.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace MarbleWebProject.ViewComponents;

[ViewComponent]
public class MainBannerV2ViewComponent : ViewComponent
{
    private readonly IMemoryCache _cache;
    private readonly IStoreContentApi _content;
    private readonly IStoreAuthService _auth;

    public MainBannerV2ViewComponent(IMemoryCache cache, IStoreContentApi content, IStoreAuthService auth)
    {
        _cache = cache;
        _content = content;
        _auth = auth;
    }

    public async Task<IViewComponentResult> InvokeAsync(List<AllBannerResponse>? models = null, CancellationToken cancellationToken = default)
    {
        var cacheHelper = new CacheHelper(_cache);
        var key = "MainBanner" + AppConfig.CMSService.CustomName + AppConfig.CMSService.LanguageCode;

        if (!cacheHelper.IfCache(key))
        {
            var session = await _auth.GetSessionAsync(cancellationToken);
            var routeResponse = await _content.GetBannerAllAsync(
                new BannerAllRequest { LanguageCode = session.LanguageCode, Type = "MAINSLIDER" },
                cancellationToken);

            if (routeResponse.Status)
            {
                cacheHelper.SetCache(key, routeResponse.Data);
                return View(routeResponse.Data);
            }
            cacheHelper.SetCache(key, new List<AllBannerResponse>());
            return Content(string.Empty);
        }

        var returnData = cacheHelper.GetCache(key) as List<AllBannerResponse>;
        return View(returnData ?? new List<AllBannerResponse>());
    }
}
