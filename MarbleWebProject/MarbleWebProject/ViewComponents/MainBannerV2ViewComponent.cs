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
    private readonly IStoreCatalogApi _catalog;
    private readonly IStoreAuthService _auth;

    public MainBannerV2ViewComponent(
        IMemoryCache cache,
        IStoreContentApi content,
        IStoreCatalogApi catalog,
        IStoreAuthService auth)
    {
        _cache = cache;
        _content = content;
        _catalog = catalog;
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
            }
            else
            {
                cacheHelper.SetCache(key, new List<AllBannerResponse>());
                return Content(string.Empty);
            }
        }

        var returnData = cacheHelper.GetCache(key) as List<AllBannerResponse> ?? new List<AllBannerResponse>();
        var slides = returnData
            .Where(x => x.Status == true && !string.IsNullOrWhiteSpace(x.Image))
            .OrderBy(x => int.TryParse(x.Order, out var order) ? order : x.ID)
            .ToList();

        if (slides.Count == 0)
            return Content(string.Empty);

        var makes = new List<VehicleMakeListItem>();
        var makesKey = "VehicleMakes" + AppConfig.CMSService.CustomName;
        if (!cacheHelper.IfCache(makesKey))
        {
            var makesResp = await _catalog.GetVehicleMakesAsync(cancellationToken);
            if (makesResp.Status && makesResp.Data != null)
                cacheHelper.SetCache(makesKey, makesResp.Data);
            else
                cacheHelper.SetCache(makesKey, new List<VehicleMakeListItem>());
        }

        makes = cacheHelper.GetCache(makesKey) as List<VehicleMakeListItem> ?? new List<VehicleMakeListItem>();

        return View(new MainBannerV2ViewModel
        {
            Slides = slides,
            VehicleMakes = makes,
        });
    }
}
