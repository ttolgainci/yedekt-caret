using MarbleWebProject.Helper;
using MarbleWebProject.Models;
using MarbleWebProject.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace MarbleWebProject.ViewComponents;

[ViewComponent]
public class HomeBannerGroupV1ViewComponent : ViewComponent
{
    private readonly IMemoryCache _cache;
    private readonly IStoreContentApi _content;
    private readonly IStoreAuthService _auth;

    public HomeBannerGroupV1ViewComponent(IMemoryCache cache, IStoreContentApi content, IStoreAuthService auth)
    {
        _cache = cache;
        _content = content;
        _auth = auth;
    }

    public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken = default)
    {
        var cacheHelper = new CacheHelper(_cache);
        var key = "HomeBannerGroup" + AppConfig.CMSService.CustomName + AppConfig.CMSService.LanguageCode;
        List<AllBannerResponse>? banners = null;

        if (cacheHelper.IfCache(key))
            banners = cacheHelper.GetCache(key) as List<AllBannerResponse>;

        if (banners == null)
        {
            var session = await _auth.GetSessionAsync(cancellationToken);
            var response = await _content.GetBannerAllAsync(
                new BannerAllRequest { LanguageCode = session.LanguageCode, Type = "BANNERGROUP" },
                cancellationToken);
            banners = response.Status ? response.Data : new List<AllBannerResponse>();
            cacheHelper.SetCache(key, banners);
        }

        return View(banners ?? new List<AllBannerResponse>());
    }
}
